@echo off
if NOT "%1" == "" goto :Build

REM Testing that we can commit from git

%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe SvnBridge.msbuild /p:Configuration=Debug /p:CodePlex3rdParty=..\3rdParty
goto :End

:Build
%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe SvnBridge.msbuild /p:Configuration=Debug /p:CodePlex3rdParty=..\3rdParty /t:%*

:End
