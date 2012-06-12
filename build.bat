del /s /q output
mkdir output

copy OpenAL32.dll output\
xcopy /s data output\data\

REM Dll
xcopy /s ManicDiggerLib\bin\release\*.dll output\

REM Fortress mode
xcopy /s /y ManicDigger\bin\release\*.dll output\
xcopy /s /y ManicDigger\bin\release\*.exe output\
REM Server
xcopy /s /y ManicDiggerServer\bin\release\*.dll output\
xcopy /s /y ManicDiggerServer\bin\release\*.exe output\
copy /y ServerConfig.xml output\
REM Start
xcopy /s /y Start\bin\release\*.dll output\
xcopy /s /y Start\bin\release\*.exe output\

REM Monster editor
xcopy /s /y MdMonsterEditor\bin\Release\*.dll output\
xcopy /s /y MdMonsterEditor\bin\Release\*.exe output\

xcopy /y /s lib\*.dll output\
del output\*vshost.exe
copy credits.txt output\credits.txt
copy OpenTK.dll.config output\OpenTK.dll.config
REM pause