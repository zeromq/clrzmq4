$PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition

$modulePath = Join-Path $PSScriptRoot Baseclass.Contrib.Nuget.GitIgnoreContent.psm1

Import-Module $modulePath

Restore-NugetContentFiles $PSScriptRoot