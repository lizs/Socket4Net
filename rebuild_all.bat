msbuild PI35.sln /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:TargetFrameworkVersion=v3.5
msbuild PI.sln /t:Rebuild /p:Configuration=Release /p:Platform="X64" /p:TargetFrameworkVersion=v4.5

call "send3`5Core.bat"

