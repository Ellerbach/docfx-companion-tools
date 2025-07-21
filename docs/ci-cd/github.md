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
      - '**/*.md'
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

To assemble the documentation, create the documentation website and publish it to pages, you can create '/.github/workflows/doc-publish.yml` with this content:

```yaml
name: Publish Documentation

on: 
  push:
    branches:
    - main
      paths:
      - '/docs/**'
      - '**/*.md'
      - '/.github/workflows/doc-validation.yml'
  workflow_dispatch:
  
# Grant GITHUB_TOKEN the permissions required to make a Pages deployment
permissions:
  pages: write      # to deploy to Pages
  id-token: write   # to verify the deployment originates from an appropriate source

jobs:
  build:
    runs-on: ubuntu-latest
    id: build
    steps:
    - uses: actions/checkout@v4
      with:
        fetch-depth: 0
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.x
    - name: Download tools and build website
      shell: pwsh
      run: |
        dotnet tool install DocAssembler -g
        dotnet tool install DocTocTranslator -g
        dotnet tool install docfx -g
        & /.docfx/tools/GenerateDocWebsite.ps1

  deploy:
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    runs-on: ubuntu-latest
    needs: build
    steps:
    steps:
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
        path: ./out/_site
```
