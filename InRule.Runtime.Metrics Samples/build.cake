//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Local");
var nugetSourceFeedUrl = Argument("nugetSourceFeedUrl", EnvironmentVariable("NuGet_Source_Feed_Url") ?? "");
var versionPrefix = Argument("versionPrefix", EnvironmentVariable("Version_Prefix") ?? "1.0.0");
var versionSuffix = Argument("versionSuffix", EnvironmentVariable("Version_Suffix") ?? "0");
var nugetPushFeedUrl = Argument("nugetPushFeedUrl", EnvironmentVariable("NuGet_Push_Feed_Url") ?? "");
var nugetPushApiKey = Argument("nugetPushApiKey", EnvironmentVariable("NuGet_Push_Api_Key") ?? "");

var isPrereleasePackageData = Argument("isPrereleasePackage", EnvironmentVariable("Is_Prerelease_Package") ?? "false");
bool isPrereleasePackage = Boolean.Parse(isPrereleasePackageData);

//////////////////////////////////////////////////////////////////////
// GLOBALS
//////////////////////////////////////////////////////////////////////

const string solution = "./InRule.Runtime.Metrics.sln";
const string nuGetOrgUrl = "https://api.nuget.org/v3/index.json";
const string nugetPackagesFolder = "./NuGetPackages";
const string releaseConfiguration = "Release";
const string versionPrefixProperty = "VersionPrefix";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
  .Does(() =>
{
  try
  {
    DotNetClean(solution);
  }
  catch { }

  try
  {
    CleanDirectory(nugetPackagesFolder);
  }
  catch { }
});

Task("Restore .NET Dependencies")
  .Does(() =>
{
  ICollection<string> sources;

  if (!string.IsNullOrWhiteSpace(nugetSourceFeedUrl))
  {
    var nugetSourceUrlArray = nugetSourceFeedUrl.Split(';');

    sources = new List<string>();

    foreach (var nuGetSource in nugetSourceUrlArray)
    {
        Warning("{0} added as an additional NuGet feed.", nuGetSource);
        sources.Add(nuGetSource);
    }

    sources.Add(nuGetOrgUrl);
  }
  else
  {
    Warning("No additional NuGet feed specified.");
    sources = new[] { nuGetOrgUrl };
  }

  DotNetRestore(solution, new DotNetRestoreSettings { Sources = sources });
});

Task("Build and Publish Metrics Adapter Libraries")
  .Does(() =>
{
  var settings = new DotNetPublishSettings
  {
    Configuration = releaseConfiguration,
    VersionSuffix = versionSuffix,
    MSBuildSettings = new DotNetMSBuildSettings().WithProperty(versionPrefixProperty, versionPrefix),
  };

  Warning("Publishing for netstandard2.0");
  settings.Framework = "netstandard2.0";
  FilePathCollection netStandardProjectFiles = GetFiles("./**/*.csproj");
  netStandardProjectFiles.Remove(GetFiles("./**/*Tests.csproj"));
  netStandardProjectFiles.Remove(GetFiles("./Samples/**/*.csproj"));
  foreach (FilePath file in netStandardProjectFiles)
  {
    DotNetPublish(file.ToString(), settings);
  }
});

Task("Test SQL Adapter")
  .Does(() =>
{
  var settings = new DotNetTestSettings
  {
    Loggers = new List<string>
    {
      "console;verbosity=normal",
    }
  };

  DotNetTest("./InRule.Runtime.Metrics.SqlServer.IntegrationTests/InRule.Runtime.Metrics.SqlServer.IntegrationTests.csproj", settings);
});

Task("Create Metrics Adapter NuGet Packages")
  .Does(() =>
{
  var settings = new DotNetPackSettings
  {
    NoBuild = true,
    Configuration = releaseConfiguration,
    OutputDirectory = nugetPackagesFolder,
  };

  if(isPrereleasePackage)
  {
    settings.VersionSuffix = versionSuffix;
    settings.MSBuildSettings = new DotNetMSBuildSettings().WithProperty(versionPrefixProperty, versionPrefix);
  }
  else
  {
    settings.MSBuildSettings = new DotNetMSBuildSettings().WithProperty("Version", versionPrefix + "." + versionSuffix);
  }

  DotNetPack(solution, settings);
});

Task("Publish to NuGet Feed")
  .Does(() =>
{
  if (!HasArgument("nugetPushFeedUrl"))
  {
    Error("nugetPushFeedUrl argument is required.");
  }

  if (!HasArgument("nugetPushApiKey"))
  {
    Error("nugetPushApiKey argument is required.");
  }

  var settings = new DotNetNuGetPushSettings
  {
    Source = nugetPushFeedUrl,
    ApiKey = nugetPushApiKey,
    WorkingDirectory = nugetPackagesFolder,
  };

  DotNetNuGetPush("*.nupkg", settings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Local")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore .NET Dependencies")
    .IsDependentOn("Build and Publish Metrics Adapter Libraries")
    .IsDependentOn("Test SQL Adapter")
    .IsDependentOn("Create Metrics Adapter NuGet Packages");

Task("Publish")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore .NET Dependencies")
    .IsDependentOn("Build and Publish Metrics Adapter Libraries")
    .IsDependentOn("Test SQL Adapter")
    .IsDependentOn("Create Metrics Adapter NuGet Packages")
    .IsDependentOn("Publish to NuGet Feed");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);