// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Hooks
{
    using FluentAssertions;
    using Mango.Http.Hooks;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class MangoHttpHookHandlerTests
    {
        private const string RequestKey = "Request";

        private class FakeHandler : DelegatingHandler
        {
            public HttpRequestMessage? ReceivedRequest { get; private set; }
            private readonly HttpResponseMessage _response;

            public FakeHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                ReceivedRequest = request;
                return Task.FromResult(_response);
            }
        }

        private static Task<HttpResponseMessage> SendThroughHandler(DelegatingHandler handler)
        {
            using var invoker = new HttpMessageInvoker(handler);
            return invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://test"), CancellationToken.None);
        }

        [Fact]
        public async Task SendAsync_PreAndPostHooks_Successful_ShouldInvokeBothAndReturnResponse()
        {
            // Arrange
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var inner = new FakeHandler(fakeResponse);
            var preInvoked = false;
            var postInvoked = false;
            var options = new HttpRequestHookOptions
            {
                PreRequestAsync = (req, ctx, ct) =>
                {
                    preInvoked = true;
                    return Task.CompletedTask;
                },
                PostResponseAsync = (resp, ctx, ct) =>
                {
                    postInvoked = true;
                    return Task.CompletedTask;
                }
            };
            var handler = new MangoHttpHookHandler(options)
            {
                InnerHandler = inner
            };

            // Act
            var response = await SendThroughHandler(handler);

            // Assert
            response.Should().BeSameAs(fakeResponse);
            preInvoked.Should().BeTrue();
            postInvoked.Should().BeTrue();
            inner.ReceivedRequest.Should().NotBeNull();
        }

        [Fact]
        public async Task SendAsync_PreHookThrows_ShouldSwallowAndInvokePost()
        {
            // Arrange
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.Accepted);
            var inner = new FakeHandler(fakeResponse);
            var preInvoked = false;
            var postInvoked = false;
            var options = new HttpRequestHookOptions
            {
                PreRequestAsync = (req, ctx, ct) =>
                {
                    preInvoked = true;
                    throw new InvalidOperationException("Pre failed");
                },
                PostResponseAsync = (resp, ctx, ct) =>
                {
                    postInvoked = true;
                    return Task.CompletedTask;
                }
            };
            var handler = new MangoHttpHookHandler(options)
            {
                InnerHandler = inner
            };

            // Act
            var response = await SendThroughHandler(handler);

            // Assert
            response.Should().BeSameAs(fakeResponse);
            preInvoked.Should().BeTrue();
            postInvoked.Should().BeTrue();
            inner.ReceivedRequest.Should().NotBeNull();
        }

        [Fact]
        public async Task SendAsync_PostHookThrows_ShouldSwallowAndReturnResponse()
        {
            // Arrange
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
            var inner = new FakeHandler(fakeResponse);
            var preInvoked = false;
            var postInvoked = false;
            var options = new HttpRequestHookOptions
            {
                PreRequestAsync = (req, ctx, ct) =>
                {
                    preInvoked = true;
                    return Task.CompletedTask;
                },
                PostResponseAsync = (resp, ctx, ct) =>
                {
                    postInvoked = true;
                    throw new InvalidOperationException("Post failed");
                }
            };
            var handler = new MangoHttpHookHandler(options)
            {
                InnerHandler = inner
            };

            // Act
            var response = await SendThroughHandler(handler);

            // Assert
            response.Should().BeSameAs(fakeResponse);
            preInvoked.Should().BeTrue();
            postInvoked.Should().BeTrue();
            inner.ReceivedRequest.Should().NotBeNull();
        }
    }
}
