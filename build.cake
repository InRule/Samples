#addin "Cake.Incubator&version=5.0.1"

//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Local");
var nugetSourceFeedUrl = Argument("nugetSourceFeedUrl", EnvironmentVariable("NuGet_Source_Feed_Url") ?? "");
var inruleVersion = Argument("inruleVersion", EnvironmentVariable("InRule_Version") ?? "");

//////////////////////////////////////////////////////////////////////
// GLOBALS
//////////////////////////////////////////////////////////////////////

FilePathCollection _solutionFiles = GetFiles("./**/*.sln");

// Determine NuGet source feeds.
ICollection<string> _nuGetSources;
const string _nuGetOrgUrl = "https://api.nuget.org/v3/index.json";
if (!string.IsNullOrWhiteSpace(nugetSourceFeedUrl))
{
  Warning("{0} added as an additional NuGet feed.", nugetSourceFeedUrl);
  _nuGetSources = new[] { nugetSourceFeedUrl, _nuGetOrgUrl };
}
else
{
  Warning("No additional NuGet feed specified.");
  _nuGetSources = new[] { _nuGetOrgUrl };
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
  .Does(() =>
{
  foreach(var solutionFile in _solutionFiles)
  {
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    Information("Cleaning " + solutionFile);
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");

    try
    {
      MSBuild(solutionFile, settings => settings.WithTarget("Clean"));
    }
    catch { }
  }
});

Task("Restore")
  .Does(() =>
{
  foreach(var solutionFile in _solutionFiles)
  {
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    Information("Restoring " + solutionFile);
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    NuGetRestore(solutionFile, new NuGetRestoreSettings { Source = _nuGetSources });
  }
});

Task("Update")
  .Does(() =>
{
  if (!string.IsNullOrWhiteSpace(inruleVersion))
  {
    foreach(var solutionFile in _solutionFiles)
    {
      Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
      Information("Updating " + solutionFile);
      Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");

      NuGetUpdate(solutionFile, new NuGetUpdateSettings {
        Id = new [] { "InRule.Common", "InRule.Runtime", "InRule.Repository", "InRule.Authoring.SDK" },
        Prerelease = true,
        Version = inruleVersion,
        Source = _nuGetSources,
      });
    }
  }
  else
    Information("No update requested.");
});

Task("Build")
  .Does(() =>
{
  foreach(var solutionFile in _solutionFiles)
  {
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    Information("Building " + solutionFile);
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    MSBuild(solutionFile, settings => settings.WithTarget("Build"));
  }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Local")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore")
  .IsDependentOn("Update")
  .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);