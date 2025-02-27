# The GitHub Platform

This repository is hosted on GitHub. As described in [CI/CD Pipelines](README.md) we have two tasks to implement: validation and publish. This document contains details on how to do that on GitHub.

## Validation

For validation you can create `/.github/workflows/doc-validation.yml` with this content:

```yaml
name: Documentation Validation

on: 
  pull_request:
    branches:
    - main
      paths:
      - '/docs/**'
      - '/.github/workflows/doc-validation.yml'

  workflow_dispatch:
  
jobs:
  build:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4
      with:
        fetch-depth: 0
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.x

    - name: Download tools and validate
      shell: pwsh
      run: |
        dotnet tool install DocLinkChecker -g
        DocLinkChecker --config .docfx/.doclinkchecker.json --verbose --table
        if [ $LastExitCode -eq 233 ] {
          exit 1
        }
```

## Publish
