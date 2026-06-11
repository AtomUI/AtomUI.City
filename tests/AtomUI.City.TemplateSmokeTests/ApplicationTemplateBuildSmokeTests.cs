using AtomUI.City.Templates;

namespace AtomUI.City.TemplateSmokeTests;

public sealed class ApplicationTemplateBuildSmokeTests
{
    [Fact]
    public void ApplicationTemplateGeneratesBuildAndTestProjectFiles()
    {
        using var workspace = new TemplateSmokeWorkspace();
        var renderer = new ApplicationTemplateRenderer();

        renderer.Render(new ApplicationTemplateOptions
        {
            AppName = "SalesClient",
            RootNamespace = "Company.SalesClient",
            OutputPath = workspace.Root,
            TargetFramework = "net10.0",
            IncludeTests = true,
        });

        var appProjectPath = Path.Combine(workspace.Root, "src", "SalesClient", "SalesClient.csproj");
        var testProjectPath = Path.Combine(workspace.Root, "tests", "SalesClient.Tests", "SalesClient.Tests.csproj");
        var programPath = Path.Combine(workspace.Root, "src", "SalesClient", "Program.cs");

        Assert.True(File.Exists(appProjectPath), $"Expected application project at {appProjectPath}.");
        Assert.True(File.Exists(testProjectPath), $"Expected test project at {testProjectPath}.");

        var appProject = File.ReadAllText(appProjectPath);
        Assert.Contains("<ImplicitUsings>enable</ImplicitUsings>", appProject, StringComparison.Ordinal);
        Assert.Contains("<Nullable>enable</Nullable>", appProject, StringComparison.Ordinal);
        Assert.Contains("""<PackageReference Include="AtomUI.City.Core" Version="0.1.0" />""", appProject, StringComparison.Ordinal);
        Assert.Contains("""<PackageReference Include="AtomUI.City.Build" Version="0.1.0" PrivateAssets="all" />""", appProject, StringComparison.Ordinal);

        var testProject = File.ReadAllText(testProjectPath);
        Assert.Contains("""<PackageReference Include="Microsoft.NET.Test.Sdk" Version=""", testProject, StringComparison.Ordinal);
        Assert.Contains("""<PackageReference Include="xunit" Version=""", testProject, StringComparison.Ordinal);
        Assert.Contains("""<ProjectReference Include="../../src/SalesClient/SalesClient.csproj" />""", testProject, StringComparison.Ordinal);

        var program = File.ReadAllText(programPath);
        Assert.Contains("using AtomUI.City.Hosting;", program, StringComparison.Ordinal);
        Assert.Contains("ApplicationHost.CreateBuilder(args)", program, StringComparison.Ordinal);
    }

    private sealed class TemplateSmokeWorkspace : IDisposable
    {
        public TemplateSmokeWorkspace()
        {
            Root = Path.Combine(Path.GetTempPath(), "AtomUICityTemplateSmokeTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
