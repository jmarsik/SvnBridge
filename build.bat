@echo off  
%windir%\Microsoft.NET\Framework\v2.0.50727\MSBuild.exe SvnBridge.msbuild /p:Configuration=Debug /p:CodePlex3rdParty=..\3rdParty /t:%*
