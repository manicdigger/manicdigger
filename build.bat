REM Windows build script

del /s /q output
mkdir output

xcopy /s data output\data\

REM Dll
xcopy /s ManicDigger.Common\bin\Release\ManicDigger.Common.dll output\

REM Scripting API
xcopy /s ManicDigger.ScriptingApi\bin\Release\ManicDigger.ScriptingApi.dll output\

REM Game Client
xcopy /s /y ManicDigger\bin\Release\*.exe output\

REM Server
xcopy /s /y ManicDigger.Server\bin\Release\*.exe output\

REM Monster editor
xcopy /s /y ManicDigger.MonsterEditor\bin\Release\*.exe output\

REM Server Mods
mkdir output\Mods
xcopy /s ManicDigger.Common\Server\Mods output\Mods\

REM Third-party libraries
xcopy /y /s Lib\*.* output\

REM NuGet packages
xcopy /s /y packages\OpenTK.2.0.0\lib\net20\OpenTK.dll output\
xcopy /s /y packages\OpenTK.2.0.0\content\OpenTK.dll.config output\
xcopy /s /y packages\protobuf-net.2.1.0\lib\net45\protobuf-net.dll output\

del output\*vshost.exe
copy COPYING.md output\credits.txt

REM pause
