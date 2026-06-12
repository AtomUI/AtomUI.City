using AtomUI.City.Security;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Data.Tests;

public sealed class DataRegistrationTests
{
    [Fact]
    public void AddDataRegistersCoreServices()
    {
        var services = new ServiceCollection();

        services.AddSecurity();
        services.AddData();

        using var serviceProvider = services.BuildServiceProvider();
        var pipeline = serviceProvider.GetRequiredService<IDataRequestPipeline>();
        var credentialProvider = serviceProvider.GetRequiredService<IDataCredentialProvider>();
        var cache = serviceProvider.GetRequiredService<IDataRequestCache>();

        Assert.IsType<DataRequestPipeline>(pipeline);
        Assert.IsType<AccessTokenCredentialProvider>(credentialProvider);
        Assert.IsType<InMemoryDataRequestCache>(cache);
    }

    [Fact]
    public async Task AddDataSupportsBearerRequestWithoutFullSecurityRegistration()
    {
        var services = new ServiceCollection();

        services.AddData();

        using var serviceProvider = services.BuildServiceProvider();
        var pipeline = serviceProvider.GetRequiredService<IDataRequestPipeline>();

        var result = await pipeline.SendAsync(
            new DataRequest<string>("catalog", "secure-items", DataTransportKind.Http)
            {
                Authentication = DataAuthenticationOptions.Bearer(),
            });

        Assert.False(result.Succeeded);
        Assert.Equal(DataErrorKind.CredentialUnavailable, result.Error?.Kind);
    }
}
