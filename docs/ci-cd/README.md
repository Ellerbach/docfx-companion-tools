# CI/CD Pipelines

To write, validate and publish documentation it is a best practice to make sure your repository uses automation for these tasks. It can be split in two main tasks:

* Validation
* Publish

## Validation

The validation task makes sure that all content of the documentation is validated. This means proper markdown formatting, but also valid (working) links. You might even want to see that there are no orphaned resources in your repository for the documentation.

For environments like GitHub, Azure DevOps or Gitlab, pipelines are used to execute these tasks. It is a best practice to use pipelines as a gate to PR's to make sure that all merged content is properly formatted and validated.

## Publish

The publish task assembles the elements of the documentation, generates a table of content and creates a static website from the content. This can be done in a pipeline as well, for example when documentation is changed in a PR. Another approach, especially with large code bases, is to have a nightly run of this pipeline to publish the documentation website.

## Implementation for various platforms

We have examples how to implement the validation and publish pipelines for some environments. If you are using another platform, you might be able to pick the main steps and intent from these examples. In the table below you can find the various platforms we described. Click the link to find the information for that platform.

From a platform you have different choices for deploying the static website. Each platform below has a Pages-concept that can be used. The Pages-concept hosts a static website next to the repository provided by the platform. That option is described with each platform.

Another option is to publish to a cloud environment like Azure. We have a description how to [deploy from an Azure DevOps environment to Azure Static Website](azure-devops-to-azure-static-website.md). This knowledge can be applied to other platforms as well.

| Platform | Description |
| --- | --- |
| [GitHub](github.md) | [GitHub](https://github.com) is a platform that provides developers with tools to create, store, manage, and share their code. It uses Git for distributed version control and offers features like access control, bug tracking, software feature requests, task management, continuous integration, and wikis for every project. |
| [Azure DevOps](azure-devops.md) | [Azure DevOps](https://azure.microsoft.com/en-us/products/devops) is a comprehensive set of tools and services provided by Microsoft to support software development and delivery. It enables teams to plan, develop, test, and deliver applications efficiently. |
| [Gitlab](gitlab.md) | [GitLab](https://about.gitlab.com/) is a comprehensive DevOps platform that provides tools for the entire software development lifecycle, from planning and source code management to CI/CD, monitoring, and security. |
