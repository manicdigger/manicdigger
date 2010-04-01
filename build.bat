del /s /q output
copy oalinst.exe output\
xcopy /s data output\data\
xcopy /s bin\release\*.exe output\
xcopy /s bin\release\*.dll output\
xcopy /y /s lib\*.dll output\
copy mine.bat output\mine.bat
copy menu.mdxs.gz output\menu.mdxs.gz
REM pause