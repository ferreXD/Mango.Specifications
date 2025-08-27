// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Headers
{
    using FluentAssertions;
    using Mango.Http.Headers;
    using System.Net;
    using System.Threading.Tasks;

    public class HttpHeadersInjectionHandlerTests
    {
        private class FakeHandler(HttpResponseMessage response) : DelegatingHandler
        {
            public HttpRequestMessage? CapturedRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CapturedRequest = request;
                return Task.FromResult(response);
            }
        }

        private static async Task<HttpResponseMessage> InvokeHandlerAsync(DelegatingHandler handler)
        {
            using var invoker = new HttpMessageInvoker(handler);
            return await InvokeHandlerAsync(invoker);
        }

        private static async Task<HttpResponseMessage> InvokeHandlerAsync(HttpMessageInvoker invoker)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            return await invoker.SendAsync(request, CancellationToken.None);
        }

        [Fact]
        public async Task SendAsync_NoCustomHeaders_DoesNotAddHeaders()
        {
            // Arrange
            var options = new HttpHeadersOptions();
            options.CustomHeaders.Clear();
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var inner = new FakeHandler(fakeResponse);
            var handler = new HttpHeadersInjectionHandler(options)
            {
                InnerHandler = inner
            };

            // Act
            var response = await InvokeHandlerAsync(handler);

            // Assert
            response.Should().BeSameAs(fakeResponse);
            inner.CapturedRequest.Should().NotBeNull();
            inner.CapturedRequest!.Headers.Should().BeEmpty();
        }

        [Fact]
        public async Task SendAsync_CustomHeaders_AddsValidHeadersOnly()
        {
            // Arrange
            var options = new HttpHeadersOptions();
            options.CustomHeaders.Clear();
            options.CustomHeaders.Add("X-Valid", () => "Value");
            options.CustomHeaders.Add("X-Empty", () => "");
            options.CustomHeaders.Add("X-Whitespace", () => "   ");
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.Accepted);
            var inner = new FakeHandler(fakeResponse);
            var handler = new HttpHeadersInjectionHandler(options)
            {
                InnerHandler = inner
            };

            // Act
            var response = await InvokeHandlerAsync(handler);

            // Assert
            response.Should().BeSameAs(fakeResponse);
            var req = inner.CapturedRequest!;
            req.Headers.Contains("X-Valid").Should().BeTrue();
            req.Headers.GetValues("X-Valid").Should().ContainSingle("Value");
            req.Headers.Contains("X-Empty").Should().BeFalse();
            req.Headers.Contains("X-Whitespace").Should().BeFalse();
        }

        [Fact]
        public async Task SendAsync_MultipleFactories_ValueFactoriesInvokedPerSend()
        {
            // Arrange
            var options = new HttpHeadersOptions();
            options.CustomHeaders.Clear();
            var callCount = 0;
            options.CustomHeaders.Add("X-Count", () =>
            {
                callCount++;
                return callCount.ToString();
            });

            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var inner = new FakeHandler(fakeResponse);
            var handler = new HttpHeadersInjectionHandler(options)
            {
                InnerHandler = inner
            };

            using var invoker = new HttpMessageInvoker(handler);

            // Act
            await InvokeHandlerAsync(invoker);
            await InvokeHandlerAsync(invoker);

            // Assert
            callCount.Should().Be(2, because: "value factory should be invoked once per request");
            // Check last invocation header
            var req = inner.CapturedRequest!;
            req.Headers.GetValues("X-Count").Should().ContainSingle("2");
        }
    }
}
