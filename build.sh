#!/usr/bin/env bash
set -euo pipefail
GAME_DIR="${1:-/mnt/game/Casualties Unknown Demo}"
dotnet build ./src/KrokMPChineseSupplement/KrokMPChineseSupplement.csproj -c Release -p:GameDir="$GAME_DIR"
rm -rf ./release/KrokMPChineseSupplement
mkdir -p ./release/KrokMPChineseSupplement
cp ./src/KrokMPChineseSupplement/bin/Release/KrokMPChineseSupplement.dll ./release/KrokMPChineseSupplement/
cp ./translations/*.json ./release/KrokMPChineseSupplement/
cp ./README.md ./release/KrokMPChineseSupplement/README.md
(cd ./release && zip -r ../KrokMPChineseSupplement_release.zip KrokMPChineseSupplement)
echo "Built ./KrokMPChineseSupplement_release.zip"
