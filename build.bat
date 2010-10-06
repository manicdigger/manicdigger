del /s /q output
mkdir output

copy oalinst.exe output\
xcopy /s data output\data\

REM Dll
xcopy /s ManicDigger\bin\release\*.dll output\

REM Mine mode
xcopy /s /y GameModeMine\bin\release\*.dll output\
xcopy /s /y GameModeMine\bin\release\*.exe output\
REM Fortress mode
xcopy /s /y GameModeFortress\bin\release\*.dll output\
xcopy /s /y GameModeFortress\bin\release\*.exe output\


REM GameLauncher
xcopy /s /y GameLauncher\bin\release\*.dll output\
xcopy /s /y GameLauncher\bin\release\*.exe output\

xcopy /y /s lib\*.dll output\
del output\*vshost.exe
REM copy menu.mdxs.gz output\menu.mdxs.gz
REM copy WorldGenerator.cs output\WorldGenerator.cs
copy credits.html output\credits.html
REM pause