# This is the script to package and deploy the zip-file in the root.
# If the zip-file doesn't exist, we'll run the build-script first.
#
# If you provide the -publish flag, we will publish. Otherwise the
# script runs in debug mode, outputting the changed files.
#
# Provide the version of the package with the -version <version>
# parameter. Only use the major.minor.patch format, like 1.0.0.
#
# The publish process depends on the CHOCO_TOKEN environment variable
# set with the key for publishing to Chocolatey. If that is not set
# the script will do everything except publishing to Chocolatey with
# a warning.
param(
    [switch] $publish = $false,
    [string] $version = '1.0.0'
)

# Include
$scriptRoot = $($MyInvocation.MyCommand.Definition) | Split-Path
. "$scriptRoot/tools/config.ps1"
. "$scriptRoot/tools/common.ps1"

# Check if the zip-file exists, otherwise we'll run the build script first
if (-not(Test-Path $solution.assetZipPath)) {
    Write-Warning "$($solution.assetZipPath) doesn't exist. We'll build first."
    & .\build.ps1
}

$hash = (Get-FileHash -Algorithm SHA256 -Path $solution.assetZipPath).Hash.ToLower()
$nupkgName = "docfx-companion-tools.$version.nupkg"

UpdateChocoConfig $choco.chocoScript $choco.nuspec $version $hash

if ($publish) {
    # create the nuspec package
    & $chocoCommand pack $choco.nuspec

    # if token is given, we will publish the package to Chocolatey here
    if ($env:CHOCO_TOKEN) {
        & $chocoCommand apiKey -k $env:CHOCO_TOKEN -source https://push.chocolatey.org/
        & $chocoCommand push $nupkgName
    } else {
        Write-Warning "Chocolatey token was not set. Publication skipped."
    }
} else {
    # For development/debuggin purposes
    $script = Get-Content $choco.chocoScript -Encoding UTF8 -Raw
    Write-Host "================= Choco Script ====================="
    Write-Host $script
    Write-Host "===================================================="

    $nuspec = Get-Content $choco.nuspec -Encoding UTF8 -Raw
    Write-Host "================== Nuspec ==========================="
    Write-Host $nuspec
    Write-Host "===================================================="

    Write-Host "$chocoCommand pack " $choco.nuspec
    Write-Host "$chocoCommand apiKey -k $env:CHOCO_TOKEN -source https://push.chocolatey.org/"
    Write-Host "$chocoCommand push $nupkgName"
}
