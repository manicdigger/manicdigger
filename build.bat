REM Windows build script

del /s /q output
mkdir output

xcopy /s data output\data\

REM Dll
xcopy /s ManicDiggerLib\bin\release\ManicDiggerLib.dll output\

REM Scripting API
xcopy /s ScriptingApi\bin\release\ScriptingApi.dll output\

REM Game Client
xcopy /s /y ManicDigger\bin\release\*.exe output\

REM Server
xcopy /s /y ManicDiggerServer\bin\release\*.exe output\

REM Monster editor
xcopy /s /y MdMonsterEditor\bin\Release\*.exe output\

REM Server Mods
mkdir output\Mods
xcopy /s ManicDiggerLib\Server\Mods output\Mods\

REM Third-party libraries
xcopy /y /s Lib\*.* output\

del output\*vshost.exe
copy COPYING.md output\credits.txt

REM pause
