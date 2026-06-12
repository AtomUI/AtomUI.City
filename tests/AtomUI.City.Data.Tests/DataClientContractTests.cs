using AtomUI.City.Data;

namespace AtomUI.City.Data.Tests;

public sealed class DataClientContractTests
{
    [Fact]
    public void DataClientRegistryReturnsTypedClient()
    {
        var registry = new DataClientRegistry();
        var client = new InventoryClient();

        registry.Register<IInventoryClient>(client);

        Assert.Same(client, registry.GetRequiredClient<IInventoryClient>());
    }

    [Fact]
    public void DataClientRegistryWritesRegisteredClientDiagnostic()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var registry = new DataClientRegistry(diagnostics);
        var client = new InventoryClient();

        registry.Register<IInventoryClient>(client);

        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.ClientRegistered);
        Assert.Equal(DataDiagnosticSeverity.Info, record.Severity);
        Assert.Equal("inventory", record.ClientId);
        Assert.Contains(nameof(IInventoryClient), record.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DataClientRegistryRejectsUnknownClient()
    {
        var registry = new DataClientRegistry();

        var exception = Assert.Throws<KeyNotFoundException>(
            () => registry.GetRequiredClient<IInventoryClient>());

        Assert.Contains(nameof(IInventoryClient), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DataClientRegistryWritesMissingClientDiagnostic()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var registry = new DataClientRegistry(diagnostics);

        Assert.Throws<KeyNotFoundException>(
            () => registry.GetRequiredClient<IInventoryClient>());

        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.ClientMissing);
        Assert.Contains(nameof(IInventoryClient), record.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DataClientRegistryUnregistersTypedClient()
    {
        var registry = new DataClientRegistry();
        var client = new InventoryClient();
        registry.Register<IInventoryClient>(client);

        var removed = registry.Unregister<IInventoryClient>();

        Assert.True(removed);
        Assert.Throws<KeyNotFoundException>(
            () => registry.GetRequiredClient<IInventoryClient>());
    }

    [Fact]
    public void DataClientRegistryWritesUnregisteredClientDiagnostic()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var registry = new DataClientRegistry(diagnostics);
        var client = new InventoryClient();
        registry.Register<IInventoryClient>(client);

        registry.Unregister<IInventoryClient>();

        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.ClientUnregistered);
        Assert.Equal(DataDiagnosticSeverity.Info, record.Severity);
        Assert.Equal("inventory", record.ClientId);
        Assert.Contains(nameof(IInventoryClient), record.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DataClientRegistryWritesMissingUnregistrationDiagnostic()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var registry = new DataClientRegistry(diagnostics);

        var removed = registry.Unregister<IInventoryClient>();

        Assert.False(removed);
        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.ClientUnregistrationMissing);
        Assert.Equal(DataDiagnosticSeverity.Warning, record.Severity);
        Assert.Contains(nameof(IInventoryClient), record.Message, StringComparison.Ordinal);
    }

    private interface IInventoryClient : IDataClient;

    private sealed class InventoryClient : IInventoryClient
    {
        public string ClientId => "inventory";
    }
}
