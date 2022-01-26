# Include
$scriptRoot = $($MyInvocation.MyCommand.Definition) | Split-Path
. "$scriptRoot/tools/config.ps1"
. "$scriptRoot/tools/common.ps1"

# Build all dotnet solution
foreach ($sln in (Get-ChildItem -Recurse src\*.sln)) {
    Write-Host "Start building $($sln.FullName)"

    & dotnet publish $sln.FullName -c Release -r win-x64 /p:PublishSingleFile=true /p:CopyOutputSymbolsToPublishDirectory=false --self-contained false -o $solution.targetFolder
}

# remove possible XML documentation files
Remove-Item output\*.xml
# Copy license to the folder to package
Copy-Item LICENSE $solution.targetFolder
# Zip targetFolder
PackAssetZip $solution.targetFolder $solution.assetZipPath