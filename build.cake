#addin "Cake.Incubator&version=5.0.1"

//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Local");
var nugetSourceFeedUrl = Argument("nugetSourceFeedUrl", EnvironmentVariable("NuGet_Source_Feed_Url") ?? "");

//////////////////////////////////////////////////////////////////////
// GLOBALS
//////////////////////////////////////////////////////////////////////

const string _nuGetOrgUrl = "https://api.nuget.org/v3/index.json";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean, Restore, and Build solutions")
  .Does(() =>
{
  // Determine solution files.
  FilePathCollection solutionFiles = GetFiles("./**/*.sln");

  // Determine NuGet source feeds.
  ICollection<string> nuGetSources;
  if (!string.IsNullOrWhiteSpace(nugetSourceFeedUrl))
  {
    Warning("{0} added as an additional NuGet feed.", nugetSourceFeedUrl);
    nuGetSources = new[] { nugetSourceFeedUrl, _nuGetOrgUrl };
  }
  else
  {
    Warning("No additional NuGet feed specified.");
    nuGetSources = new[] { _nuGetOrgUrl };
  }

  // Clean, restore, and build.
  foreach(var solutionFile in solutionFiles)
  {
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    Information("Building " + solutionFile);
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    MSBuild(solutionFile, settings => settings.WithTarget("Clean"));
    NuGetRestore(solutionFile, new NuGetRestoreSettings { Source = nuGetSources });
    MSBuild(solutionFile, settings => settings.WithTarget("Rebuild"));
  }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Local")
  .IsDependentOn("Clean, Restore, and Build Solutions");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);