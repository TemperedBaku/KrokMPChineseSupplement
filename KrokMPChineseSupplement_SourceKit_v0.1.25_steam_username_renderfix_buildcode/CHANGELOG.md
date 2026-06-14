# Changelog

## v0.1.25 Steam username render-fix source update

- The previous JSON-only placeholder rules were not enough for setups using KrokMP Chinese Name Fix, because that plugin also patches IMGUI/GUIStyle rendering and may leave final draw text as `Steam用户名玩家名`.
- Added a compiled-code postprocess that normalizes `Steam用户名玩家名` to `Steam用户名：玩家名`.
- Added late `GUIStyle.Draw` / `DrawCursor` patch coverage so the separator fix can run closer to the final render path and after other GUI text-repair prefixes.
- This requires rebuilding the DLL from SourceKit; replacing JSON alone is not enough.


## v0.1.25 Beta
### Steam username no-separator hotfix
- Added placeholder fallback rules for KrokMP builds/UI paths that concatenate `Steam Username` directly with the Steam persona name without a colon.
- Expected display: `Steam用户名：玩家名`.
- This is a JSON/rule hotfix only; no network, Steam name, or KrokMP logic is changed.


- Updated for KrokMP / CO-OP MOD v3.1.2.
- Added `Hide locked` / locked-lobby filter translation.
- Added `Current VC Talk Keybind` translation.
- Added full disabled deactivate-button text: already playing, disconnect and go to main menu.
- Added `PACKET STABILITY` translation.
- Added network debug tooltip translations: ConnectionQuality, PacketsPerSec, BytesPerSec, PacketLoss, and related stats.
- Added KrokMP 3.1.2 load-failure and version-related fallback texts.
- Fixed the remaining `FPS: ... TPS: ...` wording to use `刻率`.

## v0.1.24 Beta

- Shortened the Steam persona label in the KrokMP lobby:
  - `Steam Username` / `Steam用户名称` → `Steam用户名`（避免大厅界面重复冒号）
- Fixed cramped display when KrokMP Unicode Name Fix restores a full Chinese Steam name in the lobby.
- Updated dynamic fallback translation for `Steam Username:` to normalize the label as `Steam用户名` and avoid duplicated colons in the lobby UI.
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


Hotfix note: Steam username label in the Steam lobby now uses `Steam用户名：` for label strings and keeps standalone `Steam用户名` without a separator, avoiding both `Steam用户名：:` and `Steam用户名玩家名` display issues.
