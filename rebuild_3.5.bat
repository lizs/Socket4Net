msbuild PI35.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:TargetFrameworkVersion=v3.5

call "send3`5Core.bat"

