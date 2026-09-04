using System.Net.Http.Json;

namespace DemoPactConsumerDotNet;

public sealed class CustomerClient
{
    private readonly HttpClient httpClient;

    public CustomerClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<CustomerSummary> GetCustomerAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await httpClient.GetFromJsonAsync<CustomerSummary>($"/customers/{id}", cancellationToken);

        return customer ?? throw new InvalidOperationException("The customer response body was empty.");
    }
}
