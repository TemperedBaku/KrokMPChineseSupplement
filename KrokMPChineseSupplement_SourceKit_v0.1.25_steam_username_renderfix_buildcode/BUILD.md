# Build instructions

This SourceKit contains the Steam username render-fix source changes. It must be compiled locally before producing a new DLL.

Windows PowerShell example:

```powershell
cd "D:\ModWork\KrokMPChineseSupplement_SourceKit_v0.1.25_steam_username_renderfix_buildcode"
.\build.ps1 -GameDir "D:\steam\steamapps\common\Casualties Unknown Demo"
```

If your game is installed elsewhere, replace `-GameDir` with the actual game folder, for example:

```powershell
.\build.ps1 -GameDir "D:\SteamLibrary\steamapps\common\Casualties Unknown Demo"
```

Build output:

```text
release/KrokMPChineseSupplement/KrokMPChineseSupplement.dll
release/KrokMPChineseSupplement/translations.zh-CN.json
release/KrokMPChineseSupplement/phrases.zh-CN.json
KrokMPChineseSupplement_v0.1.25_Beta_steam_username_renderfix.zip
```

Install by copying the whole `release/KrokMPChineseSupplement/` folder to:

```text
Casualties Unknown Demo/BepInEx/plugins/KrokMPChineseSupplement/
```

---

# Build instructions

Windows / PowerShell:

```powershell
cd "D:\ModWork\KrokMPChineseSupplement_SourceKit_v0.1.25_krokmp312_steam_username_separator_hotfix_buildcode"
.\build.ps1 -GameDir "D:\steam\steamapps\common\Casualties Unknown Demo"
```

Output:

```text
release/KrokMPChineseSupplement/KrokMPChineseSupplement.dll
KrokMPChineseSupplement_v0.1.25_Beta_steam_username_separator_hotfix.zip
```

Install the generated `release/KrokMPChineseSupplement` folder to:

```text
Casualties Unknown Demo/BepInEx/plugins/KrokMPChineseSupplement/
```
