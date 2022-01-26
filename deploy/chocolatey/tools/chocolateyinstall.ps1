$ErrorActionPreference = 'Stop';

$packageName= 'docfx-companion-tools'
$version = 'v1.0.1'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = "https://github.com/Ellerbach/docfx-companion-tools/releases/download/$version/tools.zip"
$hash = '165611d397601fc0f4cd87503108abab104b23c8ef0a2b56efd2b5d3eab6ab15'

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  url           = $url
  checksum      = $hash
  checksumType  = 'SHA256'
}

Install-ChocolateyZipPackage @packageArgs
