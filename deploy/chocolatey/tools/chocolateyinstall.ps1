$ErrorActionPreference = 'Stop';

$packageName= 'docfx-companion-tools'
$version = 'v1.0.0'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = "https://github.com/Ellerbach/docfx-companion-tools/releases/download/$version/tools.zip"
$hash = 'c726597aa286436236a98b2915f93bf72632aadc248359abb3c7233fd81cb3f3'

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  url           = $url
  checksum      = $hash
  checksumType  = 'SHA256'
}

Install-ChocolateyZipPackage @packageArgs



