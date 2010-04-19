del /s /q output
copy oalinst.exe output\
xcopy /s data output\data\

REM Dll
xcopy /s bin\release\*.dll output\

REM Mine mode
xcopy /s /y GameModeMine\bin\release\*.dll output\
xcopy /s /y GameModeMine\bin\release\*.exe output\
REM Dungeon mode
xcopy /s /y GameModeDungeon\bin\release\*.dll output\
xcopy /s /y GameModeDungeon\bin\release\*.exe output\

xcopy /y /s lib\*.dll output\
copy multiplayer.bat output\multiplayer.bat
copy menu.mdxs.gz output\menu.mdxs.gz
REM pause