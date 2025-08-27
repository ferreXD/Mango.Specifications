namespace Mango.Http.IntegrationTests
{
    using Constants;
    using FluentAssertions;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;

    public class ResiliencyHandlerIntegrationTests
    {
        private const string ClientName = "resilient-client";

        /// <summary>
        /// A primary handler that always throws, and counts how many times it's called.
        /// </summary>
        private class FailingHandler : HttpMessageHandler
        {
            public int CallCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                // Simulate a transient server error
                throw new HttpRequestException("simulated failure");
            }
        }

        [Fact]
        public async Task DefaultResiliencyPolicy_RetriesExactlyDefaultMaxRetryCountTimes()
        {
            // Arrange
            var fakeHandler = new FailingHandler();
            var services = new ServiceCollection()
                .AddLogging()
                .AddOptions();

            // 1) Register the Mango client, override its primary handler to our fake
            // 2) Apply the default resiliency policy (3 retries)
            services
                .AddMangoHttpClient(ClientName, client => { })
                // override the primary HTTP handler
                .ConfigurePrimaryHttpMessageHandler(() => fakeHandler)
                // apply the default resiliency via the builder API
                .WithResiliency(cfg => cfg.WithRetry(opt => opt
                    .SetUseJitter(RetryPolicyDefaults.DefaultUseJitter)
                    .SetMaxRetryCount(RetryPolicyDefaults.DefaultMaxRetryCount)
                    .SetDelay(RetryPolicyDefaults.DefaultDelay)));

            var sp = services.BuildServiceProvider();
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient(ClientName);

            // Act
            Func<Task> act = () => client.GetAsync("http://example.local/", CancellationToken.None);
            await act.Should()
                     .ThrowAsync<HttpRequestException>("the fake handler always fails");

            // Assert: default retry count is 3, so total calls = 1 initial + 3 retries = 4
            fakeHandler.CallCount.Should().Be(
                RetryPolicyDefaults.DefaultMaxRetryCount + 1,
                "the default retry policy should retry exactly DefaultMaxRetryCount times");
        }

        [Fact]
        public async Task WithDefaults_Generic_AlsoRetriesAccordingly()
        {
            // Arrange
            var fakeHandler = new FailingHandler();
            var services = new ServiceCollection()
                .AddLogging()
                .AddOptions();

            // 1) Register the Mango client, override its primary handler to our fake
            services.AddMangoDefaultHttpLogger();

            // Use the generic WithDefaults<TLogger>, which under the hood applies DefaultResiliency
            services
                .AddMangoHttpClient(ClientName, null)
                .ConfigurePrimaryHttpMessageHandler(() => fakeHandler)
                .WithDefaults<DefaultHttpLogger>();

            var sp = services.BuildServiceProvider();
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(ClientName);

            // Act
            await client.Invoking(c => c.GetAsync("http://example.local/")).Should()
                        .ThrowAsync<HttpRequestException>();

            // Assert same retry behavior
            fakeHandler.CallCount.Should().Be(
                RetryPolicyDefaults.DefaultMaxRetryCount + 1,
                "WithDefaults<TLogger>() includes the default retry policy");
        }
    }
}
