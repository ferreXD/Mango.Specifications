// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using Authorization;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;

    public class DelegateAuthenticationStrategyFactoryTests
    {
        private readonly Mock<ILogger<DelegateAuthenticationStrategyFactory>> _loggerMock = new();

        [Fact]
        public void Ctor_NullFactory_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new DelegateAuthenticationStrategyFactory(
                null!,
                _loggerMock.Object);

            // Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("factory");
        }

        [Fact]
        public void Ctor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            Func<IServiceProvider, IAuthenticationStrategy> factory = _ => new Mock<IAuthenticationStrategy>().Object;

            // Act
            Action act = () => new DelegateAuthenticationStrategyFactory(
                factory,
                null!);

            // Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("logger");
        }

        [Fact]
        public void Create_ValidFactory_InvokesFactoryAndLogs()
        {
            // Arrange
            var sp = new object() as IServiceProvider;
            var createdStrategy = new Mock<IAuthenticationStrategy>().Object;
            var factoryMock = new Mock<Func<IServiceProvider, IAuthenticationStrategy>>();
            factoryMock
                .Setup(f => f(sp!))
                .Returns(createdStrategy);

            var sut = new DelegateAuthenticationStrategyFactory(
                factoryMock.Object,
                _loggerMock.Object);

            // Act
            var result = sut.Create(sp!);

            // Assert
            result.Should().BeSameAs(createdStrategy, "factory result should be returned");

            _loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Creating strategy via delegate"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once, "should log debug before invoking factory");

            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == $"Created strategy {createdStrategy.GetType().Name}"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once, "should log information with the created strategy name");

            factoryMock.Verify(f => f(sp!), Times.Once);
        }

        [Fact]
        public void Create_FactoryThrows_ExceptionPropagatesAndInfoNotLogged()
        {
            // Arrange
            var sp = new object() as IServiceProvider;
            var expectedEx = new InvalidOperationException("factory error");
            Func<IServiceProvider, IAuthenticationStrategy> factory = _ => throw expectedEx;

            var sut = new DelegateAuthenticationStrategyFactory(
                factory,
                _loggerMock.Object);

            // Act
            Action act = () => sut.Create(sp!);

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .Which.Should().BeSameAs(expectedEx);

            _loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString() == "Creating strategy via delegate"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once, "should log debug even if factory throws");

            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never, "should not log information when factory fails");
        }
    }
}
