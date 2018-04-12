#! /bin/bash
# Linux build script

rm -R -f output
mkdir output

cp -R data output

# Dll
cp ManicDigger.Common/bin/Release/ManicDigger.Common.dll output

# Scripting API
cp ManicDigger.ScriptingApi/bin/Release/ManicDigger.ScriptingApi.dll output

# Game Client
cp ManicDigger/bin/Release/*.exe output

# Server
cp ManicDigger.Server/bin/Release/*.exe output

# Monster editor
cp ManicDigger.MonsterEditor/bin/Release/*.exe output

# Server Mods
cp -R ManicDigger.Common/Server/Mods output

# Third-party libraries
cp Lib/* output

# NuGet packages
cp packages/OpenTK.2.0.0/lib/net20/OpenTK.dll output
cp packages/OpenTK.2.0.0/content/OpenTK.dll.config output
cp packages/protobuf-net.2.1.0/lib/net45/protobuf-net.dll output

rm -f output/*vshost.exe
cp COPYING.md output/credits.txt

# pause
