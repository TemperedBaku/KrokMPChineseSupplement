#!/usr/bin/env bash
set -euo pipefail
GAME_DIR="${1:-/mnt/game/Casualties Unknown Demo}"
dotnet build ./src/KrokMPChineseSupplement/KrokMPChineseSupplement.csproj -c Release -p:GameDir="$GAME_DIR"
rm -rf ./release/KrokMPChineseSupplement
mkdir -p ./release/KrokMPChineseSupplement
cp ./src/KrokMPChineseSupplement/bin/Release/KrokMPChineseSupplement.dll ./release/KrokMPChineseSupplement/
cp ./translations/*.json ./release/KrokMPChineseSupplement/
cp ./README.md ./release/KrokMPChineseSupplement/README.md
(cd ./release && zip -r ../KrokMPChineseSupplement_v0.1.25_Beta_steam_username_renderfix.zip KrokMPChineseSupplement)
echo "Built ./KrokMPChineseSupplement_v0.1.25_Beta_steam_username_renderfix.zip"
