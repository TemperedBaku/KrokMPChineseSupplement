# Changelog

## v0.1.36 Beta

- Fixed warning spam caused by repeatedly probing the wrong `Locale` type for `currentLang` / `currentLangName`.
- Reworked KrokMP 4.1.2 `Lang.Get` fallback and `Lang.dict` injection.
- Creates / fills KrokMP `Lang.dict` entries for the active Chinese language name and common Chinese pack aliases.
- Disables failed Locale injection after safe reflection probing instead of retrying through HarmonyX `AccessTools` every frame.
- Keeps v0.1.35 text additions such as chat-key, server-browser sort labels, and KrokMP 4.1.2 status/menu strings.
- No KrokMP networking, Steamworks, lobby, packet, world-sync, or player-validation logic changes.

## v0.1.35 Beta

- Added KrokMP 4.1.2 missing text fallbacks found during testing.
- Added `Current Chat Keybind`, `Press "/" to chat...`, and server-browser sort labels.
- Added preliminary KrokMP `Lang.Get` / language-dictionary fallback work.

## v0.1.34 Beta

- Scope isolation: global UI/TMP/IMGUI hooks no longer run broad phrase replacement by default.
- Deferred KrokMP-specific reflection patches until the KrokMP assembly is loaded.
- Disabled unstable direct player interaction button patch by default.
- Added diagnostics/log-cleanup controls.

## v0.1.33 Beta

- Fixed Discord Rich Presence setting labels in the KrokMP settings panel.
- Added fallbacks for partially translated `Discord 状态显示 Join button` artifacts.

## v0.1.32 Beta

- Added connection-close status translations such as `ClosedByPeer` → `对方关闭连接`.

## v0.1.31 Beta

- Shortened password-protected lobby filter text to `隐藏密码房`.
- Clarified `Copy Lobby Id` as `复制房间ID`.
- Added password field fallback handling.

## v0.1.30 Beta

- Added room-password / lobby-ID terminology fixes.

## v0.1.29 Beta

- Scope cleanup for KrokMP / CO-OP MOD texts only.

## v0.1.28 Beta

- Added KrokMP 4.0.1 server-browser raw label fallbacks and Last Stand wording updates.

## v0.1.27 Beta

- Added KrokMP 4.0.1 lobby, rules, tooltip, server-browser, admin/player-control, and status-message fallbacks.

## v0.1.25 Beta

- Added Steam username label render fix and KrokMP 3.1.x text updates.

## v0.1.24 Beta

- Cleaned Steam persona label wording and dependency notes.

## v0.1.23 Beta

- Initial public KrokMP Chinese Supplement baseline for KrokMP / Krokosha_MP_CU 3.x.
