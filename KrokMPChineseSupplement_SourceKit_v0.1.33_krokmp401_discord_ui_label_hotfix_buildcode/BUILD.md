# Build KrokMP Chinese Supplement v0.1.33 Beta

Discord UI label hotfix buildcode package.

## Windows PowerShell

解压本 SourceKit 到例如：

```text
D:\ModWork\KrokMPChineseSupplement_SourceKit_v0.1.33_krokmp401_discord_ui_label_hotfix_buildcode
```

然后执行：

```powershell
cd "D:\ModWork\KrokMPChineseSupplement_SourceKit_v0.1.33_krokmp401_discord_ui_label_hotfix_buildcode"
.\build.ps1 -GameDir "D:\steam\steamapps\common\Casualties Unknown Demo"
```

如果你的游戏路径不同，请把 `-GameDir` 改成实际游戏目录，例如：

```powershell
.\build.ps1 -GameDir "D:\SteamLibrary\steamapps\common\Casualties Unknown Demo"
```

## Output

编译成功后会生成：

```text
release/KrokMPChineseSupplement/KrokMPChineseSupplement.dll
release/KrokMPChineseSupplement/translations.zh-CN.json
release/KrokMPChineseSupplement/phrases.zh-CN.json
KrokMPChineseSupplement_v0.1.33_Beta_krokmp401_discord_ui_label_hotfix.zip
```

安装测试时，把整个：

```text
release/KrokMPChineseSupplement/
```

覆盖到：

```text
Casualties Unknown Demo/BepInEx/plugins/KrokMPChineseSupplement/
```
