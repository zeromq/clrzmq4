# ------------------------------------------------------------------------------------------------------
#	Name:			Baseclass.Contrib.Nuget.GitIgnoreContent
#	Description: 	Module to ignore nuget content files in git
#	Author:			D. Romero (baseclass GmbH)
# ------------------------------------------------------------------------------------------------------

# ------------------------------------------------------------------------------------------------------
#	Constants
# ------------------------------------------------------------------------------------------------------

$NugetPackageContentIgnoreSectionStart = "# START NugetPackageContentIgnoreSection -- automatically generated do not modify manually"
$NugetPackageContentIgnoreSectionEnd = "# END NugetPackageContentIgnoreSection"

# ------------------------------------------------------------------------------------------------------
#	Private helpers
# ------------------------------------------------------------------------------------------------------

Function FindOrCreateGitIgnore {
	param($path, $create)
	
	if($create -eq $null) {
		$create = $true
	}
	
	$repoPath = Find-GitRepositoryPath $path
	
	if($repoPath -eq $null) {
		Write-Error "Is no git repository!"
		break;
	}
	
	#Found git repository look for .gitignore file
	if(Test-Path $(Join-Path $repoPath ".gitignore")) {	
		return $(gi $(Join-Path $repoPath ".gitignore"))
	} elseif ($create) {
		#Create .gitignore file
		return $(New-Item -Path $repoPath -Name ".gitignore" -ItemType "file")
	}
}

Function FindNugetPackageContentIgnoreSection {
	param($gitignore)
	
	return FindOrCreateNugetPackageContentIgnoreSection $gitignore $false
}

Function FindOrCreateNugetPackageContentIgnoreSection {
	param($gitignore, $create)
	
	if($create -eq $null) {
		$create = $true
	}
	
	$lineFound = $false
	$lineNr = 0
	
	$lines = get-content $gitignore
	
	foreach($line in $lines) {
		if($line -eq $NugetPackageContentIgnoreSectionStart) {
			$lineFound = $true
			break;
		}
		$lineNr++
	}
	
	if(-not $lineFound -and $create) {
		Add-Content $gitignore "`r`n`r`n$NugetPackageContentIgnoreSectionStart"
		Add-Content $gitignore ""
		Add-Content $gitignore "$NugetPackageContentIgnoreSectionEnd"
		$lineNr = $lineNr + 1
	}
	
	return $lineNr
}

Function GetCompatibleContentFiles {
	param($package, $project)
	
	$targetFrameworkMoniker = $project.Properties.Item("TargetFrameworkMoniker").Value
	
	$targetFramework = (new-object System.Runtime.Versioning.FrameworkName($targetFrameworkMoniker))
	
	$packageFiles = $package.GetFiles() | where { $_.Path.StartsWith("content\", [System.StringComparison]::InvariantCultureIgnoreCase) }
	
	$iPackageFileType = ([type]"Nuget.IPackageFile, $($package.GetType().Assembly.FullName)");
	
	$genericType = [System.Collections.Generic.List`1].MakeGenericType($iPackageFileType)
	$packageFilesList = [System.Activator]::CreateInstance($genericType, @());
	
	foreach($file in $packageFiles) {
		$packageFilesList.Add($file);
	}
	
	$method = ([type]"NuGet.VersionUtility, $($package.GetType().Assembly.FullName)").GetMethod("TryGetCompatibleItems")
	$genericMethod = $method.MakeGenericMethod($iPackageFileType)
	$args = @([System.Runtime.Versioning.FrameworkName]$targetFramework, $packageFilesList, $null)
	
	if($genericMethod.Invoke($null, $args)) {
		return $args[2];
	} else {
		return @()
	}
}

Function WriteIgnoreSection {
	param($id, $version, $packagePath, $delimiter)
	
	$ignoreSection = $null
	
	$contentPath = (Join-Path $packagePath "Content")
	if(Test-Path $contentPath) {
		$gitIgnorePath = FindOrCreateGitIgnore $packagePath
		$uriGitIgnore = [System.Uri]($gitIgnorePath.FullName);
		
		# add all content files for projects to gitignore
		Get-Project -All | ForEach-Object {
			$package = (Get-Package -ProjectName $_.Name | Where { $_.Id -eq $id -and $_.Version -eq $version })
			$packageInstalled = ($package -ne $null)
			
			if($packageInstalled)
			{
				$projectDir = (gi -Path $_.FullName).Directory.FullName
				$contentFiles = GetCompatibleContentFiles $package $_
				
				foreach($file in $contentFiles)
				{
					$sourceRelativePath = $uriGitIgnore.MakeRelativeUri((Join-Path $packagePath $file.Path)).ToString()
					$uriFullPath = [System.Uri](Join-Path $projectDir $file.EffectivePath);
					$relativePath = $uriGitIgnore.MakeRelativeUri($uriFullPath).ToString()
					if($ignoreSection -eq $null) {
						$ignoreSection = ""
					}
					$ignoreSection += "#-source:$sourceRelativePath$delimiter"
					$ignoreSection += "$relativePath$delimiter"
				}
				if($ignoreSection -ne $null)
				{
					$ignoreSection += "$delimiter"
				}
			}
		}		
	} else {
		#Package has no content
	}
	
	return $ignoreSection
}

Function IgnoreUnignorePackage {
	param($id, $version, $packagePath, $gitignore, [int]$sectionStart, $ignoreMessage)
	
	Write-Host "$ignoreMessage package $id $version"
	
	$startPackageSection = "# $id $version"	
	
	$fileContent = ""
	$ignoreSectionWritten = $false;
	
	
	$origContent = [IO.File]::ReadAllText($gitignore.FullName)
	
	$delimiter = "`r`n"
	
	if(-not $origContent.Contains($delimiter)) {
		$delimiter = "`n"
	}
	
	$lines = $origContent.Split(@($delimiter), [System.StringSplitOptions]::None)
	
	for($lineNr = 0; $lineNr -lt $lines.Length; $lineNr++)
	{
		$line = $lines[$lineNr]
		if($lineNr -gt $sectionStart -and -not $ignoreSectionWritten)
		{
			$writeIgnoreSection = $false
			if($line -eq $startPackageSection) {
				#package already ignored, rewrite section
				#look for next section start
				do
				{
					$lineNr++
					$line = $lines[$lineNr]
				}
				until($lines[$lineNr].StartsWith("# "))
				
				$writeIgnoreSection = $true
			} elseif($line -eq $NugetPackageContentIgnoreSectionEnd) {
				$writeIgnoreSection = $true
			}
			
			if($writeIgnoreSection) {
				$ignoreSectionWritten = $true;
				$ignoreSection = $(WriteIgnoreSection $id $version $packagePath $delimiter)
				if($ignoreSection -ne $null) {
					$fileContent += "$startPackageSection$delimiter"
					$fileContent += $ignoreSection
				}
			}
		}
		$fileContent += "$line$delimiter"
	}
	
	[IO.File]::WriteAllText($gitignore.FullName, $fileContent.Substring(0, $fileContent.Length - $delimiter.Length))
}

Function Get-InstallPath {
    param(
        $package
    )
    # Get the repository path
    $componentModel = Get-VSComponentModel
    $repositorySettings = $componentModel.GetService([NuGet.VisualStudio.IRepositorySettings])
    $pathResolver = New-Object "NuGet.DefaultPackagePathResolver, $($package.GetType().Assembly.FullName)" -ArgumentList $repositorySettings.RepositoryPath
	$pathResolver.GetInstallPath($package)
}

Function RewriteGitIgnore {
	$treatedPackages = @()
	Get-Project -All | ForEach-Object {
		$packages = Get-Package -ProjectName $_.Name
		foreach($package in $packages) {
			if(-not ($treatedPackages -contains $package)) {
				$treatedPackages += $package
				$installPath = Get-InstallPath $package
				$gitignoreFile = FindOrCreateGitIgnore $installPath
				$secStart = FindOrCreateNugetPackageContentIgnoreSection $gitignoreFile
				IgnoreUnignorePackage $package.Id $package.Version $installPath $gitignoreFile $secStart 'Ignore'
			}
		}
	}
}

# ------------------------------------------------------------------------------------------------------
#	Internal helpers
# ------------------------------------------------------------------------------------------------------

Function Find-GitRepositoryPath {
	param($path)
	
	if($path -eq $null)
	{
		return $null
	}
	
	if((Test-Path $path) -and (Test-Path $(Join-Path $path ".git"))) {
		#Found git repository
		return $path
	} else {
		return Find-GitRepositoryPath (Split-Path $path -Parent)
	}
}

Function Find-GitIgnore {
	param($path)
	
	return FindOrCreateGitIgnore $path $false
}

Function Handle-PackageReferenceRemoved {
	param($packageMetadata)
	
	Write-Host $packageMetadata.Id
	$gitignoreFile = FindOrCreateGitIgnore $packageMetadata.InstallPath
	$secStart = FindOrCreateNugetPackageContentIgnoreSection $gitignoreFile
	IgnoreUnignorePackage $packageMetadata.Id $packageMetadata.VersionString $packageMetadata.InstallPath $gitignoreFile $secStart 'Unignore'
}

Function Handle-PackageReferenceAdded {
	param($packageMetadata)
	
	Write-Host $packageMetadata.Id
	$gitignoreFile = FindOrCreateGitIgnore $packageMetadata.InstallPath
	$secStart = FindOrCreateNugetPackageContentIgnoreSection $gitignoreFile
	IgnoreUnignorePackage $packageMetadata.Id $packageMetadata.VersionString $packageMetadata.InstallPath $gitignoreFile $secStart 'Ignore'
}

Function InternalRestore-NugetContentFiles {
	param($logger, $path)
	
	$gitignore = $null
	
	if($path -eq $null) {
		$gitignore = Find-GitIgnore .
	} else {
		$gitignore = Find-GitIgnore $path
	}
	
	#Restoring content files is based on the .gitignore file, that way it can be run in any powershell console (no dependency on visualstudio)	
	
	
	$gitIgnorePath = Split-Path $gitignore -Parent
	
	$sectionStart = FindNugetPackageContentIgnoreSection $gitignore
	
	$origContent = [IO.File]::ReadAllText($gitignore.FullName)
	
	$delimiter = "`r`n"
	
	if(-not $origContent.Contains($delimiter))
	{
		$delimiter = "`n"
	}
	
	$lines = $origContent.Split(@($delimiter), [System.StringSplitOptions]::None)
	
	$copyFrom = $null
		
	for($lineNr = 0; $lineNr -lt $lines.Length; $lineNr++)
	{
		$line = $lines[$lineNr]
		if($lineNr -gt $sectionStart)
		{
			if($line -eq $NugetPackageContentIgnoreSectionEnd)
			{
			 	break;
			}
			
			if($line.StartsWith('# ')) {
				$logger.Invoke("Restoring content files for $($line.Replace('# ',''))")
			} else {
				if($line.StartsWith('#-source:')) {
					$copyFrom = $line.Replace('#-source:','');
				} elseif($copyFrom -ne $null) {
					$targetFile = Join-Path  $gitIgnorePath $line 
					
					if(-not (Test-Path $targetFile)) {
						$logger.invoke("`t $line")
						
						$targetDir = Split-Path $targetFile -Parent
						
						#create directories if needed
						if(-not (Test-Path $targetDir))
						{
							$directory = New-Item -ItemType Directory $targetDir
						}
						
						$newItem = Copy-Item $copyFrom -Destination $targetDir -Force
						$copyFrom = $null
					}
				}
			}
		}
	}
}

# ------------------------------------------------------------------------------------------------------
#	Public helpers
# ------------------------------------------------------------------------------------------------------

Function Restore-NugetContentFiles {
	param($path)
	InternalRestore-NugetContentFiles {
		param($message)
		Write-Host $message
	} $path
}
	
Function Initialize-GitIgnore {
	RewriteGitIgnore
	
	Write-Host "Delete content files to be able to remove them from git"
	
	Get-Project -All | ForEach-Object {
		Write-Host "`t Delete content from $($_.Name)"
		$projectDir = (gi -Path $_.FullName).Directory.FullName
		
		$gitignore = Find-GitIgnore $projectDir
		$uriGitIgnore = [System.Uri]($gitignore.FullName);
		
		$packages = Get-Package -ProjectName $_.Name		
		foreach($package in $packages) {
			$contentFiles = GetCompatibleContentFiles $package $_
			foreach($file in $contentFiles)	{
				$effectivePath = $file.EffectivePath
				
				$filePath = (Join-Path $projectDir $effectivePath)
				$uriFullPath = [System.Uri]$filePath;
				$relativePath = $uriGitIgnore.MakeRelativeUri($uriFullPath).ToString()
				
				if(Test-Path $filePath)	{
					Write-Host "`t`t$relativePath"
					Remove-Item -Path $filePath
				}				
			}
		}
	}
	
	Write-Host
	Write-Host "Commit changes to git and then rebuild solution (or run Restore-NugetContentFiles) to restore nuget content files"
}

Export-ModuleMember -Function Restore-NugetContentFiles
Export-ModuleMember -Function Initialize-GitIgnore

# ------------------------------------------------------------------------------------------------------
#	Custom types
# ------------------------------------------------------------------------------------------------------

Add-Type -TypeDefinition @"
public class NugetContentIgnoreVSInterop
{ 
	private EnvDTE80.DTE2 dte2;
	private string buildWindowPaneGuid;
	private EnvDTE.BuildEvents buildEvents;
	
	public delegate void BuildEventHandler(System.Threading.ManualResetEventSlim resetEvent);

    public event BuildEventHandler OnBuild;
	
	public NugetContentIgnoreVSInterop(object dte, string buildWindowPaneGuid)
	{ 
		this.buildWindowPaneGuid = buildWindowPaneGuid;
		this.dte2 = (EnvDTE80.DTE2)dte;
		this.buildEvents = this.dte2.Events.BuildEvents; // this line is needed to prevent build events from beeing garbage collected
		this.buildEvents.OnBuildBegin += (scope,action) => { 
			var resetEvent = new System.Threading.ManualResetEventSlim();
			OnBuild(resetEvent);
			resetEvent.Wait(); // wait on asynchronous powershell to be finished
			resetEvent.Dispose();
		}; 
	} 
		
	EnvDTE.OutputWindowPane GetBuildOutputPane()
    {
        foreach (EnvDTE.OutputWindowPane pane in dte2.ToolWindows.OutputWindow.OutputWindowPanes)
        {
            if (System.String.Equals(pane.Guid, buildWindowPaneGuid, System.StringComparison.OrdinalIgnoreCase))
            {
                return pane;
            }
        }

        return null;
    }
	
	public void WriteOnBuildWindow(string message)
	{
		GetBuildOutputPane().OutputString(message);
	}
} 
"@ -ReferencedAssemblies "EnvDTE80", "EnvDTE"