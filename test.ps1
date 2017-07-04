$ToolsDirectory = ".\tools"
$OpenCoverPackage = "OpenCover"
$OpenCoverVersion = "4.6.519"
$CoverallsNetPackage = "coveralls.net"
$CoverallsNetVersion = "0.7.0"

nuget install OpenCover -Version $OpenCoverVersion -OutputDirectory $ToolsDirectory
nuget install coveralls.net -Version $CoverallsNetVersion -OutputDirectory $ToolsDirectory

$ToolsDirectory\$OpenCoverPackage.$OpenCoverVersion\tools\OpenCover.Console.exe -target:nunit3-console.exe -targetargs:"--result=myresults.xml;format=AppVeyor ZeroMQTest\bin\Debug\ZeroMQTest.dll" -filter:"+[*]* -[*.Tests]*" -register:user

if ($Env:COVERALLS_REPO_TOKEN == "") {
  echo Skipping coverage result upload because COVERALLS_REPO_TOKEN is not set
} else {
  $ToolsDirectory\$CoverallsNetPackage.$CoverallsNetVersion\tools\csmacnz.Coveralls.exe --opencover -i .\results.xml
}
