#! /bin/bash

# build script

rm -R -f ouput
mkdir output

cp OpenAL32.dll ouput
cp -R data output

# Dll
cp ManicDigger/bin/Release/*.dll output

# Fortress mode
cp GameModeFortress/bin/Release/*.dll output
cp GameModeFortress/bin/Release/*.exe output

# Server
cp ManicDiggerServer/bin/Release/*.dll output
cp ManicDiggerServer/bin/Release/*.exe output
cp ServerConfig.xml output

# Start
#cp Start/bin/Release/*.dll output
#cp Start/bin/Release/*.exe output

# Monster editor
cp MdMonsterEditor/bin/Release/*.dll output
cp MdMonsterEditor/bin/Release/*.exe output

#cp lib/*.dll output
rm -f output/*vshost.exe
cp credits.txt output
cp OpenTK.dll.config output

# pause
