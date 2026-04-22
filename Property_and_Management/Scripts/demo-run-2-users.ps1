$ErrorActionPreference = "Stop"
dotnet restore "../Property_and_Management.sln"
dotnet clean "../Property_and_Management.sln"
if (Test-Path "../bin") { rm "../bin" -Recurse -Force }
if (Test-Path "../obj")   { rm "../obj"   -Recurse -Force }
dotnet build "../Property_and_Management.sln" -c Debug --force -p:Platform=x64
$src = "../bin/x64/Debug/net8.0-windows10.0.19041.0/win-x64"

cp $src "../../demo-builds/user1" -Recurse -Force
cp $src "../../demo-builds/user2" -Recurse -Force
Start-Process "dotnet" -ArgumentList "run --project ../../NotificationServer/NotificationServer.csproj"
Start-Sleep -Seconds 2
Start-Process "../../demo-builds/user1/win-x64/Property_and_Management.exe" -ArgumentList "1"
Start-Process "../../demo-builds/user2/win-x64/Property_and_Management.exe" -ArgumentList "2"
