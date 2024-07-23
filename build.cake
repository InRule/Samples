// Usage: dotnet cake build.cake --target=Local --nugetSourceFeedUrl=https://www.myget.org/F/inrule/api/v3/index.json --inruleVersion=8.0.0

#tool nuget:?package=NuGet.CommandLine&version=6.10.1

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
MSBuildToolVersion _msbuildVersion = MSBuildToolVersion.VS2022;

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
  Information($"Cleaning all solutions. {_solutionFiles.Count} found.");
  foreach(var solutionFile in _solutionFiles)
  {
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
    Information("Cleaning " + solutionFile);
    Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");

    try
    {
      MSBuild(solutionFile, settings => settings.WithTarget("Clean").UseToolVersion(_msbuildVersion));
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
  NuGetUpdateSettings nuGetUpdateSettings;

  if (!string.IsNullOrWhiteSpace(inruleVersion))
  {
    Warning("Updating to version {0} of InRule.", inruleVersion);

    nuGetUpdateSettings = new NuGetUpdateSettings
    {
      Prerelease = true,
      Version = inruleVersion,
      Source = _nuGetSources,
    };

    foreach(var solutionFile in _solutionFiles)
    {
      Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");
      Information("Updating " + solutionFile);
      Information("-+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+- -+-");

      nuGetUpdateSettings.Id = new [] { "InRule.Authoring.SDK" };
      NuGetUpdate(solutionFile, nuGetUpdateSettings);

      nuGetUpdateSettings.Id = new [] { "InRule.Runtime" };
      NuGetUpdate(solutionFile, nuGetUpdateSettings);

      nuGetUpdateSettings.Id = new [] { "InRule.Repository" };
      NuGetUpdate(solutionFile, nuGetUpdateSettings);

      nuGetUpdateSettings.Id = new [] { "InRule.Common" };
      NuGetUpdate(solutionFile, nuGetUpdateSettings);
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
    MSBuild(solutionFile, settings => settings.WithTarget("Build").UseToolVersion(_msbuildVersion));
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