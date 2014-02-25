#! /bin/bash

# build script

rm -R -f output
mkdir output

cp -R data output

# Dll
cp ManicDiggerLib/bin/Release/*.dll output

# Fortress mode
cp ManicDigger/bin/Release/*.dll output
cp ManicDigger/bin/Release/*.exe output

# Server
cp ManicDiggerServer/bin/Release/*.dll output
cp ManicDiggerServer/bin/Release/*.exe output

# Monster editor
cp MdMonsterEditor/bin/Release/*.dll output
cp MdMonsterEditor/bin/Release/*.exe output

# Mods
cp -R ManicDiggerLib/Server/Mods output

cp Lib/* output
rm -f output/*vshost.exe
cp COPYING.md output/credits.txt

# pause
