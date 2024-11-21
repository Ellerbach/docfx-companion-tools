# This is the script to build and zip the executables
# from the solutions.

# Include settings and common functions
$scriptRoot = $($MyInvocation.MyCommand.Definition) | Split-Path
. "$scriptRoot/tools/config.ps1"
. "$scriptRoot/tools/common.ps1"

# Clean output first
if (Test-Path -Path $solution.targetFolder) {
    Remove-Item $solution.targetFolder -Recurse
}
if (Test-Path -Path $solution.assetZipPath) {
    Remove-Item $solution.assetZipPath
}

# Build all dotnet projects into $solution.targetFolder as single exe's. Skip Test projects.
foreach ($sln in (Get-ChildItem -Recurse src\*\*.csproj -Exclude *.Test.*)) {
    Write-Host "Building $($sln.FullName)"
    & dotnet publish $sln.FullName -c Release -r win-x64 /p:PublishSingleFile=true /p:CopyOutputSymbolsToPublishDirectory=false --self-contained false -o $solution.targetFolder
}

# Package NuGet packages
Write-Host "Package .\src\DocFxCompanionTools.sln"
dotnet pack .\src\DocFxCompanionTools.sln -c Release -p:PackAsTool=true -o ./artifacts
Get-ChildItem ./artifacts

# remove possible generated XML documentation files
Remove-Item "$($solution.targetFolder)\*.xml"
# Copy license to the folder to package
Copy-Item LICENSE $solution.targetFolder
# Zip targetFolder
PackAssetZip $solution.targetFolder $solution.assetZipPath