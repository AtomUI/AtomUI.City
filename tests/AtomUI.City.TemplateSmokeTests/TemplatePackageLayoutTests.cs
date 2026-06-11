namespace AtomUI.City.TemplateSmokeTests;

public sealed class TemplatePackageLayoutTests
{
    [Fact]
    public void TemplateRootDirectoryExists()
    {
        var repositoryRoot = FindRepositoryRoot();
        var templateRoot = Path.Combine(repositoryRoot.FullName, "src", "AtomUI.City.Templates", "templates");

        Assert.True(Directory.Exists(templateRoot), $"Expected template root directory at {templateRoot}.");
    }

    [Theory]
    [InlineData("atomui-city-app")]
    [InlineData("atomui-city-plugin")]
    public void TemplateMetadataExists(string templateName)
    {
        var repositoryRoot = FindRepositoryRoot();
        var templateJson = Path.Combine(
            repositoryRoot.FullName,
            "src",
            "AtomUI.City.Templates",
            "templates",
            templateName,
            ".template.config",
            "template.json");

        Assert.True(File.Exists(templateJson), $"Expected template metadata at {templateJson}.");
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AtomUICity.slnx")))
            {
                return directory;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test output directory.");
    }
}
