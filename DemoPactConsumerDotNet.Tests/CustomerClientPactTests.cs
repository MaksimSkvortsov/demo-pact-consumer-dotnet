using DemoPactConsumerDotNet;
using PactNet;
using PactNet.Matchers;

namespace DemoPactConsumerDotNet.Tests;

public sealed class CustomerClientPactTests
{
    // These names become part of the generated Pact file name and must match
    // what the provider verifier expects: dotnet-consumer-customer-provider.json.
    private const string ConsumerName = "dotnet-consumer";
    private const string ProviderName = "customer-provider";

    [Fact]
    public async Task GetCustomer_Returns_Fields_This_Consumer_Requires()
    {
        var config = new PactConfig
        {
            // PactNet writes the generated contract here after VerifyAsync succeeds.
            // The provider repo checks out this consumer repo and reads this folder.
            PactDir = Path.Combine(GetRepositoryRoot(), "pacts")
        };

        // This creates a temporary Pact mock provider for the consumer test.
        // The real customer-provider application is not running yet.
        var pact = Pact.V3(ConsumerName, ProviderName, config).WithHttpInteractions();

        pact
            .UponReceiving("A request for customer 123")
            // This is the request the real CustomerClient must make to the mock provider.
            .WithRequest(HttpMethod.Get, "/customers/123")
            .WillRespond()
            .WithStatus(200)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new
            {
                // This consumer only depends on id and email. Do not add name or
                // phone here, otherwise the provider would be forced to keep them.
                id = Match.Type(123),
                email = Match.Type("john@example.com")
            });

        await pact.VerifyAsync(async context =>
        {
            using var httpClient = new HttpClient
            {
                // context.MockServerUri is the temporary Pact mock provider URL.
                // The real typed client is pointed at it for this test only.
                BaseAddress = context.MockServerUri
            };

            var client = new CustomerClient(httpClient);
            var customer = await client.GetCustomerAsync(123);

            // These assertions prove the consumer code can read the fields it uses.
            // Pact separately records the request and response expectations as JSON.
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
