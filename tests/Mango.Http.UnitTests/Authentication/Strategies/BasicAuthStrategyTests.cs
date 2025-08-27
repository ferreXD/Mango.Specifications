// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using FluentAssertions;
    using Mango.Http.Authorization;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Diagnostics;

    public class BasicAuthStrategyTests
    {
        private readonly ActivitySource _activitySource = new("TestSource");
        private readonly Mock<ILogger<BasicAuthStrategy>> _loggerMock = new();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Ctor_InvalidUsername_ThrowsArgumentException(string username)
        {
            // Arrange
            var password = "secret";

            // Act
            Action act = () => new BasicAuthStrategy(
                username!,
                password,
                _activitySource,
                _loggerMock.Object);

            // Assert
            act.Should()
               .Throw<ArgumentException>()
               .WithMessage("username required*")
               .And.ParamName.Should().Be("username");
        }

        [Fact]
        public void Ctor_NullPassword_ThrowsArgumentNullException()
        {
            // Arrange
            var username = "user";

            // Act
            Action act = () => new BasicAuthStrategy(
                username,
                null!,
                _activitySource,
                _loggerMock.Object);

            // Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("password");
        }
    }
}
