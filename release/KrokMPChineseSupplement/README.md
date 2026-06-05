# KrokMP Chinese Supplement v0.1.23

这是 KrokMP / Krokosha_MP_CU 联机 Mod 的简体中文补全文本补丁。

本补丁不是 WC1.5.6.json 这类大型维基汉化包的替代品，也不会修改它。它只用自己的小型 JSON 词表补充 KrokMP 中未走原有语言表的硬编码英文，并向 KrokMP 的 Lang.Get / Locale.other 提供独立 zh-CN 兜底翻译。

## 依赖

- BepInEx 5
- KrokMP / Krokosha_MP_CU 3.0.0
- XTMP（建议安装，用于中文字体支持）

## 安装

将整个 `KrokMPChineseSupplement` 文件夹放入：

```text
BepInEx/plugins/
```

最终结构应类似：

```text
BepInEx/plugins/KrokMPChineseSupplement/
├─ KrokMPChineseSupplement.dll
├─ translations.zh-CN.json
├─ phrases.zh-CN.json
└─ README.md
```

## 词表

- `translations.zh-CN.json`：精确翻译和带占位符翻译。
- `phrases.zh-CN.json`：短语替换。

普通补词只需要修改 JSON，不需要重新编译 DLL。

## 当前补丁入口

本补丁会尝试拦截：

- `UnityEngine.UI.Text.text`
- `TMPro.TMP_Text.text`
- `GUI` / `GUILayout` 常见文本方法
- `PlayerCamera` 的 Alert / DoAlert 类方法
- `ConsoleScript` 的 Log / Console 类方法
- `KrokoshaCasualtiesMP.Lang.Get` 语言表入口（默认关闭）
- `Locale.currentLang.other` 的 krokosha_coop_* 键注入（默认关闭）
- `KrokoshaScavMultiplayer.DoMultiplayerStatusMessageLog/Error` 最后状态信息栏
- `Chat.Server_ChatAnnouncement` / 系统聊天消息
- `GUILayout_DropdownMenu.Dropdown` 下拉菜单选项

它不会修改 KrokMP 的联机逻辑、Steamworks 初始化、房间、网络包或菜单对象生命周期。

## 配置

配置文件位于：

```text
BepInEx/config/casualtiesunknown.krokmpchinesesupplement.cfg
```

正常使用无需修改。


## v0.1.5

- 补充大厅规则界面的原始规则名翻译。
- 修复 `Reset defaults` 被翻成“恢复默认s”的问题。
- 增加翻译缓存，降低与其他中文补丁共存时 GUI 每帧翻译带来的卡顿。


## v0.1.5 notes
- Fixed leftover English/partial phrase issues such as `恢复默认s`, `Steam Username`, `Close Multiplayer Mod Menu`, and `AutoContinue`.
- Disabled risky Lang.Get/Locale injection by default for compatibility with KrokMP 3.0.0; GUI/Text fallback remains active.
- Added experimental Chinese server-rule search helper.


## v0.1.5

- Added KrokMP-specific tooltip/status-message string patching.
- Improved multiline tooltip/status translation.
- Added overlay, server-browser, lobby-status, and rule-tooltip entries.
- Reworked Chinese rule search to preserve the Chinese text in the search box instead of replacing it with internal English field names.
- Fixed residual `恢复默认s` fallback.


## v0.1.11 notes

- Adds a performance safety switch: generic IMGUI translation is disabled in SampleScene by default (`Performance.EnableImGuiInGameplay=false`). This keeps gameplay playable while retaining main-menu KrokMP UI translation. Enable it only if you specifically want the in-game overlay translated and your FPS remains acceptable.
- Caches tooltip field reflection instead of resolving UIBullshit fields every frame.
- Adds additional lobby status, mood names, location names, server status messages and tooltip fallback entries.
- Keeps Chinese rule search in the menu without rewriting the search box text.


## v0.1.11

- 补充 Dried desert / Steamworks initialized / Shown / Steam lobby distance enum 等残留文本。
- 从 KrokMP 源码里的 DoMultiplayerStatusMessageLog / Error 调用补充更多最后状态信息栏文本。
- 保留 v0.1.7 起的性能安全策略，默认不在 SampleScene 做泛用 IMGUI 翻译。


## v0.1.11

- Added source-swept status/error strings from decompiled KrokMP classes.
- Patched DoMultiplayerStatusMessageError in addition to DoMultiplayerStatusMessageLog.
- Added more lobby-browser region/mood/status variants while keeping gameplay IMGUI translation disabled by default.


## v0.1.11

- Added Steam lobby enter / lobby match / lobby create status strings from decompiled KSteam.
- Added LiteNetLib DisconnectReason enum translations.
- Added Steam enum translations used as dynamic status suffixes.
- Added more TransportSteamworks connection status and error messages.
- Kept performance-safe IMGUI behavior; generic gameplay IMGUI translation remains disabled by default.


## v0.1.23

Experimental Steam display-name mode and length configuration:

- Keeps the display-only Steam persona name replacement from v0.1.18.
- Does not change KrokMP internal usernames, approval checks, network packets, save IDs, or Steam lobby metadata.
- Intended to replace displayed `??????????` style names with Steam persona names when a SteamID can be resolved.
- Adds configurable Steam display-name length, so Chinese names are no longer hard-limited to the short experimental display length.
- Adds config:

```ini
[SteamName]
EnableSteamDisplayNames = true
MaxSteamDisplayNameChars = 8
AppendEllipsisWhenTruncated = false
VerboseSteamDisplayNameLogging = false
```

Set `MaxSteamDisplayNameChars = 0` to disable display-name shortening completely. If name replacement causes any unexpected UI problem, set `EnableSteamDisplayNames = false`.


## v0.1.17

- Added a safe patch for `GUILayout_DropdownMenu.Dropdown` so custom dropdown option arrays can be translated before width calculation and drawing.
- Added chat/server-announcement string sinks so KrokMP system chat messages are translated before entering the chat log.
- Added server-side status/debug strings from decompiled `ServerMain`, `Chat`, and `KrokoshaScavMultiplayer` snippets.
- Kept `EnableImGuiInGameplay=false` as the default performance-safe behavior.


## 0.1.13

- Added targeted translation patches for in-game multiplayer interaction buttons without re-enabling generic gameplay IMGUI translation.
- Added keybind labels for point, chat, voice chat, wound view, push, carry, inventory, piggyback, and show-player-directions.
- Added extra English fallback translations for carry/piggyback/push/wound-view/inventory/spectator interaction UI.


## v0.1.17

- Added `Interact` / `Interact.` fallback translations.
- Added lag-safety config `Patch.EnableFocusableButtonPatch=false` by default. The broader `UIBullshit._GUI_FocusableButton` patch is now opt-in; the targeted `UIInGame.DoPlayerInteractionMenuButton` patch remains enabled.
- This is intended to test whether the v0.1.13 broad focusable-button patch contributed to multiplayer latency.


## v0.1.23 notes

- Added a separate short-name limit for bracketed name tags: `NameTagMaxSteamDisplayNameChars = 6`. If the closing `]` is still squeezed out, set it to `4`.
- Added direct translation for the single-player warning: `You can't play singleplayer with MP mod active. / Deactivate it in Settings > General`.
- Added Steam lobby type status translations such as `STEAM: CHANGED LOBBY TYPE TO k_ELobbyTypePublic`.


## v0.1.23

- Removed the experimental Steam Chinese display-name replacement. KrokMP's original internal/display names are left untouched to avoid nametag bracket overflow and UI layout issues.
- Fixed mixed-language status text: `Deactivate it in 设置 > 通用` now becomes `请在 设置 > 通用 中停用它。`

