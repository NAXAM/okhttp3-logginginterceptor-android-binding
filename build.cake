#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

// Cake Addins
#addin nuget:?package=Cake.FileHelpers&version=2.0.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var VERSION = "3.11.0";
var NUGET_SUFIX = "";

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionPath = "./okhttp3-logging-interceptor.sln";
var artifacts = new [] {
    new Artifact {
        AssemblyInfoPath = "./Naxam.SquareUp.OkHttp3.LoggingInterceptor/Properties/AssemblyInfo.cs",
        NuspecPath = "./logginginterceptor.nuspec",
        DownloadUrl = "http://central.maven.org/maven2/com/squareup/okhttp3/logging-interceptor/{0}/logging-interceptor-{0}.jar ",
        JarPath = "./Naxam.SquareUp.OkHttp3.LoggingInterceptor/Jars/logging-interceptor.jar"
    },
};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Downloads")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        var downloadUrl = string.Format(artifact.DownloadUrl, VERSION);
        var jarPath = string.Format(artifact.JarPath, VERSION);

        DownloadFile(downloadUrl, jarPath);
    }
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./packages");

    var nugetPackages = GetFiles("./*.nupkg");

    foreach (var package in nugetPackages)
    {
        DeleteFile(package);
    }
});

Task("Restore-NuGet-Packages")
    .Does(() =>
{
    NuGetRestore(solutionPath);
});

Task("Build")
    .Does(() =>
{
    MSBuild(solutionPath, settings => settings.SetConfiguration(configuration));
});

Task("UpdateVersion")
    .Does(() => 
{
    foreach(var artifact in artifacts) {
        ReplaceRegexInFiles(artifact.AssemblyInfoPath, "\\[assembly\\: AssemblyVersion([^\\]]+)\\]", string.Format("[assembly: AssemblyVersion(\"{0}\")]", VERSION));
    }
});

Task("Pack")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        NuGetPack(artifact.NuspecPath, new NuGetPackSettings {
            Version = VERSION+NUGET_SUFIX,
            Dependencies = new []{
                new NuSpecDependency {
                    Id = "Square.OkHttp3",
                    Version = "3.8.1"
                },
                new NuSpecDependency {
                    Id = "Square.OkIO",
                    Version = "1.13.0"
                }
            },

            ReleaseNotes = new [] {
                $"Naxam Retrofit2 Convert Gson v{VERSION}"
            }
        });
    }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Downloads")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("UpdateVersion")
    .IsDependentOn("Build")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

class Artifact {
    public string AssemblyInfoPath { get; set; }

    public string SolutionPath { get; set; }

    public string DownloadUrl  { get; set; }

    public string JarPath { get; set; }

    public string NuspecPath { get; set; }
}