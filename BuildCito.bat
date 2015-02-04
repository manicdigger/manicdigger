del /q /s CitoOutput

mkdir CitoOutput
mkdir CitoOutput\C
mkdir CitoOutput\Java
mkdir CitoOutput\Cs
mkdir CitoOutput\JsTa

copy CitoPlatform\Cs\* CitoOutput\Cs\*
copy CitoPlatform\Java\* CitoOutput\Java\*
copy CitoPlatform\JsTa\* CitoOutput\JsTa\*
copy CitoPlatform\C\* CitoOutput\C\*

CodeGenerator Packet.proto

setlocal enabledelayedexpansion enableextensions
set LIST=
for %%x in (ManicDiggerLib\Client\*.ci.cs) do set LIST=!LIST! %%x
for %%x in (ManicDiggerLib\Client\MainMenu\*.ci.cs) do set LIST=!LIST! %%x
for %%x in (ManicDiggerLib\Client\Mods\*.ci.cs) do set LIST=!LIST! %%x
for %%x in (ManicDiggerLib\Client\Misc\*.ci.cs) do set LIST=!LIST! %%x
set LIST=%LIST:~1%
echo %LIST%

cito -D CITO -D C -l c -o CitoOutput\C\ManicDigger.c %LIST% Packet.Serializer.ci.cs
IF NOT "%1"=="fast" cito -D CITO -D JAVA -l java -o CitoOutput\Java\ManicDigger.java -n manicdigger.lib  %LIST% Packet.Serializer.ci.cs
IF NOT "%1"=="fast" cito -D CITO -D CS -l cs -o CitoOutput\Cs\ManicDigger.cs %LIST% Packet.Serializer.ci.cs

IF NOT "%1"=="fast" CitoAssets data Assets.ci.cs
IF NOT "%1"=="fast" cito -D CITO -D JS -D JSTA -l js-ta -o Html\Assets.js Assets.ci.cs
IF NOT "%1"=="fast" cito -D CITO -D JS -D JSTA -l js-ta -o CitoOutput\JsTa\ManicDigger.js %LIST% Packet.Serializer.ci.cs

IF NOT "%1"=="fast" copy CitoOutput\JsTa\* Html\*