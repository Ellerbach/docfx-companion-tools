param(
    [switch] $publish = $false,
    [string] $version = '1.0.0'
)

# Include
$scriptRoot = $($MyInvocation.MyCommand.Definition) | Split-Path
. "$scriptRoot/tools/config.ps1"
. "$scriptRoot/tools/common.ps1"

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
