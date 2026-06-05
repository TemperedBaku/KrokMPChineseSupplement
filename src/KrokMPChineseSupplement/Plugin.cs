using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace KrokMPChineseSupplement
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "casualtiesunknown.krokmpchinesesupplement";
        public const string PluginName = "KrokMP Chinese Supplement";
        public const string PluginVersion = "0.1.23";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> EnableTextSetters;
        internal static ConfigEntry<bool> EnableImGuiPatch;
        internal static ConfigEntry<bool> EnableAlertPatch;
        internal static ConfigEntry<bool> EnableConsolePatch;
        internal static ConfigEntry<bool> EnableKrokLangPatch;
        internal static ConfigEntry<bool> EnableLocaleInjection;
        internal static ConfigEntry<bool> EnableChineseRuleSearch;
        internal static ConfigEntry<bool> EnableKrokSpecificStringPatch;
        internal static ConfigEntry<bool> EnableFocusableButtonPatch;
        internal static ConfigEntry<bool> EnableImGuiInGameplay;
        internal static ConfigEntry<bool> EnableSafeGameplayGuiTranslation;
        internal static ConfigEntry<bool> EnableSteamDisplayNames;
        internal static ConfigEntry<int> MaxSteamDisplayNameChars;
        internal static ConfigEntry<int> NameTagMaxSteamDisplayNameChars;
        internal static ConfigEntry<bool> AppendEllipsisWhenTruncated;
        internal static ConfigEntry<bool> VerboseSteamDisplayNameLogging;
        internal static ConfigEntry<bool> VerboseLogging;

        private readonly List<FieldInfo> _ruleSearchFields = new List<FieldInfo>();
        private float _nextRuleSearchScan;
        private float _nextRuleSearchTranslate;

        private bool _krokSpecificStringPatchDone;
        private bool _chineseSearchPatchDone;
        private float _nextKrokSpecificPatchAttempt;
        private float _nextChineseSearchPatchAttempt;
        private int _krokSpecificPatchAttempts;
        private int _chineseSearchPatchAttempts;

        private bool _krokLangPatchDone;
        private float _nextKrokLangPatchAttempt;
        private int _krokLangPatchAttempts;
        private float _nextLocaleInjectAttempt;
        private object _lastLocaleObject;

        private Harmony _harmony;

        private static bool _tooltipFieldsResolved;
        private static FieldInfo _tooltipNameField;
        private static FieldInfo _tooltipDescField;

        private void Awake()
        {
            Log = Logger;
            EnableTextSetters = Config.Bind("Patch", "EnableTextSetters", true,
                "Patch Unity UI.Text and TMP_Text text setters. Safe exact/known-text translation only.");
            EnableImGuiPatch = Config.Bind("Patch", "EnableImGuiPatch", true,
                "Patch common GUI/GUILayout label/button/toggle/box calls used by KrokMP menus.");
            EnableAlertPatch = Config.Bind("Patch", "EnableAlertPatch", true,
                "Patch PlayerCamera DoAlert-style methods. This catches many multiplayer denial/status popups.");
            EnableConsolePatch = Config.Bind("Patch", "EnableConsolePatch", true,
                "Patch ConsoleScript.LogToConsole-style methods. Mostly affects admin/debug/server feedback text.");
            EnableKrokLangPatch = Config.Bind("Patch", "EnableKrokLangPatch", false,
                "Patch KrokoshaCasualtiesMP.Lang.Get after KrokMP loads. Disabled by default because KrokMP 3.0.0 can reject this patch on some builds; Text/TMP/GUI fallback remains active.");
            EnableLocaleInjection = Config.Bind("Patch", "EnableLocaleInjection", false,
                "Inject krokosha_coop_* translations into Locale.currentLang.other if the game locale object exposes it. Disabled by default for compatibility.");
            EnableChineseRuleSearch = Config.Bind("Patch", "EnableChineseRuleSearch", true,
                "Patch the KrokMP rule-search comparison so Chinese search text can match translated rule labels without rewriting the search box input.");
            EnableKrokSpecificStringPatch = Config.Bind("Patch", "EnableKrokSpecificStringPatch", true,
                "Patch KrokMP-specific string sinks such as UIBullshit._GUI_SetTooltip and DoMultiplayerStatusMessageLog.");
            EnableFocusableButtonPatch = Config.Bind("Patch", "EnableFocusableButtonPatch", false,
                "Patch KrokMP UIBullshit._GUI_FocusableButton directly. Disabled by default as a lag-safety guard; UIInGame.DoPlayerInteractionMenuButton remains patched for player interaction menu text.");
            EnableImGuiInGameplay = Config.Bind("Performance", "EnableImGuiInGameplay", false,
                "Translate generic IMGUI text while the active scene is SampleScene. Disabled by default because KrokMP draws high-frequency gameplay overlays with IMGUI; enable only if you want in-game overlay text translated and your FPS remains acceptable.");
            EnableSafeGameplayGuiTranslation = Config.Bind("Performance", "EnableSafeGameplayGuiTranslation", true,
                "When EnableImGuiInGameplay is false, still translate low-cost exact/prefix KrokMP GUI strings in gameplay. This fixes in-game KrokMP menu fallback without re-enabling expensive full phrase/regex IMGUI translation.");
            EnableSteamDisplayNames = Config.Bind("SteamName", "EnableSteamDisplayNames", false,
                "Removed/disabled by default. The Steam persona display-name replacement caused nametag overflow in KrokMP UI, so v0.1.22 no longer performs this replacement.");
            MaxSteamDisplayNameChars = Config.Bind("SteamName", "MaxSteamDisplayNameChars", 8,
                "Maximum number of visible text elements used for general Steam display-name replacement. Set to 0 or a negative number to disable truncation. This is display-only and does not affect KrokMP internal usernames or network packets.");
            NameTagMaxSteamDisplayNameChars = Config.Bind("SteamName", "NameTagMaxSteamDisplayNameChars", 6,
                "Maximum number of visible text elements used when replacing names inside [name] style nametags. Lower this if the right bracket is pushed out of the nametag box.");
            AppendEllipsisWhenTruncated = Config.Bind("SteamName", "AppendEllipsisWhenTruncated", false,
                "Append an ellipsis when Steam display names are shortened by MaxSteamDisplayNameChars.");
            VerboseSteamDisplayNameLogging = Config.Bind("SteamName", "VerboseSteamDisplayNameLogging", false,
                "Log Steam display-name mappings when they are discovered. Leave false for normal use.");
            VerboseLogging = Config.Bind("Diagnostics", "VerboseLogging", false,
                "Log every translated string. Leave false for normal use.");

            Translator.Load(Path.GetDirectoryName(Info.Location), Log);
            DisplayNameResolver.Reset();

            _harmony = new Harmony(PluginGuid);
            int total = 0;

            if (EnableTextSetters.Value) total += PatchTextSetters();
            if (EnableImGuiPatch.Value) total += PatchImGui();
            if (EnableAlertPatch.Value) total += PatchAlertMethods();
            if (EnableConsolePatch.Value) total += PatchConsoleMethods();
            if (EnableKrokSpecificStringPatch.Value)
            {
                int c = PatchKrokSpecificStringMethods();
                total += c;
                if (c > 0) _krokSpecificStringPatchDone = true;
            }
            if (EnableChineseRuleSearch.Value)
            {
                int c = PatchChineseSearchMethods();
                total += c;
                if (c > 0) _chineseSearchPatchDone = true;
            }

            Log.LogInfo($"{PluginName} {PluginVersion} loaded. Translations={Translator.ExactCount}; phraseRules={Translator.PhraseCount}; placeholderRules={Translator.PlaceholderCount}; patchedMethods={total}; krokLangEntries={Translator.KrokLangEntryCount}");
        }

        private void Update()
        {
            if (EnableKrokLangPatch != null && EnableKrokLangPatch.Value && !_krokLangPatchDone)
            {
                if (Time.unscaledTime >= _nextKrokLangPatchAttempt)
                {
                    _nextKrokLangPatchAttempt = Time.unscaledTime + 0.5f;
                    _krokLangPatchAttempts++;
                    if (TryPatchKrokLangGet())
                    {
                        _krokLangPatchDone = true;
                    }
                    else if (_krokLangPatchAttempts == 1 || _krokLangPatchAttempts == 20 || _krokLangPatchAttempts == 60)
                    {
                        Log.LogInfo($"Waiting for KrokoshaCasualtiesMP.Lang to load; attempts={_krokLangPatchAttempts}");
                    }
                }
            }

            if (EnableLocaleInjection != null && EnableLocaleInjection.Value)
            {
                if (Time.unscaledTime >= _nextLocaleInjectAttempt)
                {
                    _nextLocaleInjectAttempt = Time.unscaledTime + 1.0f;
                    TryInjectLocaleOther();
                }
            }

            if (EnableKrokSpecificStringPatch != null && EnableKrokSpecificStringPatch.Value && !_krokSpecificStringPatchDone)
            {
                if (Time.unscaledTime >= _nextKrokSpecificPatchAttempt)
                {
                    _nextKrokSpecificPatchAttempt = Time.unscaledTime + 0.75f;
                    _krokSpecificPatchAttempts++;
                    int c = PatchKrokSpecificStringMethods();
                    if (c > 0)
                    {
                        _krokSpecificStringPatchDone = true;
                        Log.LogInfo($"Delayed KrokMP-specific string patches applied: {c}; attempts={_krokSpecificPatchAttempts}");
                    }
                }
            }

            if (EnableChineseRuleSearch != null && EnableChineseRuleSearch.Value && !_chineseSearchPatchDone)
            {
                if (Time.unscaledTime >= _nextChineseSearchPatchAttempt)
                {
                    _nextChineseSearchPatchAttempt = Time.unscaledTime + 0.75f;
                    _chineseSearchPatchAttempts++;
                    int c = PatchChineseSearchMethods();
                    if (c > 0)
                    {
                        _chineseSearchPatchDone = true;
                        Log.LogInfo($"Delayed Chinese rule/search patches applied: {c}; attempts={_chineseSearchPatchAttempts}");
                    }
                }
            }

        }

        private void OnDestroy()
        {
            try { _harmony?.UnpatchSelf(); } catch { }
        }


        private void TryTranslateRuleSearchField()
        {
            try
            {
                if (Time.unscaledTime >= _nextRuleSearchScan || _ruleSearchFields.Count == 0)
                {
                    _nextRuleSearchScan = Time.unscaledTime + 2.0f;
                    if (_ruleSearchFields.Count == 0)
                    {
                        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            Type[] types;
                            try { types = asm.GetTypes(); } catch { continue; }
                            foreach (var t in types)
                            {
                                FieldInfo[] fields;
                                try { fields = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic); } catch { continue; }
                                foreach (var f in fields)
                                {
                                    if (f.FieldType != typeof(string)) continue;
                                    var name = f.Name ?? string.Empty;
                                    if (name.IndexOf("RenderRuleField_search", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        name.Equals("_GUI____RenderRuleField_search", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (!_ruleSearchFields.Contains(f)) _ruleSearchFields.Add(f);
                                    }
                                }
                            }
                        }
                        if (_ruleSearchFields.Count > 0 && VerboseLogging != null && VerboseLogging.Value)
                            Log.LogInfo($"Chinese rule search helper found {_ruleSearchFields.Count} KrokMP search field(s).");
                    }
                }

                if (_ruleSearchFields.Count == 0) return;
                if (Time.unscaledTime < _nextRuleSearchTranslate) return;
                _nextRuleSearchTranslate = Time.unscaledTime + 0.15f;

                foreach (var f in _ruleSearchFields)
                {
                    string current = null;
                    try { current = f.GetValue(null) as string; } catch { continue; }
                    if (Translator.TryTranslateRuleSearchQuery(current, out var mapped))
                    {
                        try { f.SetValue(null, mapped); } catch { }
                    }
                }
            }
            catch { }
        }


        private bool TryPatchKrokLangGet()
        {
            try
            {
                var langType = AccessTools.TypeByName("KrokoshaCasualtiesMP.Lang");
                if (langType == null) return false;
                var get = AccessTools.Method(langType, "Get");
                if (get == null)
                {
                    Log.LogWarning("KrokoshaCasualtiesMP.Lang type found but Get method was not found.");
                    return false;
                }
                _harmony.Patch(get, prefix: new HarmonyMethod(typeof(Plugin), nameof(KrokLangGetPrefix)));
                Log.LogInfo("Patched KrokoshaCasualtiesMP.Lang.Get for direct zh-CN fallback.");
                TryInjectLocaleOther(forceLog: true);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWarning("Failed to patch KrokoshaCasualtiesMP.Lang.Get: " + ex.Message);
                return false;
            }
        }

        private void TryInjectLocaleOther(bool forceLog = false)
        {
            try
            {
                var localeType = AccessTools.TypeByName("Locale");
                if (localeType == null) return;
                object currentLang = null;
                var f = AccessTools.Field(localeType, "currentLang");
                if (f != null) currentLang = f.GetValue(null);
                if (currentLang == null)
                {
                    var p = AccessTools.Property(localeType, "currentLang");
                    if (p != null) currentLang = p.GetValue(null, null);
                }
                if (currentLang == null) return;

                var langType = currentLang.GetType();
                object other = null;
                var of = AccessTools.Field(langType, "other");
                if (of != null) other = of.GetValue(currentLang);
                if (other == null)
                {
                    var op = AccessTools.Property(langType, "other");
                    if (op != null) other = op.GetValue(currentLang, null);
                }
                if (other == null) return;

                int changed = Translator.InjectKrokLangEntriesIntoDictionaryLikeObject(other);
                if (changed > 0 || forceLog || !object.ReferenceEquals(_lastLocaleObject, currentLang))
                {
                    Log.LogInfo($"Injected KrokMP zh-CN locale entries into Locale.currentLang.other: changed={changed}; totalKrokLangEntries={Translator.KrokLangEntryCount}");
                }
                _lastLocaleObject = currentLang;
            }
            catch (Exception ex)
            {
                if (VerboseLogging != null && VerboseLogging.Value)
                    Log.LogWarning("Locale.currentLang.other injection failed: " + ex.Message);
            }
        }

        private int PatchTextSetters()
        {
            int count = 0;
            try
            {
                var uiTextSetter = AccessTools.PropertySetter(typeof(Text), "text");
                if (uiTextSetter != null)
                {
                    _harmony.Patch(uiTextSetter, prefix: new HarmonyMethod(typeof(Plugin), nameof(TextSetterPrefix)));
                    count++;
                    Log.LogInfo("Patched UnityEngine.UI.Text.text setter.");
                }
            }
            catch (Exception ex) { Log.LogWarning("Failed to patch UI.Text setter: " + ex.Message); }

            try
            {
                var tmpTextType = AccessTools.TypeByName("TMPro.TMP_Text");
                var tmpSetter = tmpTextType == null ? null : AccessTools.PropertySetter(tmpTextType, "text");
                if (tmpSetter != null)
                {
                    _harmony.Patch(tmpSetter, prefix: new HarmonyMethod(typeof(Plugin), nameof(TextSetterPrefix)));
                    count++;
                    Log.LogInfo("Patched TMPro.TMP_Text.text setter.");
                }
                else
                {
                    Log.LogInfo("TMP_Text setter not found at Awake. This is usually fine if TMP is not loaded yet.");
                }
            }
            catch (Exception ex) { Log.LogWarning("Failed to patch TMP_Text setter: " + ex.Message); }
            return count;
        }

        private int PatchImGui()
        {
            int count = 0;
            count += PatchStaticGuiMethods(typeof(GUI), new HashSet<string>(StringComparer.Ordinal)
            {
                "Label", "Button", "Toggle", "Box", "Toolbar", "SelectionGrid"
            });
            count += PatchStaticGuiMethods(typeof(GUILayout), new HashSet<string>(StringComparer.Ordinal)
            {
                "Label", "Button", "Toggle", "Box", "Toolbar", "SelectionGrid"
            });
            Log.LogInfo($"Patched GUI/GUILayout text methods: {count}");
            return count;
        }

        private int PatchStaticGuiMethods(Type type, HashSet<string> methodNames)
        {
            int count = 0;
            try
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (!methodNames.Contains(method.Name)) continue;
                    if (method.IsGenericMethod || method.ContainsGenericParameters) continue;
                    var ps = method.GetParameters();
                    bool hasTranslatableArg = ps.Any(p => p.ParameterType == typeof(string) || p.ParameterType == typeof(GUIContent) || p.ParameterType == typeof(string[]) || p.ParameterType == typeof(GUIContent[]));
                    if (!hasTranslatableArg) continue;
                    try
                    {
                        _harmony.Patch(method, prefix: new HarmonyMethod(typeof(Plugin), nameof(TranslateArgsPrefix)));
                        count++;
                    }
                    catch { }
                }
            }
            catch (Exception ex) { Log.LogWarning($"Failed to patch {type.FullName} methods: {ex.Message}"); }
            return count;
        }

        private int PatchAlertMethods()
        {
            int count = 0;
            try
            {
                var playerCamera = AccessTools.TypeByName("PlayerCamera");
                if (playerCamera == null)
                {
                    Log.LogWarning("PlayerCamera type not found; alert patch skipped.");
                    return 0;
                }

                foreach (var method in AccessTools.GetDeclaredMethods(playerCamera))
                {
                    if (method == null || method.IsGenericMethod || method.ContainsGenericParameters) continue;
                    if (method.Name.IndexOf("DoAlert", StringComparison.OrdinalIgnoreCase) < 0 &&
                        method.Name.IndexOf("Alert", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    if (!method.GetParameters().Any(p => p.ParameterType == typeof(string))) continue;
                    try
                    {
                        _harmony.Patch(method, prefix: new HarmonyMethod(typeof(Plugin), nameof(TranslateFirstStringArgPrefix)));
                        count++;
                    }
                    catch { }
                }
                Log.LogInfo($"Patched PlayerCamera alert methods: {count}");
            }
            catch (Exception ex) { Log.LogWarning("Failed to patch alert methods: " + ex.Message); }
            return count;
        }

        private int PatchConsoleMethods()
        {
            int count = 0;
            try
            {
                var consoleScript = AccessTools.TypeByName("ConsoleScript");
                if (consoleScript == null)
                {
                    Log.LogWarning("ConsoleScript type not found; console text patch skipped.");
                    return 0;
                }

                foreach (var method in AccessTools.GetDeclaredMethods(consoleScript))
                {
                    if (method == null || method.IsGenericMethod || method.ContainsGenericParameters) continue;
                    if (method.Name.IndexOf("Log", StringComparison.OrdinalIgnoreCase) < 0 &&
                        method.Name.IndexOf("Console", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    if (!method.GetParameters().Any(p => p.ParameterType == typeof(string))) continue;
                    try
                    {
                        _harmony.Patch(method, prefix: new HarmonyMethod(typeof(Plugin), nameof(TranslateFirstStringArgPrefix)));
                        count++;
                    }
                    catch { }
                }
                Log.LogInfo($"Patched ConsoleScript console methods: {count}");
            }
            catch (Exception ex) { Log.LogWarning("Failed to patch console methods: " + ex.Message); }
            return count;
        }


        private int PatchKrokSpecificStringMethods()
        {
            int count = 0;
            try
            {
                var uiBullshit = AccessTools.TypeByName("KrokoshaCasualtiesMP.UIBullshit") ?? AccessTools.TypeByName("UIBullshit");
                var setTooltip = uiBullshit == null ? null : AccessTools.Method(uiBullshit, "_GUI_SetTooltip");
                if (setTooltip != null)
                {
                    _harmony.Patch(setTooltip, prefix: new HarmonyMethod(typeof(Plugin), nameof(TranslateAllStringArgsPrefix)));
                    count++;
                    Log.LogInfo("Patched UIBullshit._GUI_SetTooltip.");
                }
                var doTooltip = uiBullshit == null ? null : AccessTools.Method(uiBullshit, "__GUI__DoTooltipBullshit");
                if (doTooltip != null)
                {
                    _harmony.Patch(doTooltip, prefix: new HarmonyMethod(typeof(Plugin), nameof(TranslateTooltipStaticFieldsPrefix)));
                    count++;
                    Log.LogInfo("Patched UIBullshit.__GUI__DoTooltipBullshit static-field translator.");
                }
            }
            catch (Exception ex) { Log.LogWarning("Failed to patch KrokMP tooltip method: " + ex.Message); }

            try
            {
                var krok = AccessTools.TypeByName("KrokoshaCasualtiesMP.KrokoshaScavMultiplayer") ?? AccessTools.TypeByName("KrokoshaScavMultiplayer");
                var status = krok == null ? null : AccessTools.Method(krok, "DoMultiplayerStatusMessageLog");
                if (status != null)
                {
                    _harmony.Patch(status, prefix: new HarmonyMethod(typeof(Plugin), nameof(TranslateAllStringArgsPrefix)));
                    count++;
                    Log.LogInfo("Patched KrokoshaScavMultiplayer.DoMultiplayerStatusMessageLog.");
                }
                var statusError = krok == null ? null : AccessTools.Method(krok, "DoMultiplayerStatusMessageError");
                if (statusError != null)
                {
                    _harmony.Patch(statusError, prefix: new HarmonyMethod(typeof(Plugin), nameof(TranslateAllStringArgsPrefix)));
                    count++;
                    Log.LogInfo("Patched KrokoshaScavMultiplayer.DoMultiplayerStatusMessageError.");
                }
            }
            catch (Exception ex) { Log.LogWarning("Failed to patch KrokMP status-message method: " + ex.Message); }

            try
            {
                var chat = AccessTools.TypeByName("KrokoshaCasualtiesMP.Chat") ?? AccessTools.TypeByName("Chat");
                if (chat != null)
                {
                    foreach (var m in AccessTools.GetDeclaredMethods(chat))
                    {
                        if (m == null || m.IsGenericMethod || m.ContainsGenericParameters) continue;
                        if (m.Name == "Server_ChatAnnouncement")
                        {
                            _harmony.Patch(m, prefix: new HarmonyMethod(typeof(Plugin), nameof(TranslateAllStringArgsPrefix)));
                            count++;
                        }
                        else if (m.Name == "LogMessage")
                        {
                            _harmony.Patch(m, prefix: new HarmonyMethod(typeof(Plugin), nameof(ChatLogMessagePrefix)));
                            count++;
                        }
                    }
                    Log.LogInfo("Patched Chat server-announcement/log message sinks.");
                }
            }
            catch (Exception ex) { Log.LogWarning("Failed to patch KrokMP chat message methods: " + ex.Message); }

            if (EnableFocusableButtonPatch != null && EnableFocusableButtonPatch.Value)
            {
                try
                {
                    var uiBullshit2 = AccessTools.TypeByName("KrokoshaCasualtiesMP.UIBullshit") ?? AccessTools.TypeByName("UIBullshit");
                    if (uiBullshit2 != null)
                    {
                        foreach (var m in AccessTools.GetDeclaredMethods(uiBullshit2))
                        {
                            if (m == null || m.IsGenericMethod || m.ContainsGenericParameters) continue;
                            if (m.Name != "_GUI_FocusableButton") continue;
                            if (!m.GetParameters().Any(x => x.ParameterType == typeof(string) || x.ParameterType == typeof(string).MakeByRefType())) continue;
                            _harmony.Patch(m, prefix: new HarmonyMethod(typeof(Plugin), nameof(TranslateAllStringArgsPrefix)));
                            count++;
                        }
                        Log.LogInfo("Patched UIBullshit._GUI_FocusableButton string sinks.");
                    }
                }
                catch (Exception ex) { Log.LogWarning("Failed to patch KrokMP focusable button method: " + ex.Message); }
            }
            else
            {
                Log.LogInfo("Skipped UIBullshit._GUI_FocusableButton patch by default lag-safety setting.");
            }

            try
            {
                var uiInGame = AccessTools.TypeByName("KrokoshaCasualtiesMP.UIInGame") ?? AccessTools.TypeByName("UIInGame");
                var interactionButton = uiInGame == null ? null : AccessTools.Method(uiInGame, "DoPlayerInteractionMenuButton");
                if (interactionButton != null)
                {
                    _harmony.Patch(interactionButton, prefix: new HarmonyMethod(typeof(Plugin), nameof(TranslateAllStringArgsPrefix)));
                    count++;
                    Log.LogInfo("Patched UIInGame.DoPlayerInteractionMenuButton string sink.");
                }
            }
            catch (Exception ex) { Log.LogWarning("Failed to patch KrokMP player interaction button method: " + ex.Message); }

            try
            {
                var dropdownType = AccessTools.TypeByName("KrokoshaCasualtiesMP.GUILayout_DropdownMenu") ?? AccessTools.TypeByName("GUILayout_DropdownMenu");
                var dropdown = dropdownType == null ? null : AccessTools.Method(dropdownType, "Dropdown", new[] { typeof(int), typeof(string[]), typeof(GUILayoutOption[]) });
                if (dropdown != null)
                {
                    _harmony.Patch(dropdown, prefix: new HarmonyMethod(typeof(Plugin), nameof(TranslateDropdownOptionsPrefix)));
                    count++;
                    Log.LogInfo("Patched GUILayout_DropdownMenu.Dropdown string[] options.");
                }
            }
            catch (Exception ex) { Log.LogWarning("Failed to patch KrokMP dropdown menu method: " + ex.Message); }

            return count;
        }

        private int PatchChineseSearchMethods()
        {
            int count = 0;
            try
            {
                var stringUtility = AccessTools.TypeByName("KrokoshaCasualtiesUtils.StringUtility") ?? AccessTools.TypeByName("StringUtility");
                if (stringUtility == null) return 0;
                foreach (var m in AccessTools.GetDeclaredMethods(stringUtility))
                {
                    if (m == null || m.Name != "ContainsInsensitive") continue;
                    var ps = m.GetParameters();
                    if (ps.Length >= 2 && ps[0].ParameterType == typeof(string) && ps[1].ParameterType == typeof(string) && m.ReturnType == typeof(bool))
                    {
                        _harmony.Patch(m, postfix: new HarmonyMethod(typeof(Plugin), nameof(ContainsInsensitivePostfix)));
                        count++;
                    }
                }
                if (count > 0) Log.LogInfo($"Patched StringUtility.ContainsInsensitive for Chinese rule search: {count}");
            }
            catch (Exception ex) { Log.LogWarning("Failed to patch StringUtility.ContainsInsensitive: " + ex.Message); }
            return count;
        }

        [HarmonyPriority(Priority.Last)]
        private static bool IsGameplayScene()
        {
            try
            {
                return string.Equals(SceneManager.GetActiveScene().name, "SampleScene", StringComparison.Ordinal);
            }
            catch { return false; }
        }

        private static bool ShouldTranslateGenericImGuiNow()
        {
            try
            {
                if (EnableImGuiInGameplay != null && EnableImGuiInGameplay.Value) return true;
                return !IsGameplayScene();
            }
            catch { return true; }
        }

        private static string TranslateForImGui(string input)
        {
            try
            {
                if (ShouldTranslateGenericImGuiNow()) return Translator.Translate(input);
                if (EnableSafeGameplayGuiTranslation != null && EnableSafeGameplayGuiTranslation.Value)
                    return Translator.TranslateGameplaySafe(input);
            }
            catch { }
            return input;
        }

        private static void TranslateGuiContentForImGui(GUIContent gc)
        {
            if (gc == null) return;
            gc.text = TranslateForImGui(gc.text);
            gc.tooltip = TranslateForImGui(gc.tooltip);
        }

        public static void TranslateTooltipStaticFieldsPrefix()
        {
            try
            {
                if (!_tooltipFieldsResolved)
                {
                    _tooltipFieldsResolved = true;
                    var t = AccessTools.TypeByName("KrokoshaCasualtiesMP.UIBullshit") ?? AccessTools.TypeByName("UIBullshit");
                    if (t != null)
                    {
                        _tooltipNameField = AccessTools.Field(t, "_gui_tooltip_name");
                        _tooltipDescField = AccessTools.Field(t, "_gui_tooltip_desc");
                    }
                }
                var nf = _tooltipNameField;
                var df = _tooltipDescField;
                if (nf != null)
                {
                    var v = nf.GetValue(null) as string;
                    var tv = Translator.Translate(v);
                    if (!string.Equals(v, tv, StringComparison.Ordinal)) nf.SetValue(null, tv);
                }
                if (df != null)
                {
                    var v = df.GetValue(null) as string;
                    var tv = Translator.Translate(v);
                    if (!string.Equals(v, tv, StringComparison.Ordinal)) df.SetValue(null, tv);
                }
            }
            catch { }
        }

        public static bool KrokLangGetPrefix(object[] __args, ref string __result)
        {
            try
            {
                if (__args == null || __args.Length == 0) return true;
                string key = __args[0] as string;
                if (string.IsNullOrEmpty(key)) return true;
                if (Translator.TryTranslateKrokLangKey(key, out var translated))
                {
                    __result = translated;
                    if (VerboseLogging != null && VerboseLogging.Value)
                        Log.LogInfo($"Lang.Get: [{key}] -> [{translated}]");
                    return false;
                }
            }
            catch { }
            return true;
        }

        public static void ContainsInsensitivePostfix(object[] __args, ref bool __result)
        {
            try
            {
                if (IsGameplayScene()) return;
                if (__result) return;
                if (__args == null || __args.Length < 2) return;
                string haystack = __args[0] as string;
                string needle = __args[1] as string;
                if (Translator.ChineseSearchMatches(haystack, needle)) __result = true;
            }
            catch { }
        }

        [HarmonyPriority(Priority.Last)]
        public static void TranslateAllStringArgsPrefix(object[] __args)
        {
            if (__args == null) return;
            for (int i = 0; i < __args.Length; i++)
            {
                if (__args[i] is string s) __args[i] = Translator.Translate(s);
                else if (__args[i] is GUIContent gc && gc != null)
                {
                    gc.text = Translator.Translate(gc.text);
                    gc.tooltip = Translator.Translate(gc.tooltip);
                }
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void ChatLogMessagePrefix(ref string plrname, ref string msg)
        {
            try
            {
                var rawName = plrname ?? string.Empty;
                var translatedName = Translator.Translate(rawName);
                if (!string.Equals(rawName, translatedName, StringComparison.Ordinal)) plrname = translatedName;

                string nameForCheck = rawName.Trim();
                bool looksSystem = nameForCheck.StartsWith("*", StringComparison.Ordinal) ||
                                   nameForCheck.IndexOf("SERVER", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   nameForCheck.IndexOf("SYSTEM", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   nameForCheck.IndexOf("OFFLINE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   nameForCheck.IndexOf("服务器", StringComparison.Ordinal) >= 0 ||
                                   nameForCheck.IndexOf("系统", StringComparison.Ordinal) >= 0 ||
                                   nameForCheck.IndexOf("离线", StringComparison.Ordinal) >= 0;
                if (looksSystem)
                {
                    msg = Translator.Translate(msg);
                }
            }
            catch { }
        }

        [HarmonyPriority(Priority.Last)]
        public static void TranslateDropdownOptionsPrefix(ref string[] options)
        {
            try
            {
                if (options == null || options.Length == 0) return;
                string[] copy = null;
                for (int i = 0; i < options.Length; i++)
                {
                    var original = options[i];
                    var translated = TranslateForImGui(original);
                    if (!string.Equals(original, translated, StringComparison.Ordinal))
                    {
                        if (copy == null)
                        {
                            copy = new string[options.Length];
                            Array.Copy(options, copy, options.Length);
                        }
                        copy[i] = translated;
                    }
                }
                if (copy != null) options = copy;
            }
            catch { }
        }

        [HarmonyPriority(Priority.Last)]
        public static void TextSetterPrefix(ref string value)
        {
            value = Translator.Translate(value);
        }

        [HarmonyPriority(Priority.Last)]
        public static void TranslateFirstStringArgPrefix(object[] __args)
        {
            if (__args == null) return;
            for (int i = 0; i < __args.Length; i++)
            {
                if (__args[i] is string s)
                {
                    __args[i] = TranslateForImGui(s);
                    return;
                }
                if (__args[i] is GUIContent gc && gc != null)
                {
                    gc.text = TranslateForImGui(gc.text);
                    gc.tooltip = TranslateForImGui(gc.tooltip);
                    return;
                }
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void TranslateArgsPrefix(object[] __args)
        {
            if (__args == null) return;
            for (int i = 0; i < __args.Length; i++)
            {
                var arg = __args[i];
                if (arg is string s)
                {
                    __args[i] = TranslateForImGui(s);
                }
                else if (arg is GUIContent gc && gc != null)
                {
                    TranslateGuiContentForImGui(gc);
                }
                else if (arg is string[] arr)
                {
                    for (int j = 0; j < arr.Length; j++) arr[j] = TranslateForImGui(arr[j]);
                }
                else if (arg is GUIContent[] contents)
                {
                    foreach (var c in contents) TranslateGuiContentForImGui(c);
                }
            }
        }
    }


    internal static class DisplayNameResolver
    {
        private static readonly object LockObj = new object();
        private static readonly Dictionary<string, string> Replacements = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<ulong, string> SteamNameCache = new Dictionary<ulong, string>();
        private static readonly List<KeyValuePair<string, string>> OrderedReplacements = new List<KeyValuePair<string, string>>();
        private static float _nextScan;
        private static int _lastLoggedCount = -1;
        private static Type _netPlayerType;
        private static FieldInfo _clientIdDictField;
        private static FieldInfo _playerNameField;
        private static FieldInfo _steamIdField;
        private static Type _kSteamType;
        private static Type _steamFriendsType;
        private static Type _cSteamIdType;
        private static MethodInfo _getFriendPersonaNameMethod;
        private static MethodInfo _requestUserInformationMethod;
        private static MethodInfo _getLocalUsernameMethod;
        private static MethodInfo _getLocalUserSteamIdMethod;

        public static void Reset()
        {
            lock (LockObj)
            {
                Replacements.Clear();
                SteamNameCache.Clear();
                OrderedReplacements.Clear();
                _nextScan = 0f;
                _lastLoggedCount = -1;
                _netPlayerType = null;
                _clientIdDictField = null;
                _playerNameField = null;
                _steamIdField = null;
                _kSteamType = null;
                _steamFriendsType = null;
                _cSteamIdType = null;
                _getFriendPersonaNameMethod = null;
                _requestUserInformationMethod = null;
                _getLocalUsernameMethod = null;
                _getLocalUserSteamIdMethod = null;
            }
        }

        public static string Apply(string input)
        {
            // v0.1.22: Steam persona display-name replacement has been removed.
            // It was display-only and did not affect networking, but KrokMP nametag boxes are too narrow
            // for CJK Steam names and the closing bracket can be pushed out of view. Keep the original
            // KrokMP player names to preserve stable UI layout.
            return input;
        }

        private static void RefreshIfNeeded()
        {
            try
            {
                if (Time.unscaledTime < _nextScan) return;
                _nextScan = Time.unscaledTime + 0.75f;
                RefreshMappings();
            }
            catch { }
        }

        private static void RefreshMappings()
        {
            ResolveReflectionHandles();
            var next = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var player in EnumerateNetPlayers())
            {
                try
                {
                    var raw = _playerNameField == null ? null : _playerNameField.GetValue(player) as string;
                    ulong steamId = 0UL;
                    if (_steamIdField != null)
                    {
                        var v = _steamIdField.GetValue(player);
                        if (v is ulong ul) steamId = ul;
                        else if (v != null) ulong.TryParse(v.ToString(), out steamId);
                    }
                    if (steamId == 0UL || string.IsNullOrWhiteSpace(raw)) continue;
                    var display = GetPersonaName(steamId);
                    if (!IsUsableDisplayName(display)) continue;
                    AddMapping(next, raw, display);
                }
                catch { }
            }

            // Some KrokMP paths use the local Steam persona directly before NetPlayer is fully populated.
            try
            {
                ulong localId = GetLocalSteamId();
                if (localId != 0UL)
                {
                    var localName = GetLocalSteamName();
                    if (IsUsableDisplayName(localName))
                    {
                        var cached = GetPersonaName(localId);
                        if (IsUsableDisplayName(cached)) localName = cached;
                        // Do not add arbitrary local-name aliases here; NetPlayer scan is safer.
                    }
                }
            }
            catch { }

            lock (LockObj)
            {
                Replacements.Clear();
                foreach (var kv in next) Replacements[kv.Key] = kv.Value;
                OrderedReplacements.Clear();
                OrderedReplacements.AddRange(Replacements.OrderByDescending(kv => kv.Key.Length));
                if (Plugin.VerboseSteamDisplayNameLogging != null && Plugin.VerboseSteamDisplayNameLogging.Value && _lastLoggedCount != OrderedReplacements.Count)
                {
                    _lastLoggedCount = OrderedReplacements.Count;
                    Plugin.Log.LogInfo("Steam display-name mappings: " + string.Join(", ", OrderedReplacements.Select(kv => "[" + kv.Key + "]=>[" + kv.Value + "]").ToArray()));
                }
            }
        }

        private static void AddMapping(Dictionary<string, string> map, string raw, string display)
        {
            if (string.IsNullOrWhiteSpace(raw) || string.IsNullOrWhiteSpace(display)) return;
            display = FormatDisplayName(display);
            if (string.IsNullOrWhiteSpace(display)) return;
            if (raw.Length < 2) return;
            if (string.Equals(raw, display, StringComparison.Ordinal)) return;
            map[raw] = display;

            // KrokMP can replace unsupported CJK glyphs with question marks in NetPlayer.ApplyNameAndColor.
            // Add the question-mark alias only when it is long enough to avoid replacing ordinary punctuation.
            if (raw.Length >= 3 && raw.All(c => c == '?'))
                map[raw] = display;
        }

        private static string FormatDisplayName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;
            var trimmed = name.Trim();
            int max = 8;
            try
            {
                if (Plugin.MaxSteamDisplayNameChars != null) max = Plugin.MaxSteamDisplayNameChars.Value;
            }
            catch { }
            if (max <= 0) return trimmed;

            bool ellipsis = true;
            try
            {
                if (Plugin.AppendEllipsisWhenTruncated != null) ellipsis = Plugin.AppendEllipsisWhenTruncated.Value;
            }
            catch { }
            return TruncateTextElements(trimmed, max, ellipsis);
        }

        private static string FormatDisplayNameForNameTag(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return name;
            var trimmed = name.Trim();
            int max = 6;
            try
            {
                if (Plugin.NameTagMaxSteamDisplayNameChars != null) max = Plugin.NameTagMaxSteamDisplayNameChars.Value;
            }
            catch { }
            if (max <= 0) return trimmed;
            return TruncateTextElements(trimmed, max, false);
        }

        private static string TruncateTextElements(string trimmed, int max, bool ellipsis)
        {
            try
            {
                var parts = new List<string>();
                TextElementEnumerator e = StringInfo.GetTextElementEnumerator(trimmed);
                while (e.MoveNext()) parts.Add(e.GetTextElement());
                if (parts.Count <= max) return trimmed;
                int take = ellipsis ? Math.Max(1, max - 1) : max;
                var sb = new StringBuilder();
                for (int i = 0; i < take && i < parts.Count; i++) sb.Append(parts[i]);
                if (ellipsis) sb.Append("…");
                return sb.ToString();
            }
            catch
            {
                if (trimmed.Length <= max) return trimmed;
                int take = ellipsis ? Math.Max(1, max - 1) : max;
                return trimmed.Substring(0, Math.Min(take, trimmed.Length)) + (ellipsis ? "…" : string.Empty);
            }
        }

        private static bool IsUsableDisplayName(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (s.All(c => c == '?')) return false;
            if (s.IndexOf("ERROR", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            return true;
        }

        private static IEnumerable<object> EnumerateNetPlayers()
        {
            ResolveReflectionHandles();
            if (_clientIdDictField == null) yield break;
            object dict = null;
            try { dict = _clientIdDictField.GetValue(null); } catch { }
            if (dict == null) yield break;
            object values = null;
            try
            {
                var valuesProp = dict.GetType().GetProperty("Values");
                if (valuesProp != null) values = valuesProp.GetValue(dict, null);
            }
            catch { }
            var enumerable = values as System.Collections.IEnumerable;
            if (enumerable == null) yield break;
            foreach (var obj in enumerable)
            {
                if (obj != null) yield return obj;
            }
        }

        private static void ResolveReflectionHandles()
        {
            if (_netPlayerType == null)
            {
                _netPlayerType = AccessTools.TypeByName("KrokoshaCasualtiesMP.NetPlayer") ?? AccessTools.TypeByName("NetPlayer");
                if (_netPlayerType != null)
                {
                    _clientIdDictField = AccessTools.Field(_netPlayerType, "ClientIdToPlayerDict");
                    _playerNameField = AccessTools.Field(_netPlayerType, "playername");
                    _steamIdField = AccessTools.Field(_netPlayerType, "steam_id");
                }
            }
            if (_kSteamType == null)
            {
                _kSteamType = AccessTools.TypeByName("KrokoshaCasualtiesMP.KSteam") ?? AccessTools.TypeByName("KSteam");
                if (_kSteamType != null)
                {
                    _getLocalUsernameMethod = AccessTools.Method(_kSteamType, "GetLocalUsername");
                    _getLocalUserSteamIdMethod = AccessTools.Method(_kSteamType, "GetLocalUserSteamID");
                }
            }
            if (_steamFriendsType == null)
            {
                _steamFriendsType = AccessTools.TypeByName("Steamworks.SteamFriends") ?? AccessTools.TypeByName("SteamFriends");
                _cSteamIdType = AccessTools.TypeByName("Steamworks.CSteamID") ?? AccessTools.TypeByName("CSteamID");
                if (_steamFriendsType != null && _cSteamIdType != null)
                {
                    _getFriendPersonaNameMethod = AccessTools.Method(_steamFriendsType, "GetFriendPersonaName", new[] { _cSteamIdType });
                    _requestUserInformationMethod = AccessTools.Method(_steamFriendsType, "RequestUserInformation", new[] { _cSteamIdType, typeof(bool) });
                }
            }
        }

        private static string GetPersonaName(ulong steamId)
        {
            if (steamId == 0UL) return null;
            if (SteamNameCache.TryGetValue(steamId, out var cached)) return cached;
            ResolveReflectionHandles();
            string name = null;
            try
            {
                if (_getFriendPersonaNameMethod != null && _cSteamIdType != null)
                {
                    object csteam = Activator.CreateInstance(_cSteamIdType, new object[] { steamId });
                    try { _requestUserInformationMethod?.Invoke(null, new object[] { csteam, true }); } catch { }
                    name = _getFriendPersonaNameMethod.Invoke(null, new object[] { csteam }) as string;
                }
            }
            catch { }
            if (!IsUsableDisplayName(name)) name = null;
            SteamNameCache[steamId] = name;
            return name;
        }

        private static string GetLocalSteamName()
        {
            ResolveReflectionHandles();
            try { return _getLocalUsernameMethod?.Invoke(null, null) as string; } catch { return null; }
        }

        private static ulong GetLocalSteamId()
        {
            ResolveReflectionHandles();
            try
            {
                var idObj = _getLocalUserSteamIdMethod?.Invoke(null, null);
                if (idObj == null) return 0UL;
                var f = AccessTools.Field(idObj.GetType(), "m_SteamID");
                if (f != null)
                {
                    var v = f.GetValue(idObj);
                    if (v is ulong ulField) return ulField;
                    if (v != null && ulong.TryParse(v.ToString(), out var parsedField)) return parsedField;
                }
                var p = AccessTools.Property(idObj.GetType(), "m_SteamID");
                if (p != null)
                {
                    var v = p.GetValue(idObj, null);
                    if (v is ulong ulProp) return ulProp;
                    if (v != null && ulong.TryParse(v.ToString(), out var parsedProp)) return parsedProp;
                }
            }
            catch { }
            return 0UL;
        }
    }

    internal static class Translator
    {
        private static readonly Dictionary<string, string> Exact = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> ExactNorm = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly List<KeyValuePair<string, string>> Phrases = new List<KeyValuePair<string, string>>();
        private static readonly List<PlaceholderRule> PlaceholderRules = new List<PlaceholderRule>();
        private static readonly Dictionary<string, string> KrokLangEntries = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> TranslationCache = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly object CacheLock = new object();
        private static readonly Dictionary<string, string> RuleLabelToField = new Dictionary<string, string>(StringComparer.Ordinal);

        public static int ExactCount => Exact.Count;
        public static int PhraseCount => Phrases.Count;
        public static int PlaceholderCount => PlaceholderRules.Count;
        public static int KrokLangEntryCount => KrokLangEntries.Count;

        public static void Load(string dir, ManualLogSource log)
        {
            Exact.Clear();
            ExactNorm.Clear();
            Phrases.Clear();
            PlaceholderRules.Clear();
            KrokLangEntries.Clear();
            lock (CacheLock) TranslationCache.Clear();
            RuleLabelToField.Clear();

            LoadFile(Path.Combine(dir, "translations.zh-CN.json"), false, log);
            LoadFile(Path.Combine(dir, "phrases.zh-CN.json"), true, log);
            Phrases.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));
        }

        private static void LoadFile(string file, bool phrase, ManualLogSource log)
        {
            if (!File.Exists(file))
            {
                log.LogWarning("Translation file not found: " + file);
                return;
            }
            try
            {
                var text = File.ReadAllText(file, Encoding.UTF8);
                var entries = JsonObjectParser.Parse(text);
                foreach (var kv in entries)
                {
                    if (string.IsNullOrEmpty(kv.Key)) continue;
                    if (phrase)
                    {
                        Phrases.Add(new KeyValuePair<string, string>(kv.Key, kv.Value ?? string.Empty));
                    }
                    else
                    {
                        AddExact(kv.Key, kv.Value ?? string.Empty);
                    }
                }
                log.LogInfo($"Loaded {entries.Count} {(phrase ? "phrase" : "exact")} entries from {file}");
            }
            catch (Exception ex)
            {
                log.LogWarning($"Failed to load {file}: {ex.Message}");
            }
        }

        private static void AddExact(string key, string value)
        {
            Exact[key] = value;
            var n = Normalize(key);
            if (!ExactNorm.ContainsKey(n)) ExactNorm[n] = value;
            if (IsLikelyKrokRuleFieldName(key) && !string.IsNullOrEmpty(value))
            {
                if (!RuleLabelToField.ContainsKey(value)) RuleLabelToField[value] = key;
            }
            if (key.StartsWith("krokosha_coop_", StringComparison.Ordinal))
            {
                KrokLangEntries[key] = value;
                var raw = key.Substring("krokosha_coop_".Length);
                if (!KrokLangEntries.ContainsKey(raw)) KrokLangEntries[raw] = value;
            }
            if (key.IndexOf('{') >= 0 && key.IndexOf('}') >= 0)
            {
                var rule = PlaceholderRule.TryCreate(key, value);
                if (rule != null) PlaceholderRules.Add(rule);
            }
        }


        private static bool IsLikelyKrokRuleFieldName(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            if (key.StartsWith("krokosha_coop_", StringComparison.Ordinal)) return false;
            if (key.IndexOf(' ') >= 0 || key.IndexOf(':') >= 0) return false;
            if (key == key.ToUpperInvariant() && key.IndexOf('_') >= 0) return true;
            return key.Any(char.IsUpper) && key.Any(char.IsLower);
        }


        public static bool ChineseSearchMatches(string haystack, string query)
        {
            if (string.IsNullOrWhiteSpace(haystack) || string.IsNullOrWhiteSpace(query)) return false;
            if (!ContainsCjk(query)) return false;
            string q = query.Trim();

            string translated;
            if (Exact.TryGetValue(haystack, out translated) || ExactNorm.TryGetValue(Normalize(haystack), out translated))
            {
                if (!string.IsNullOrEmpty(translated) && translated.IndexOf(q, StringComparison.Ordinal) >= 0) return true;
            }

            foreach (var kv in RuleLabelToField)
            {
                if (string.Equals(kv.Value, haystack, StringComparison.Ordinal))
                {
                    if (kv.Key.IndexOf(q, StringComparison.Ordinal) >= 0) return true;
                }
            }

            var alias = MapChineseRuleKeyword(q);
            if (!string.IsNullOrEmpty(alias) && haystack.IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        public static bool TryTranslateRuleSearchQuery(string input, out string mapped)
        {
            mapped = null;
            if (string.IsNullOrWhiteSpace(input)) return false;
            if (!ContainsCjk(input)) return false;
            string q = input.Trim();

            foreach (var kv in RuleLabelToField)
            {
                if (string.Equals(kv.Key, q, StringComparison.Ordinal) || kv.Key.IndexOf(q, StringComparison.Ordinal) >= 0)
                {
                    mapped = kv.Value;
                    return mapped != input;
                }
            }

            var alias = MapChineseRuleKeyword(q);
            if (!string.IsNullOrEmpty(alias) && alias != input)
            {
                mapped = alias;
                return true;
            }
            return false;
        }

        private static bool ContainsCjk(string s)
        {
            foreach (char c in s)
                if (c >= '\u4e00' && c <= '\u9fff') return true;
            return false;
        }

        private static string MapChineseRuleKeyword(string s)
        {
            string r = s;
            var pairs = new[]
            {
                new KeyValuePair<string,string>("额外生命", "Vital"),
                new KeyValuePair<string,string>("生命", "Vital"),
                new KeyValuePair<string,string>("恢复", "Regen"),
                new KeyValuePair<string,string>("衰减", "Drain"),
                new KeyValuePair<string,string>("倍率", "Mult"),
                new KeyValuePair<string,string>("玩家人数", "PLAYER_COUNT"),
                new KeyValuePair<string,string>("玩家", "Player"),
                new KeyValuePair<string,string>("人数", "COUNT"),
                new KeyValuePair<string,string>("上限", "LIMIT"),
                new KeyValuePair<string,string>("显示", "Show"),
                new KeyValuePair<string,string>("方向", "Directions"),
                new KeyValuePair<string,string>("名牌", "Nametag"),
                new KeyValuePair<string,string>("状态", "Status"),
                new KeyValuePair<string,string>("图标", "Icon"),
                new KeyValuePair<string,string>("聊天框", "Chatbox"),
                new KeyValuePair<string,string>("聊天", "Chat"),
                new KeyValuePair<string,string>("近距离", "Proximity"),
                new KeyValuePair<string,string>("睡眠", "Sleep"),
                new KeyValuePair<string,string>("睡觉", "Sleep"),
                new KeyValuePair<string,string>("自动", "Auto"),
                new KeyValuePair<string,string>("继续", "Continue"),
                new KeyValuePair<string,string>("死亡", "Died"),
                new KeyValuePair<string,string>("离开", "Leave"),
                new KeyValuePair<string,string>("退出", "Exit"),
                new KeyValuePair<string,string>("经验", "Experience"),
                new KeyValuePair<string,string>("获取", "Gain"),
                new KeyValuePair<string,string>("惩罚", "Punish"),
                new KeyValuePair<string,string>("距离", "Distance"),
                new KeyValuePair<string,string>("层级", "Layer"),
                new KeyValuePair<string,string>("完成", "Finish"),
                new KeyValuePair<string,string>("复活", "Respawn"),
                new KeyValuePair<string,string>("允许", "Allow"),
                new KeyValuePair<string,string>("客户端", "Client"),
                new KeyValuePair<string,string>("作弊", "Cheat"),
                new KeyValuePair<string,string>("命令", "Commands"),
                new KeyValuePair<string,string>("保存", "Save"),
                new KeyValuePair<string,string>("状态", "State"),
            };
            foreach (var kv in pairs) r = r.Replace(kv.Key, kv.Value);
            return ContainsCjk(r) ? null : r;
        }

        public static bool TryTranslateKrokLangKey(string key, out string translated)
        {
            translated = null;
            if (string.IsNullOrEmpty(key)) return false;
            if (KrokLangEntries.TryGetValue(key, out translated)) return true;
            if (!key.StartsWith("krokosha_coop_", StringComparison.Ordinal))
            {
                if (KrokLangEntries.TryGetValue("krokosha_coop_" + key, out translated)) return true;
                if (Exact.TryGetValue("krokosha_coop_" + key, out translated)) return true;
            }
            if (Exact.TryGetValue(key, out translated)) return true;
            var norm = Normalize(key);
            if (ExactNorm.TryGetValue(norm, out translated)) return true;
            return false;
        }

        public static int InjectKrokLangEntriesIntoDictionaryLikeObject(object dictionaryLike)
        {
            if (dictionaryLike == null) return 0;
            int changed = 0;
            var type = dictionaryLike.GetType();
            var containsKey = type.GetMethod("ContainsKey", new[] { typeof(string) });
            var item = type.GetProperty("Item", new[] { typeof(string) });
            if (containsKey == null || item == null || !item.CanWrite) return 0;

            foreach (var kv in KrokLangEntries)
            {
                if (!kv.Key.StartsWith("krokosha_coop_", StringComparison.Ordinal)) continue;
                bool exists = false;
                try { exists = (bool)containsKey.Invoke(dictionaryLike, new object[] { kv.Key }); } catch { exists = false; }
                if (!exists)
                {
                    try
                    {
                        item.SetValue(dictionaryLike, kv.Value, new object[] { kv.Key });
                        changed++;
                    }
                    catch { }
                }
            }
            return changed;
        }

        public static string TranslateGameplaySafe(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Length > 512) return DisplayNameResolver.Apply(input);

            string output;
            lock (CacheLock)
            {
                if (TranslationCache.TryGetValue("#GAME#" + input, out var cached)) return DisplayNameResolver.Apply(cached);
            }

            output = TranslateGameplaySafeUncached(input);
            lock (CacheLock)
            {
                if (TranslationCache.Count > 4096) TranslationCache.Clear();
                TranslationCache["#GAME#" + input] = output;
            }
            return DisplayNameResolver.Apply(output);
        }

        private static string TranslateGameplaySafeUncached(string input)
        {
            if (!LooksTranslatable(input)) return input;

            if (Exact.TryGetValue(input, out var direct)) return direct;
            var norm = Normalize(input);
            if (ExactNorm.TryGetValue(norm, out var ndirect)) return ndirect;

            var dynamic = TryDynamicTranslate(input);
            if (dynamic != null) return dynamic;

            if (input.IndexOf('\n') >= 0)
            {
                var lines = input.Split(new[] { "\n" }, StringSplitOptions.None);
                bool anyLineChanged = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    string t = TranslateGameplaySafe(lines[i]);
                    if (!string.Equals(t, lines[i], StringComparison.Ordinal))
                    {
                        lines[i] = t;
                        anyLineChanged = true;
                    }
                }
                if (anyLineChanged) return string.Join("\n", lines);
            }

            return input;
        }

        public static string Translate(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Length > 2000) return DisplayNameResolver.Apply(input);

            string output;
            lock (CacheLock)
            {
                if (TranslationCache.TryGetValue(input, out var cached)) return DisplayNameResolver.Apply(cached);
            }

            output = TranslateUncached(input);
            lock (CacheLock)
            {
                if (TranslationCache.Count > 4096) TranslationCache.Clear();
                TranslationCache[input] = output;
            }
            return DisplayNameResolver.Apply(output);
        }

        private static string TranslateUncached(string input)
        {
            if (!LooksTranslatable(input)) return input;

            if (Exact.TryGetValue(input, out var direct)) return direct;
            var norm = Normalize(input);
            if (ExactNorm.TryGetValue(norm, out var ndirect)) return ndirect;

            var dynamic = TryDynamicTranslate(input);
            if (dynamic != null) return dynamic;

            foreach (var rule in PlaceholderRules)
            {
                var result = rule.TryTranslate(input);
                if (result != null) return result;
            }

            if (input.IndexOf('\n') >= 0)
            {
                var lines = input.Split(new[] { "\n" }, StringSplitOptions.None);
                bool anyLineChanged = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    string t = Translate(lines[i]);
                    if (!string.Equals(t, lines[i], StringComparison.Ordinal))
                    {
                        lines[i] = t;
                        anyLineChanged = true;
                    }
                }
                if (anyLineChanged) return string.Join("\n", lines);
            }

            string changed = input;
            bool didPhrase = false;
            foreach (var kv in Phrases)
            {
                if (string.IsNullOrEmpty(kv.Key)) continue;
                if (changed.IndexOf(kv.Key, StringComparison.Ordinal) >= 0)
                {
                    changed = changed.Replace(kv.Key, kv.Value);
                    didPhrase = true;
                }
            }

            if (didPhrase)
            {
                if (Exact.TryGetValue(changed, out var after)) return after;
                var n2 = Normalize(changed);
                if (ExactNorm.TryGetValue(n2, out var afterNorm)) return afterNorm;
                if (Plugin.VerboseLogging != null && Plugin.VerboseLogging.Value)
                    Plugin.Log.LogInfo($"Translated by phrase: [{input}] -> [{changed}]");
                return changed;
            }
            return input;
        }

        private static bool LooksTranslatable(string s)
        {
            if (s.Length > 2000) return false;
            foreach (char c in s)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) return true;
            }
            return false;
        }

        private static string Normalize(string s)
        {
            if (s == null) return string.Empty;
            return s.Replace("\r\n", "\n").Trim();
        }

        private static string TryDynamicTranslate(string input)
        {
            try
            {
                string s = input ?? string.Empty;
                string rest;
                if (TryPrefix(s, "Last Status Message:", out rest)) return "最后状态信息：" + (rest.Length > 0 ? Translate(rest) : string.Empty);
                if (TryPrefix(s, "Mode:", out rest)) return "模式：" + TranslateCapturedValue(rest);
                if (TryPrefix(s, "Average Ping:", out rest)) return "平均延迟：" + rest.Trim();
                if (TryPrefix(s, "Connected Players:", out rest)) return "已连接玩家：" + rest.Trim();
                if (TryPrefix(s, "Ping:", out rest)) return "延迟：" + rest.Trim();
                if (TryPrefix(s, "ping:", out rest)) return "延迟：" + rest.Trim();
                if (TryPrefix(s, "SERVER TPS:", out rest)) return "服务器刻率：" + rest.Trim();
                if (TryPrefix(s, "LOCAL TPS:", out rest)) return "本地刻率：" + rest.Trim();
                if (TryPrefix(s, "FPS:", out rest)) return "帧率：" + TranslateCapturedValue(rest.Trim().Replace("   TPS:", "   刻率："));
                if (TryPrefix(s, "Alive:", out rest)) return "存活：" + rest.Trim();
                if (TryPrefix(s, "Mood:", out rest)) return "心情：" + TranslateCapturedValue(rest);
                if (TryPrefix(s, "心情:", out rest)) return "心情：" + TranslateCapturedValue(rest);
                if (TryPrefix(s, "心情：", out rest)) return "心情：" + TranslateCapturedValue(rest);
                if (TryPrefix(s, "存活:", out rest)) return "存活人数：" + rest.Trim();
                if (TryPrefix(s, "存活：", out rest)) return "存活人数：" + rest.Trim();
                if (TryPrefix(s, "Alive ", out rest)) return "存活人数：" + rest.Trim();
                if (TryPrefix(s, "Steam Username:", out rest)) return "Steam 用户名：" + rest;
                if (TryPrefix(s, "Found Lobbies:", out rest)) return "已找到的房间：" + Translate(rest.Trim());
                if (TryPrefix(s, "Shown:", out rest)) return "显示数量：" + rest.Trim();
                if (s.IndexOf("Shown:", StringComparison.Ordinal) >= 0)
                {
                    var replaced = s.Replace("Shown:", "显示数量：");
                    if (!string.Equals(replaced, s, StringComparison.Ordinal)) return Translate(replaced);
                }
                if (TryPrefix(s, "SteamMatchmaking.RequestLobbyList  dist:", out rest)) return "Steam 大厅列表请求：距离筛选 " + TranslateCapturedValue(rest);
                if (TryPrefix(s, "Steamworks initialized!", out rest)) return "Steamworks 已初始化！" + (string.IsNullOrEmpty(rest) ? string.Empty : "\n" + rest.TrimStart('\n', '\r'));
                if (TryPrefix(s, "Group ", out rest)) return "队伍 " + rest;
                if (TryPrefix(s, "CO-OP MOD v", out rest)) return "联机模组 v" + rest.Replace("    \"RELEASE BUILD\"  STEAM_APPID:", "  发布版  APPID：");
                if (TryPrefix(s, "Local player username:", out rest)) return "本地玩家用户名：" + rest.TrimStart();
                if (TryPrefix(s, "Voicechat current volume level:", out rest)) return "语音聊天当前音量：" + rest.Trim();
                if (TryPrefix(s, "Kicked ", out rest)) return "已踢出 " + TranslateCapturedValue(rest);
                if (TryPrefix(s, "Disconnected:", out rest)) return "已断开连接：" + TranslateCapturedValue(rest);
                if (TryPrefix(s, "Connection Rejected:", out rest)) return "连接被拒绝：" + TranslateCapturedValue(rest);
                if (TryPrefix(s, "DEDICATED SERVER: SWITCH COUNTER:", out rest)) return "专用服务器：切换计数：" + rest.Trim();
                if (TryPrefix(s, "Unknown command:", out rest)) return "未知命令：" + TranslateCapturedValue(rest.Trim());
                if (TryPrefix(s, "SERVER: SENDING CHAT ANNOUNCEMENT:", out rest)) return "服务器：正在发送聊天公告：" + Translate(rest.TrimStart());
                if (TryPrefix(s, "SERVER: SENDING NAMED CHAT ANNOUNCEMENT:", out rest)) return "服务器：正在发送署名聊天公告：" + rest.TrimStart();
                if (TryPrefix(s, "STEAM: LeaveLobby called! lobbyID:", out rest)) return "STEAM：已调用离开大厅，lobbyID：" + rest.Trim();
                if (TryPrefix(s, "STEAM: CHANGED LOBBY TYPE TO", out rest)) return "STEAM：大厅类型已改为 " + TranslateCapturedValue(rest.Trim());
                if (s.IndexOf("You can't play singleplayer with MP mod active", StringComparison.Ordinal) >= 0 || s.IndexOf("Deactivate it in", StringComparison.Ordinal) >= 0)
                {
                    string replaced = s
                        .Replace("You can't play singleplayer with MP mod active.", "联机模组启用时不能游玩单人模式。")
                        .Replace("You can't play singleplayer with MP mod active", "联机模组启用时不能游玩单人模式")
                        .Replace("Deactivate it in Settings > General", "请在 设置 > 通用 中停用它。")
                        .Replace("Deactivate it in 设置 > 通用", "请在 设置 > 通用 中停用它。")
                        .Replace("Deactivate it in Settings > 通用", "请在 设置 > 通用 中停用它。")
                        .Replace("Deactivate it in 设置 > General", "请在 设置 > 通用 中停用它。")
                        .Replace("Settings > General", "设置 > 通用")
                        .Replace("General", "通用");
                    if (replaced.IndexOf("Deactivate it in", StringComparison.Ordinal) >= 0)
                        replaced = replaced.Replace("Deactivate it in", "请在");
                    if (!string.Equals(replaced, s, StringComparison.Ordinal)) return replaced;
                }
                if (s.IndexOf("Slash", StringComparison.Ordinal) >= 0)
                {
                    var replaced = s.Replace("\"Slash\"", "“斜杠”").Replace("'Slash'", "“斜杠”").Replace("Slash", "斜杠");
                    if (!string.Equals(replaced, s, StringComparison.Ordinal)) return replaced;
                }
                if (s.IndexOf("You're running the latest version", StringComparison.Ordinal) >= 0)
                    return s.Replace("You're running the latest version.", "您正在使用最新版本。").Replace("You're running the latest version", "您正在使用最新版本");
            }
            catch { }
            return null;
        }

        private static bool TryPrefix(string s, string prefix, out string rest)
        {
            rest = null;
            if (s == null) return false;
            if (!s.StartsWith(prefix, StringComparison.Ordinal)) return false;
            rest = s.Substring(prefix.Length);
            if (rest.StartsWith(" ", StringComparison.Ordinal)) rest = rest.Substring(1);
            return true;
        }

        private static string TranslateCapturedValue(string v)
        {
            if (string.IsNullOrEmpty(v)) return v;
            string trimmed = v.Trim();
            string translated;
            if (Exact.TryGetValue(trimmed, out translated) || ExactNorm.TryGetValue(Normalize(trimmed), out translated))
            {
                if (v.StartsWith(" ", StringComparison.Ordinal)) translated = " " + translated;
                if (v.EndsWith(" ", StringComparison.Ordinal)) translated = translated + " ";
                return translated;
            }
            if (v.IndexOf('\n') >= 0) return Translate(v);
            return v;
        }

        private sealed class PlaceholderRule
        {
            private readonly Regex _regex;
            private readonly string _replacement;
            private readonly int _count;

            private PlaceholderRule(Regex regex, string replacement, int count)
            {
                _regex = regex;
                _replacement = replacement;
                _count = count;
            }

            public static PlaceholderRule TryCreate(string key, string value)
            {
                try
                {
                    var matches = Regex.Matches(key, "\\{(\\d+)\\}");
                    if (matches.Count == 0) return null;
                    var sb = new StringBuilder();
                    int last = 0;
                    int max = -1;
                    foreach (Match m in matches)
                    {
                        sb.Append(Regex.Escape(key.Substring(last, m.Index - last)));
                        sb.Append("(.+?)");
                        int idx = int.Parse(m.Groups[1].Value);
                        if (idx > max) max = idx;
                        last = m.Index + m.Length;
                    }
                    sb.Append(Regex.Escape(key.Substring(last)));
                    return new PlaceholderRule(new Regex("^" + sb + "$", RegexOptions.Compiled | RegexOptions.Singleline), value, max + 1);
                }
                catch { return null; }
            }

            public string TryTranslate(string input)
            {
                var m = _regex.Match(input ?? string.Empty);
                if (!m.Success) return null;
                string output = _replacement;
                for (int i = 0; i < _count; i++)
                {
                    string v = i + 1 < m.Groups.Count ? m.Groups[i + 1].Value : string.Empty;
                    output = output.Replace("{" + i + "}", TranslateCapturedValue(v));
                }
                return output;
            }
        }
    }

    internal static class JsonObjectParser
    {
        public static Dictionary<string, string> Parse(string text)
        {
            var dict = new Dictionary<string, string>(StringComparer.Ordinal);
            int i = 0;
            SkipWs(text, ref i);
            if (i >= text.Length || text[i] != '{') throw new FormatException("Expected '{'.");
            i++;
            while (true)
            {
                SkipWs(text, ref i);
                if (i < text.Length && text[i] == '}') break;
                string key = ParseString(text, ref i);
                SkipWs(text, ref i);
                if (i >= text.Length || text[i] != ':') throw new FormatException("Expected ':'.");
                i++;
                SkipWs(text, ref i);
                string value = ParseString(text, ref i);
                dict[key] = value;
                SkipWs(text, ref i);
                if (i < text.Length && text[i] == ',') { i++; continue; }
                if (i < text.Length && text[i] == '}') break;
                if (i >= text.Length) break;
                throw new FormatException("Expected ',' or '}'.");
            }
            return dict;
        }

        private static void SkipWs(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        }

        private static string ParseString(string s, ref int i)
        {
            if (i >= s.Length || s[i] != '"') throw new FormatException("Expected string.");
            i++;
            var sb = new StringBuilder();
            while (i < s.Length)
            {
                char c = s[i++];
                if (c == '"') return sb.ToString();
                if (c != '\\') { sb.Append(c); continue; }
                if (i >= s.Length) break;
                char e = s[i++];
                switch (e)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (i + 4 > s.Length) throw new FormatException("Bad unicode escape.");
                        string hex = s.Substring(i, 4);
                        sb.Append((char)Convert.ToInt32(hex, 16));
                        i += 4;
                        break;
                    default:
                        sb.Append(e);
                        break;
                }
            }
            throw new FormatException("Unterminated string.");
        }
    }
}
