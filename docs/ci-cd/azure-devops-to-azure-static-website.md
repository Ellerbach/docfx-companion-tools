# Azure DevOps deploy to Azure Web App

## Serving specific files on Azure App Services

If you start customizing the DocFX templates, you'll most likely end up having to support specific files for the search and for the fonts. Depending where you deploy them, this may require to adjust your web server. In the case of deployment in an Azure Web Application, you will need to adjust the `web.config` file:

```xml
<?xml version="1.0"?>
<configuration>
  <system.webServer>
    <staticContent>
      <!-- remove first in case they are defined in IIS already, which would cause a runtime error -->
      <remove fileExtension=".yml" />
      <remove fileExtension=".json" />
      <remove fileExtension=".woff" />
      <remove fileExtension=".woff2" />
      <mimeMap fileExtension=".yml" mimeType="text/yaml" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <mimeMap fileExtension=".woff" mimeType="application/woff" />
      <mimeMap fileExtension=".woff2" mimeType="application/woff2" />
    </staticContent>
  </system.webServer>
</configuration>
```

If the [DocAssembler](~/src/DocAssembler/README.md) is used, make sure that the `web.config` file is copied to the output folder as well. And you also need to add it to the `docfx.json` file to add the `web.config` file as resource. This way it is also deployed. This could be done like this:

```json
{
  "build": {
    "content": [
      { "files": [ "**/*.{md,yml}" ] }
    ],
    "resource": [
      { "files": ["web.config", "**/.attachments/**", "**/assets/**"] }
    ],
    "dest": "_site",
```

```yaml
trigger: none

pool:
  vmImage: windows-latest

jobs:
# Scan markdown files on style consistency
- job:
  displayName: Install markdownlint
  steps:
    - displayName: 'Install markdownlint'
      bash: npm install -g markdownlint-cli

    - displayName: Run markdownlint
      env:
        WORKDIR: $(System.DefaultWorkingDirectory)
        CONFIGFILE: $(System.DefaultWorkingDirectory)/.markdownlint.json
      bash: markdownlint -c $CONFIGFILE $WORKDIR

# install the necessary tools
- displayName: Instal DocLinkChecker and run
  powershell: |
    dotnet tool install DocLinkChecker -g
    DocLinkChecker -f ".docfx/.doclinkchecker.json"
  displayName: 'Checking links in .\DocExamples'
```

```yaml
###########################################################################
# This is a sample Azure DevOps pipeline that can be used for generating 
# a documentation website using DocFX. In this pipeline we use the 
# DocFxTocGenerator tool to generate the table of contents.
###########################################################################
trigger:
- none

variables:
- name: AzureConnectionName
  value: '<Azure connection name from ADO>'

pool:
  vmImage: windows-latest

steps:
# install docfx
- displayName: Install the tools
  powershell: |
    dotnet tool install docfx -g
    dotnet tool install DocAssembler -g
    dotnet tool install DocFxTocGenerator -g
  
# run the toc generator on /DocExamample folder
- displayName: Generating documentation
  powershell: |
    & .docfx/tools/GenerateDocWebsite.ps1

  # Create an archive
- task: ArchiveFiles@2
  displayName: 'Packing Documentation Web Site'
  inputs:
    rootFolderOrFile: '$(System.DefaultWorkingDirectory)/out/_site'
    archiveType: 'zip'
    archiveFile: '$(Build.ArtifactStagingDirectory)/site.zip'
    replaceExistingArchive: true

# deployment to Azure
- task: AzureRmWebAppDeployment@4
  displayName: 'Publish website to Azure App Service'
  inputs:
    azureSubscription: $(AzureConnectionName)
    WebAppName: docs-website
    packageForLinux: '$(Build.ArtifactStagingDirectory)/site.zip'
```

