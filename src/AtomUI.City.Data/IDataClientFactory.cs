namespace AtomUI.City.Data;

public interface IDataClientFactory
{
    TClient GetRequiredClient<TClient>()
        where TClient : class, IDataClient;
}
