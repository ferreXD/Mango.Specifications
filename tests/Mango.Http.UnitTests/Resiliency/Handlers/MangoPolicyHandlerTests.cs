// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Resiliency
{
    using FluentAssertions;
    using Http.Diagnostics;
    using Mango.Http.Resiliency;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Polly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class MangoPolicyHandlerTests
    {
        private class FakeHandler : DelegatingHandler
        {
            public HttpRequestMessage? Request { get; private set; }
            public HttpResponseMessage Response { get; } = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;
                return Task.FromResult(Response);
            }
        }

        private record DummyPolicyDefinition : ResiliencyPolicyDefinition
        {
            private readonly Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>> _factory;
            public DummyPolicyDefinition(int order, Func<IResiliencyDiagnostics?, IAsyncPolicy<HttpResponseMessage>> factory)
                : base(order) => _factory = factory;
            public override IAsyncPolicy<HttpResponseMessage> BuildPolicy(IResiliencyDiagnostics? diagnostics, ILogger? logger = null) => _factory(diagnostics);
        }

        private class ThrowingHandler : FakeHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException("Simulated failure");
            }
        }

        private static async Task<HttpResponseMessage> SendThroughHandler(HttpMessageHandler handler, HttpRequestMessage request, CancellationToken ct)
        {
            using var invoker = new HttpMessageInvoker(handler);
            return await invoker.SendAsync(request, ct);
        }

        [Fact]
        public async Task SendAsync_NoDefinitions_CallsInnerHandler()
        {
            var inner = new FakeHandler();
            var handler = new MangoPolicyHandler(Enumerable.Empty<ResiliencyPolicyDefinition>(), null)
            {
                InnerHandler = inner
            };
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            var response = await SendThroughHandler(handler, request, CancellationToken.None);

            inner.Request.Should().BeSameAs(request);
            response.Should().Be(inner.Response);
        }

        [Fact]
        public async Task SendAsync_SingleBuiltInPolicy_UsesThatPolicy()
        {
            var inner = new FakeHandler();
            var built = false;
            var def = new DummyPolicyDefinition(1, diag =>
            {
                built = true;
                return Policy.NoOpAsync<HttpResponseMessage>();
            });
            var handler = new MangoPolicyHandler(new[] { def }, null)
            {
                InnerHandler = inner
            };
            var request = new HttpRequestMessage(HttpMethod.Post, "http://single");

            var response = await SendThroughHandler(handler, request, CancellationToken.None);

            built.Should().BeTrue();
            inner.Request.Should().BeSameAs(request);
            response.Should().Be(inner.Response);
        }

        [Fact]
        public async Task SendAsync_MultipleBuiltInPolicies_WrapsIntoPolicyWrap()
        {
            var inner = new FakeHandler();
            var builtCount = 0;
            var defs = new List<ResiliencyPolicyDefinition>
            {
                new DummyPolicyDefinition(2, diag => { builtCount++; return Policy.NoOpAsync<HttpResponseMessage>(); }),
                new DummyPolicyDefinition(1, diag => { builtCount++; return Policy.NoOpAsync<HttpResponseMessage>(); })
            };
            var handler = new MangoPolicyHandler(defs, null)
            {
                InnerHandler = inner
            };
            var request = new HttpRequestMessage(HttpMethod.Put, "http://wrap");

            var response = await SendThroughHandler(handler, request, CancellationToken.None);

            builtCount.Should().Be(2);
            inner.Request.Should().BeSameAs(request);
            response.Should().Be(inner.Response);
        }

        [Fact]
        public async Task SendAsync_CustomPolicyTakingPrecedence_IgnoresBuiltIns()
        {
            // Arrange: inner handler throws to trigger fallback
            var inner = new ThrowingHandler();
            var customInvoked = false;
            IResiliencyDiagnostics? capturedDiag = null;
            var customPolicy = new CustomPolicyDefinition(5, diag =>
            {
                capturedDiag = diag;
                return Policy<HttpResponseMessage>
                    .Handle<HttpRequestException>()
                    .FallbackAsync(
                        fallbackAction: (_, __, ___) =>
                        {
                            customInvoked = true;
                            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                        },
                        onFallbackAsync: (_, __) => Task.CompletedTask
                    );
            });
            var builtDef = new DummyPolicyDefinition(1, diag => Policy.NoOpAsync<HttpResponseMessage>());
            var diagMock = new Mock<IResiliencyDiagnostics>().Object;
            var handler = new MangoPolicyHandler(new ResiliencyPolicyDefinition[] { builtDef, customPolicy }, diagMock)
            {
                InnerHandler = inner
            };
            var request = new HttpRequestMessage(HttpMethod.Delete, "http://custom");

            // Act
            var response = await SendThroughHandler(handler, request, CancellationToken.None);

            // Assert
            customInvoked.Should().BeTrue();
            capturedDiag.Should().BeSameAs(diagMock);
            // Inner handler never assigns request due to exception
            Assert.Null((inner as FakeHandler)?.Request);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }


        [Fact]
        public async Task SendAsync_CustomPolicyStillCallsBaseIfNoFallback_ExecutesInner()
        {
            var inner = new FakeHandler();
            var customPolicy = new CustomPolicyDefinition(1, diag => Policy.NoOpAsync<HttpResponseMessage>());
            var handler = new MangoPolicyHandler(new ResiliencyPolicyDefinition[] { customPolicy }, null)
            {
                InnerHandler = inner
            };
            var request = new HttpRequestMessage(HttpMethod.Get, "http://nopolicy");

            var response = await SendThroughHandler(handler, request, CancellationToken.None);

            inner.Request.Should().BeSameAs(request);
            response.Should().Be(inner.Response);
        }
    }
}
