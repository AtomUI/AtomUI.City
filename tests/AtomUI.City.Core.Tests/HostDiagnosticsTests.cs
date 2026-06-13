using AtomUI.City.Diagnostics;
using AtomUI.City.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Core.Tests;

public sealed class HostDiagnosticsTests
{
    [Fact]
    public async Task ApplicationHostRegistersDiagnosticsAndRecordsHostLifecycleEvents()
    {
        await using var host = ApplicationHost.CreateBuilder().Build();
        var diagnostics = host.Services.GetRequiredService<IHostDiagnostics>();

        Assert.Contains(diagnostics.Records, record => record.Code == HostDiagnosticIds.HostBuilt);

        await host.StartAsync();
        await host.StopAsync();

        Assert.Contains(diagnostics.Records, record => record.Code == HostDiagnosticIds.HostStarted);
        Assert.Contains(diagnostics.Records, record => record.Code == HostDiagnosticIds.HostStopped);
    }

    [Fact]
    public void InMemoryDiagnosticsStoresRecordsInWriteOrder()
    {
        var diagnostics = new InMemoryHostDiagnostics();

        diagnostics.Write(new HostDiagnosticRecord("TEST001", "First", HostDiagnosticSeverity.Info));
        diagnostics.Write(new HostDiagnosticRecord("TEST002", "Second", HostDiagnosticSeverity.Warning));

        Assert.Equal(["TEST001", "TEST002"], diagnostics.Records.Select(record => record.Code));
    }

    [Fact]
    public void InMemoryDiagnosticsRecordsSnapshotRejectsExternalListMutation()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        diagnostics.Write(new HostDiagnosticRecord("TEST001", "First", HostDiagnosticSeverity.Info));
        var records = Assert.IsAssignableFrom<IList<HostDiagnosticRecord>>(diagnostics.Records);

        Assert.Throws<NotSupportedException>(() => records[0] = new HostDiagnosticRecord(
            "TEST999",
            "Changed",
            HostDiagnosticSeverity.Error));
        Assert.Equal("TEST001", diagnostics.Records[0].Code);
    }
}
