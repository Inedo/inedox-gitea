@echo off

dotnet new tool-manifest --force
dotnet tool install inedo.extensionpackager

cd Gitea\InedoExtension
dotnet inedoxpack pack . C:\LocalDev\BuildMaster\Extensions\Gitea.upack --build=Debug -o
cd ..\..