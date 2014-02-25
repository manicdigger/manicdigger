del /s /q output
mkdir output

xcopy /s data output\data\

REM Dll
REM xcopy /s ManicDiggerLib\bin\release\*.dll output\

REM Fortress mode
xcopy /s /y ManicDigger\bin\release\*.dll output\
xcopy /s /y ManicDigger\bin\release\*.exe output\

REM Server
xcopy /s /y ManicDiggerServer\bin\release\*.dll output\
xcopy /s /y ManicDiggerServer\bin\release\*.exe output\

REM Monster editor
xcopy /s /y MdMonsterEditor\bin\Release\*.dll output\
xcopy /s /y MdMonsterEditor\bin\Release\*.exe output\

REM Mods
mkdir output\Mods
xcopy /s ManicDiggerLib\Server\Mods output\Mods\

xcopy /y /s Lib\*.* output\
del output\*vshost.exe
copy COPYING.md output\credits.txt

REM pause
