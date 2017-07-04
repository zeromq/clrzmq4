if ([string]::IsNullOrEmpty($Env:COVERALLS_REPO_TOKEN)) {
  echo "Skipping coverage result upload because COVERALLS_REPO_TOKEN is not set"
} else {
  $ToolsDirectory = ".\tools"
  $CoverallsNetPackage = "coveralls.net"
  $CoverallsNetVersion = "0.7.0"
  nuget install $CoverallsNetPackage -Version $CoverallsNetVersion -OutputDirectory $ToolsDirectory

  Start-Process -Wait -NoNewWindow $ToolsDirectory/$CoverallsNetPackage.$CoverallsNetVersion/tools/csmacnz.Coveralls.exe '--opencover','-i','.\results.xml'
}
