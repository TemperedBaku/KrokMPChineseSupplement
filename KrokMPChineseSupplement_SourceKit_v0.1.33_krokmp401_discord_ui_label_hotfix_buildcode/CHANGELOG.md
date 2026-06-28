# Changelog

## v0.1.33 Beta

- Added exact and dynamic fallback translations for Steam connection-close status text.
- Fixed `ClosedByPeer` and the mixed artifact `关闭dByPeer`, rendering them as `对方关闭连接`.
- Fixed strings such as `Steam: Connection closed: ClosedByPeer` / `Steam: Connection closed: 关闭dByPeer` to `Steam：连接已关闭：对方关闭连接`.
- No networking, Steamworks, lobby, packet, player validation, or world-sync logic changes.


## v0.1.33 Beta

- Shortened `Hide locked` / password-protected lobby filter text to `隐藏密码房` to avoid clipping in the server browser layout.
- Clarified Steam lobby identifier wording: `Copy Lobby Id` is now `复制房间ID`, not `复制房间码`, because the value copied is the Steam Lobby ID.
- Added exact and dynamic fallback handling for `Password:` / `Password:  ` so KrokMP password fields can render as `房间密码：`.
- Kept bare `Password` as `密码` to avoid over-translating unrelated password contexts.

## v0.1.33 Beta

- Changed `Hide locked` to `隐藏密码房` because KrokMP filters password-protected rooms via `haspassword`, not Steam locked/private lobbies.
- Changed multiplayer password field labels to `房间密码` / `房间密码：`.
- Changed `Copy Lobby Id` and related Lobby ID labels to `复制房间ID` / `房间ID`.
- Updated password-related messages such as wrong password and password protection notices.
- Kept scope limited to KrokMP / CO-OP MOD UI texts.

## v0.1.33 Beta

- Scope cleanup for KrokMP / CO-OP MOD v4.0.1 translation supplement.
- Removed generic non-KrokMP UI fallback entries from v0.1.28, including `You're almost there. Just a couple more steps.` and `Also try CUCoreLib!`.
- Kept verified KrokMP-related v4.0.1 entries, including lobby browser labels, rule/tooltips, `PACKET STABILITY`, multiplayer tutorial message, and `STARTING GAME` status text.
- Kept the `Last Stand` terminology correction: `背水一战`.
- No networking, Steamworks, lobby, packet, player validation, or world-sync logic changes.
# Changelog

## v0.1.33 Beta

- Fixed KrokMP 4.0.1 server-browser raw labels such as `Layer` and `Mods` when they bypass `Lang.Get`.
- Changed `LastStandAllowed` wording from `最后一搏` to `背水一战`.
- Added missing tutorial / pause overlay strings such as `You're almost there. Just a couple more steps.` and `Also try CUCoreLib!`.
- Added raw exact fallback entries for KrokMP `Lang.EN` values so UI paths that draw English text directly are translated more reliably.


## v0.1.27 Beta

- Updated target compatibility notes for KrokMP / CO-OP MOD v4.0.1.
- Added missing KrokMP 4.0.1 `krokosha_coop_*` language entries.
- Added translations for new/previously missing lobby and host UI entries:
  - `Copy Lobby Id`
  - `Go back to Main Menu`
  - `Changed rules:`
  - `Rules are default.`
  - `Move away your mouse`
- Added translations for admin/player controls:
  - `IP Ban`
  - `TC Mute`
  - `VC Mute`
  - `TC Muted`
  - `VC Muted`
- Added translations for Steam lobby type description and enforced mod-list settings.
- Added translations for server browser details:
  - `Hide incompatible`
  - `In Debug World`
  - `Mods`
  - `Players`
  - `Rules`
  - `Loading`
  - `Layer`
- Added translations for Discord RPC join request UI.
- Added KrokMP 4.0.1 version/load-failure/save-version mismatch fallback strings.
- Fixed `mmsb_lobbyfounds` placeholder coverage for total player count: `{0}`, `{1}`, `{2}`.
- Added missing rule-name fallbacks for KrokMP 4.0.1 rule fields such as `SleepingMute`, `AdditionalBrainRegen`, and `LastStandAllowed`.

## v0.1.25 Steam username render-fix source update

- The previous JSON-only placeholder rules were not enough for setups using KrokMP Chinese Name Fix, because that plugin also patches IMGUI/GUIStyle rendering and may leave final draw text as `Steam用户名玩家名`.
- Added a compiled-code postprocess that normalizes `Steam用户名玩家名` to `Steam用户名：玩家名`.
- Added late `GUIStyle.Draw` / `DrawCursor` patch coverage so the separator fix can run closer to the final render path and after other GUI text-repair prefixes.
- This requires rebuilding the DLL from SourceKit; replacing JSON alone is not enough.

## v0.1.25 Beta

- Updated for KrokMP / CO-OP MOD v3.1.2.
- Added `Hide locked` / locked-lobby filter translation.
- Added `Current VC Talk Keybind` translation.
- Added full disabled deactivate-button text: already playing, disconnect and go to main menu.
- Added `PACKET STABILITY` translation.
- Added network debug tooltip translations: ConnectionQuality, PacketsPerSec, BytesPerSec, PacketLoss, and related stats.
- Added KrokMP 3.1.2 load-failure and version-related fallback texts.
- Fixed the remaining `FPS: ... TPS: ...` wording to use `刻率`.

## v0.1.24 Beta

- Shortened/normalized the Steam persona label in the KrokMP lobby.
- Fixed cramped display when KrokMP Unicode Name Fix restores a full Chinese Steam name in the lobby.
- Cleaned README version/dependency notes and removed obsolete XTMP / Steam display-name experiment wording.

## v0.1.23 Beta

- Updated for KrokMP / CO-OP MOD v3.1.0 and Casualties Unknown Demo v7.0.1.
- Added Battle Royale mode text translations.
- Added TPS / FPS display translations.
- Added `AlwaysAllowCarry` rule name and tooltip translation.
- Added new voice mode text such as `Toggle to talk`.
- Added new KrokMP 3.1.0 `krokosha_coop_*` language keys.
- Added new location and mood text entries from Wiki Chinese v1.6.5.
- Removed the experimental Steam Chinese display-name replacement to avoid breaking nametag layout.

## v0.1.22 Beta

- Completed the main KrokMP 3.0.0 multiplayer menu, server browser, rule menu, status message, chat, and tooltip translations.
- Kept Chinese rule search support.
- Disabled broad gameplay IMGUI translation by default for lag safety.
