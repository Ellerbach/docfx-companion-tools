# General configuration settings for running
# the build and package scripts
$homeDir = (Resolve-Path "$PSScriptRoot\..").Path
$gitCommand = "git"
$chocoCommand = "choco"

$solution = @{
    targetFolder = "$homeDir\output"
    assetZipPath = "$homeDir\tools.zip"
}

$choco = @{
    homeDir = "$homeDir\deploy\chocolatey"
    nuspec = "$homeDir\deploy\chocolatey\docfx-companion-tools.nuspec"
    chocoScript = "$homeDir\deploy\chocolatey\tools\chocolateyinstall.ps1"
}
