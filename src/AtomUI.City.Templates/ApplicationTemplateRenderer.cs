namespace AtomUI.City.Templates;

public sealed class ApplicationTemplateRenderer
{
    public TemplatePlan CreatePlan(ApplicationTemplateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new TemplatePlan(
            operationId: $"new-app-{options.AppName}",
            command: "atomui city new app",
            inputs: new Dictionary<string, object?>
            {
                ["appName"] = options.AppName,
                ["rootNamespace"] = options.RootNamespace,
                ["targetFramework"] = options.TargetFramework,
                ["includeTests"] = options.IncludeTests,
                ["useAot"] = options.UseAot,
                ["useDynamicPlugins"] = options.UseDynamicPlugins,
                ["includeSample"] = options.IncludeSample,
            },
            changes: GetChanges(options).ToArray());
    }

    public TemplateRenderResult Render(ApplicationTemplateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var plan = CreatePlan(options);

        WriteFile(options, $"src/{options.AppName}/{options.AppName}.csproj", CreateApplicationProject(options));
        WriteFile(options, $"src/{options.AppName}/Program.cs", CreateProgram(options));
        WriteFile(options, $"src/{options.AppName}/App.axaml", CreateAppXaml(options));
        WriteFile(options, $"src/{options.AppName}/App.axaml.cs", CreateAppCodeBehind(options));
        WriteFile(options, $"src/{options.AppName}/Modules/.gitkeep", string.Empty);
        WriteFile(options, $"src/{options.AppName}/Routes/.gitkeep", string.Empty);
        WriteFile(options, $"src/{options.AppName}/Resources/.gitkeep", string.Empty);
        WriteFile(options, $"src/{options.AppName}/Configuration/.gitkeep", string.Empty);
        WriteFile(options, $"src/{options.AppName}/Localization/.gitkeep", string.Empty);

        if (options.IncludeTests)
        {
            WriteFile(options, $"tests/{options.AppName}.Tests/FeatureTestMatrix.md", CreateFeatureTestMatrix(options));
            WriteFile(options, $"tests/{options.AppName}.Tests/ApplicationSmokeTests.cs", CreateApplicationSmokeTests(options));
        }

        return TemplateRenderResult.Success(plan);
    }

    private static IEnumerable<TemplateChange> GetChanges(ApplicationTemplateOptions options)
    {
        yield return TemplateChange.Create($"src/{options.AppName}/{options.AppName}.csproj");
        yield return TemplateChange.Create($"src/{options.AppName}/Program.cs");
        yield return TemplateChange.Create($"src/{options.AppName}/App.axaml");
        yield return TemplateChange.Create($"src/{options.AppName}/App.axaml.cs");
        yield return TemplateChange.Create($"src/{options.AppName}/Modules/.gitkeep");
        yield return TemplateChange.Create($"src/{options.AppName}/Routes/.gitkeep");
        yield return TemplateChange.Create($"src/{options.AppName}/Resources/.gitkeep");
        yield return TemplateChange.Create($"src/{options.AppName}/Configuration/.gitkeep");
        yield return TemplateChange.Create($"src/{options.AppName}/Localization/.gitkeep");

        if (options.IncludeTests)
        {
            yield return TemplateChange.Create($"tests/{options.AppName}.Tests/FeatureTestMatrix.md");
            yield return TemplateChange.Create($"tests/{options.AppName}.Tests/ApplicationSmokeTests.cs");
        }
    }

    private static void WriteFile(
        ApplicationTemplateOptions options,
        string relativePath,
        string content)
    {
        var path = Path.Combine([options.OutputPath, .. relativePath.Split('/')]);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static string CreateApplicationProject(ApplicationTemplateOptions options)
    {
        var dynamicPlugins = options.UseDynamicPlugins
            ? """
                <PackageReference Include="AtomUI.City.PluginSystem" />
            """
            : string.Empty;

        return $$"""
            <Project Sdk="Microsoft.NET.Sdk">

              <PropertyGroup>
                <OutputType>WinExe</OutputType>
                <TargetFramework>{{options.TargetFramework}}</TargetFramework>
                <RootNamespace>{{options.RootNamespace}}</RootNamespace>
                <AtomUICityManifestGeneration>true</AtomUICityManifestGeneration>
                <AtomUICityAotFriendly>{{options.UseAot.ToString().ToLowerInvariant()}}</AtomUICityAotFriendly>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="AtomUI.City.Build" PrivateAssets="all" />
                <PackageReference Include="AtomUI.City.Core" />
                <PackageReference Include="AtomUI.City.Mvvm" />
                <PackageReference Include="AtomUI.City.Routing" />
                <PackageReference Include="AtomUI.City.Presentation" />
                <PackageReference Include="AtomUI.City.Localization" />
            {{dynamicPlugins}}
              </ItemGroup>

            </Project>
            """;
    }

    private static string CreateProgram(ApplicationTemplateOptions options)
    {
        return $$"""
            using AtomUI.City.Hosting;

            namespace {{options.RootNamespace}};

            internal static class Program
            {
                public static async Task<int> Main(string[] args)
                {
                    var host = CityApplication.CreateBuilder(args)
                        .Build();

                    await host.RunAsync();

                    return 0;
                }
            }
            """;
    }

    private static string CreateAppXaml(ApplicationTemplateOptions options)
    {
        return $$"""
            <Application
                x:Class="{{options.RootNamespace}}.App"
                xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
            </Application>
            """;
    }

    private static string CreateAppCodeBehind(ApplicationTemplateOptions options)
    {
        return $$"""
            namespace {{options.RootNamespace}};

            public sealed partial class App
            {
                public void Initialize()
                {
                }
            }
            """;
    }

    private static string CreateFeatureTestMatrix(ApplicationTemplateOptions options)
    {
        return $$"""
            # {{options.AppName}} Feature Test Matrix

            | Feature | Unit Tests | Integration Tests | Notes |
            |---|---|---|---|
            | Application startup | ApplicationSmokeTests | Pending | Generated by AtomUI.City template. |
            """;
    }

    private static string CreateApplicationSmokeTests(ApplicationTemplateOptions options)
    {
        return $$"""
            namespace {{options.RootNamespace}}.Tests;

            public sealed class ApplicationSmokeTests
            {
                [Fact]
                public void ApplicationTemplateContainsSmokeTest()
                {
                    Assert.True(true);
                }
            }
            """;
    }
}
