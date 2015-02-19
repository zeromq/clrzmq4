$PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition

$modulePath = Join-Path $PSScriptRoot Baseclass.Contrib.Nuget.GitIgnoreContent.psm1
$module = Import-Module $modulePath -PassThru

$componentModel = Get-VSComponentModel
$packageInstallerEvents = $componentModel.GetService([NuGet.VisualStudio.IVsPackageInstallerEvents])

#Register event when a package reference is added to a project
$packageReferenceAddedEventRegistration = Register-ObjectEvent $packageInstallerEvents PackageReferenceAdded -MessageData $module -Action {
	$packageMetadata = $event.SourceArgs[0]	
	$module = $event.MessageData
	
	& $module {
		param($packageMetadata)
		Handle-PackageReferenceAdded $packageMetadata
	} $packageMetadata
}

#Register event when a package reference is removed from a project
$packageReferenceRemovedEventRegistration = Register-ObjectEvent $packageInstallerEvents PackageReferenceRemoved -MessageData $module -Action {
	$packageMetadata = $event.SourceArgs[0]	
	$module = $event.MessageData
	
	& $module {
		param($packageMetadata)
		Handle-PackageReferenceRemoved $packageMetadata
	} $packageMetadata
}

$vsInterop = New-Object NugetContentIgnoreVSInterop -ArgumentList $DTE, $([Microsoft.VisualStudio.VSConstants]::BuildOutput.ToString("B"))

$data = @($module, $vsInterop)

$buildEvent = Register-ObjectEvent $vsInterop OnBuild -MessageData $data -Action { 
	Try {
		$module = $event.MessageData[0]
		$interop = $event.MessageData[1]	
		
		& $module {
			param($interop)
			InternalRestore-NugetContentFiles {
				param($message)
				$interop.WriteOnBuildWindow("NuGetContentRestore> $message`r`n")
			}
		} $interop
	} Finally {
		$event.SourceArgs[0].Set();
	}
}

