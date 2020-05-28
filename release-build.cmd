@echo off
call "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat"
cd /d "%~dp0"
msbuild LogTalk.sln /fileLogger /t:Restore;LogTalk:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
pause
