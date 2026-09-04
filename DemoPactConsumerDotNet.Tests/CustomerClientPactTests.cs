using DemoPactConsumerDotNet;
using PactNet;
using PactNet.Matchers;

namespace DemoPactConsumerDotNet.Tests;

public sealed class CustomerClientPactTests
{
    private const string ConsumerName = "dotnet-consumer";
    private const string ProviderName = "customer-provider";

    [Fact]
    public async Task GetCustomer_Returns_Fields_This_Consumer_Requires()
    {
        var config = new PactConfig
        {
            PactDir = Path.Combine(GetRepositoryRoot(), "pacts")
        };

        var pact = Pact.V3(ConsumerName, ProviderName, config).WithHttpInteractions();

        pact
            .UponReceiving("A request for customer 123")
            .WithRequest(HttpMethod.Get, "/customers/123")
            .WillRespond()
            .WithStatus(200)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new
            {
                id = Match.Type(123),
                email = Match.Type("john@example.com")
            });

        await pact.VerifyAsync(async context =>
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = context.MockServerUri
            };

            var client = new CustomerClient(httpClient);
            var customer = await client.GetCustomerAsync(123);

            Assert.Equal(123, customer.Id);
            Assert.False(string.IsNullOrWhiteSpace(customer.Email));
        });
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DemoPactConsumerDotNet.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root.");
    }
}
