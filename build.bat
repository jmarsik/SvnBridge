@echo off
%windir%\Microsoft.NET\Framework\v2.0.50727\MSBuild.exe SvnBridge.msbuild /p:Configuration=Debug /logger:Kobush.Build.Logging.XmlLogger,%CodePlex3rdParty%\Kobush.Build.dll;BuildResults.xml /t:%*
