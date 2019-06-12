#addin "Cake.Incubator&version=5.0.1"

//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Local");
var nugetSourceFeedUrl = Argument("nugetSourceFeedUrl", EnvironmentVariable("NuGet_Source_Feed_Url") ?? "");

//////////////////////////////////////////////////////////////////////
// GLOBALS
//////////////////////////////////////////////////////////////////////

FilePathCollection _solutionFilesThatNeedRunTimeTesting = new FilePathCollection();
_solutionFilesThatNeedRunTimeTesting.Add(GetFiles("./Developer Samples/Wpf.ObjectAsStateInvoice/Wpf.ObjectAsStateInvoice.sln"));

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean, Restore, and Build solutions")
  .Does(() =>
{
  FilePathCollection solutionFiles = GetFiles("./**/*.sln");
  solutionFiles.Remove(_solutionFilesThatNeedRunTimeTesting);

  foreach(var solutionFile in solutionFiles)
  {
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    Information("Building " + solutionFile);
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    MSBuild(solutionFile, settings => settings.WithTarget("Clean"));
    NuGetRestore(solutionFile);
    MSBuild(solutionFile, settings => settings.WithTarget("Rebuild"));
  }
});

Task("Clean, Restore, and Build solutions that depend on InRule.RunTime.Testing")
  .Does(() =>
{
  foreach(var solutionFile in _solutionFilesThatNeedRunTimeTesting)
  {
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    Information("Building " + solutionFile);
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    MSBuild(solutionFile, settings => settings.WithTarget("Clean"));
    NuGetRestore(solutionFile);
    MSBuild(solutionFile, settings => settings.WithTarget("Rebuild"));
  }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Local")
  .IsDependentOn("Clean, Restore, and Build Solutions");

Task("Remote")
  .IsDependentOn("Clean, Restore, and Build Solutions")
  .IsDependentOn("Clean, Restore, and Build Solutions that depend on InRule.RunTime.Testing");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);