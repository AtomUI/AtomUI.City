namespace AtomUI.City.Build.Tests;

internal static class RepositoryPaths
{
    public static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var solutionPath = Path.Combine(directory.FullName, "AtomUICity.slnx");

            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate AtomUICity.slnx from the test output directory.");
    }

    public static string ToRepositoryRelativePath(string repositoryRoot, string path)
    {
        return Path
            .GetRelativePath(repositoryRoot, path)
            .Replace('\\', '/');
    }
}
