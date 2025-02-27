##########################################################################################
# GENERATE DOC WEBSITE
#
# Usage: GenerateDocWebsite.ps1 [--serve]
#   An argument can be passed, like '--serve' to start a local server
#
# This script is used to generate the documentation website. It copies all documentation
# to a /out folder. All manipulation, changes and generation is done in that folder.
#
# Prerequisitess:
#
# dotnet tool install DocFxTocGenerator -g
# dotnet tool install DocAssembler -g
# dotnet tool install DocFx -g
##########################################################################################

[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingPositionalParameters', '', Justification = 'Needed for Run() function')]
param()

##########################################################################################
# MAIN SCRIPT
##########################################################################################

# GENERAL SETTINGS
$rootFolder = $PWD
$outFolder = Join-Path $rootFolder "out"

# Assemble all content in a separate folder
Write-Output "Assembling documentation in $($outFolder)"
& DocAssembler --config ".docfx/.docassembler.json" --cleanup-output
if ($LASTEXITCODE -eq 2) {
    Write-Error "ERROR: Assembling documentation failed. Process stopped."
}
else {
    # Generate the TOCs for the various MAIN folders
    # excluding .docfx folders and docfx output folder
    $folders = Get-ChildItem $outFolder -Directory -Exclude images,template,_site
    foreach($folder in $folders)
    {
        Write-Output "Create toc.yml for $($folder.FullName)"
        & DocFxTocGenerator -d $folder.FullName -srg --indexing NoDefaultMulti --folderRef IndexReadme --ordering FoldersFirst
    }

    # Generate the website. An argument can be passed, like '--serve' to start a local server
    Write-Output "Generate website with DocFx from $($outFolder)"
    $config = Join-Path $outFolder docfx.json
    & docfx $config $args[0]
}
