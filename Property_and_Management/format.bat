@echo off
echo Running dotnet format...
dotnet format ".\Property_and_Management.csproj" -v diag
echo Done!
pause
