REM Generate ProtoBuf files
CodeGenerator Packet.proto

REM Clean cito output directory
del /q /s cito\output

REM Create output directories
mkdir cito\output
mkdir cito\output\JsTa

REM Create list of input files
setlocal enabledelayedexpansion enableextensions
set LIST=
for %%x in (ManicDiggerLib\Client\*.ci.cs) do set LIST=!LIST! %%x
for %%x in (ManicDiggerLib\Client\MainMenu\*.ci.cs) do set LIST=!LIST! %%x
for %%x in (ManicDiggerLib\Client\Mods\*.ci.cs) do set LIST=!LIST! %%x
for %%x in (ManicDiggerLib\Client\Misc\*.ci.cs) do set LIST=!LIST! %%x
for %%x in (ManicDiggerLib\Client\SimpleServer\*.ci.cs) do set LIST=!LIST! %%x
for %%x in (ManicDiggerLib\Client\UI\*.ci.cs) do set LIST=!LIST! %%x
for %%x in (ManicDiggerLib\Client\UI\Screens\*.ci.cs) do set LIST=!LIST! %%x
for %%x in (ManicDiggerLib\Common\*.ci.cs) do set LIST=!LIST! %%x
set LIST=%LIST:~1%
echo %LIST%

REM Compile JavaScript files
IF NOT "%1"=="fast" CitoAssets data Assets.ci.cs
IF NOT "%1"=="fast" cito -D CITO -D JS -D JSTA -l js-ta -o cito\output\JsTa\Assets.js Assets.ci.cs
IF NOT "%1"=="fast" cito -D CITO -D JS -D JSTA -l js-ta -o cito\output\JsTa\ManicDigger.js %LIST% Packet.Serializer.ci.cs

REM Copy skeleton files
copy cito\platform\JsTa\* cito\output\JsTa\*

REM mkdir cito\output\C
REM mkdir cito\output\Java
REM mkdir cito\output\Cs

REM IF NOT "%1"=="fast" cito -D CITO -D C -l c -o cito\output\C\ManicDigger.c %LIST% Packet.Serializer.ci.cs
REM IF NOT "%1"=="fast" cito -D CITO -D JAVA -l java -o cito\output\Java\ManicDigger.java -n manicdigger.lib  %LIST% Packet.Serializer.ci.cs
REM IF NOT "%1"=="fast" cito -D CITO -D CS -l cs -o cito\output\Cs\ManicDigger.cs %LIST% Packet.Serializer.ci.cs

REM copy cito\platform\C\* cito\output\C\*
REM copy cito\platform\Java\* cito\output\Java\*
REM copy cito\platform\Cs\* cito\output\Cs\*