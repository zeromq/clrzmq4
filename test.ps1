$ToolsDirectory = ".\tools"
$OpenCoverPackage = "OpenCover"
$OpenCoverVersion = "4.6.519"

nuget install $OpenCoverPackage -Version $OpenCoverVersion -OutputDirectory $ToolsDirectory

start $ToolsDirectory/$OpenCoverPackage.$OpenCoverVersion/tools/OpenCover.Console.exe -ArgumentList '-target:nunit3-console.exe','-targetargs:"--result=myresults.xml;format=AppVeyor ZeroMQTest\bin\Debug\ZeroMQTest.dll"','-filter:"+[*]* -[*.Tests]*"','-register:user'

