param($installPath, $toolsPath, $package)

$modulePath = Join-Path $toolsPath Baseclass.Contrib.Nuget.GitIgnoreContent.psm1

Import-Module $modulePath

. (Join-Path $toolsPath 'GitIgnoreNugetContentRegisterEvents.ps1')

Function Create-RestoreTool {
	$modulePath = Join-Path $toolsPath Baseclass.Contrib.Nuget.GitIgnoreContent.psm1

	$module = Import-Module $modulePath -PassThru
	$packagesPath = Split-Path $installPath -Parent

	& $module { 
		param($packagesPath, $toolsPath)
		$restoreScriptPath = Join-Path $toolsPath 'RestoreNugetContent.ps1'
		$gitIgnoreUri = [System.Uri](Find-GitIgnore $packagesPath).FullName
		
		$relativeRestoreScriptPath = $gitIgnoreUri.MakeRelativeUri($restoreScriptPath).ToString().Replace('/', [System.IO.Path]::DirectorySeparatorChar)
		
		$gitRepoPath = Split-Path $gitIgnoreUri.LocalPath -Parent
		
		$restoreToolPath = Join-Path $gitRepoPath 'RestoreNugetContent.ps1'
		$powershellContent = ". '$relativeRestoreScriptPath'"
		
		Set-Content -Path $restoreToolPath $powershellContent
	} $packagesPath $toolsPath
}

Create-RestoreTool