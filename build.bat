@echo off  
%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe SvnBridge.msbuild /p:Configuration=Debug /p:CodePlex3rdParty=..\3rdParty /t:%*
