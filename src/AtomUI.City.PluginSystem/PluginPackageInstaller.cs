using System.IO.Compression;
using System.Text.Json;

namespace AtomUI.City.PluginSystem;

public sealed class PluginPackageInstaller
{
    public async ValueTask<PluginInstallResult> InstallFromDirectoryAsync(
        string packageRoot,
        string pluginsRoot,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginsRoot);

        var stagingRoot = Path.Combine(
            pluginsRoot,
            PluginPackagePaths.StagingDirectoryName,
            Guid.NewGuid().ToString("N"));
        var extractRoot = Path.Combine(stagingRoot, "extract");

        try
        {
            CopyDirectory(packageRoot, extractRoot);

            return await InstallFromExtractedRootAsync(
                    extractRoot,
                    stagingRoot,
                    pluginsRoot,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            DeleteStagingRoot(stagingRoot);
            throw;
        }
    }

    public async ValueTask<PluginInstallResult> InstallFromPackageAsync(
        string packagePath,
        string pluginsRoot,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginsRoot);

        var stagingRoot = Path.Combine(
            pluginsRoot,
            PluginPackagePaths.StagingDirectoryName,
            Guid.NewGuid().ToString("N"));
        var extractRoot = Path.Combine(stagingRoot, "extract");
        Directory.CreateDirectory(extractRoot);

        try
        {
            ZipFile.ExtractToDirectory(packagePath, extractRoot);
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            DeleteStagingRoot(stagingRoot);

            return PluginInstallResult.Failed(
                [
                    new PluginDiagnostic(
                        PluginDiagnosticIds.PackageExtractionFailed,
                        $"Plugin package '{packagePath}' extraction failed: {exception.Message}",
                        Path: packagePath),
                ]);
        }

        try
        {
            return await InstallFromExtractedRootAsync(
                    extractRoot,
                    stagingRoot,
                    pluginsRoot,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            DeleteStagingRoot(stagingRoot);
            throw;
        }
    }

    private static async ValueTask<PluginInstallResult> InstallFromExtractedRootAsync(
        string extractRoot,
        string stagingRoot,
        string pluginsRoot,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validation = PluginPackageLayoutValidator.Validate(extractRoot);
        if (!validation.Succeeded)
        {
            DeleteStagingRoot(stagingRoot);
            return PluginInstallResult.Failed(validation.Diagnostics);
        }

        var manifest = PluginManifestReader.Read(PluginPackagePaths.GetManifestPath(extractRoot));
        var installedVersionPath = PluginPackagePaths.GetInstalledVersionPath(
            pluginsRoot,
            manifest.PluginId,
            manifest.Version);
        var installedRootPath = Path.Combine(installedVersionPath, PluginPackagePaths.RuntimeRootDirectoryName);

        if (Directory.Exists(installedVersionPath))
        {
            DeleteStagingRoot(stagingRoot);
            return PluginInstallResult.Failed(
                [
                    new PluginDiagnostic(
                        PluginDiagnosticIds.PluginAlreadyInstalled,
                        $"Plugin '{manifest.PluginId}' version '{manifest.Version}' is already installed.",
                        manifest.PluginId,
                        Path: installedVersionPath),
                ]);
        }

        Directory.CreateDirectory(installedVersionPath);
        Directory.Move(extractRoot, installedRootPath);

        var installation = new PluginInstallation(
            manifest.PluginId,
            manifest.PackageId,
            manifest.Version,
            installedRootPath,
            PluginPackagePaths.GetManifestPath(installedRootPath));

        var installRecordPath = PluginPackagePaths.GetInstallRecordPath(
            pluginsRoot,
            manifest.PluginId,
            manifest.Version);
        var installRecordJson = JsonSerializer.Serialize(
            installation,
            new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(installRecordPath, installRecordJson, cancellationToken)
            .ConfigureAwait(false);

        DeleteStagingRoot(stagingRoot);

        return PluginInstallResult.Success(installation);
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(targetDirectory, Path.GetRelativePath(sourceDirectory, directory)));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var targetFile = Path.Combine(targetDirectory, Path.GetRelativePath(sourceDirectory, file));
            Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
            File.Copy(file, targetFile, overwrite: true);
        }
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static void DeleteStagingRoot(string stagingRoot)
    {
        DeleteDirectoryIfExists(stagingRoot);

        var stagingDirectory = Path.GetDirectoryName(stagingRoot);
        if (string.IsNullOrWhiteSpace(stagingDirectory) ||
            !Directory.Exists(stagingDirectory) ||
            Directory.EnumerateFileSystemEntries(stagingDirectory).Any())
        {
            return;
        }

        Directory.Delete(stagingDirectory);
    }
}
