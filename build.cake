#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configs = new string[] { "NET40", "NET35" };
string version;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
  .Does(() =>
{
  var buildDir = Directory("./publish/HttpClientPolyfill/lib");
  CleanDirectory(buildDir);
});

Task("Patch-Version")
  .IsDependentOn("Clean")
  .Does(() =>
{
  var file = "./HttpClientPolyfill/Properties/AssemblyInfo.cs";
  var info = ParseAssemblyInfo(file);
  var parts = info.AssemblyVersion.Split('.');
  var lastPart = int.Parse(parts[parts.Length - 1]) + 1;
  parts[parts.Length - 1] = lastPart.ToString();
  version = string.Join(".", parts);
  CreateAssemblyInfo(file, new AssemblyInfoSettings {
    Product = "HttpClientPolyfill",
    Version = version,
    FileVersion = version,
    Description = "A complete port of Mono's System.Net.Http assembly to .NET 3.5"
  });
});

Task("Restore-NuGet-Packages")
  .IsDependentOn("Patch-Version")
  .Does(() =>
{
  NuGetRestore("./HttpClientPolyfill.sln");
});

Task("Build")
  .IsDependentOn("Restore-NuGet-Packages")
  .Does(() =>
{
  foreach (var config in configs)
  {
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./HttpClientPolyfill.sln", settings =>
        settings.SetConfiguration(config));
    }
    else
    {
      // Use XBuild
      XBuild("./HttpClientPolyfill.sln", settings =>
        settings.SetConfiguration(config));
    }
  }
});

Task("NuGet-Pack")
  .IsDependentOn("Build")
  .Does(() =>
{
  DeleteFiles("./HttpClientPolyfill.*.nupkg");
  DeleteFiles("./publish/**/System.Threading.*");
  var nuGetPackSettings = new NuGetPackSettings {
    Version = version
  };
  NuGetPack("./publish/HttpClientPolyfill/HttpClientPolyfill.nuspec", nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("NuGet-Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
