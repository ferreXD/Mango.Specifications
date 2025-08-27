// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Registry
{
    using FluentAssertions;
    using Http.Registry;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class DefaultMangoHttpClientRegistryTests
    {
        private readonly DefaultMangoHttpClientRegistry _registry = new DefaultMangoHttpClientRegistry();

        [Fact]
        public void Register_NullName_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => _registry.Register(null!, new DummyClientBuilder());

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("name");
        }

        [Fact]
        public void Register_NullBuilder_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => _registry.Register("client1", null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("builder");
        }

        [Fact]
        public void Register_DuplicateName_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var builder1 = new DummyClientBuilder();
            var builder2 = new DummyClientBuilder();
            _registry.Register("duplicate", builder1);

            // Act
            Action act = () => _registry.Register("duplicate", builder2);

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("A client with the name 'duplicate' is already registered.");
        }

        [Fact]
        public void Register_Valid_ShouldBeRetrievableViaTryGetAndClients()
        {
            // Arrange
            var name = "myClient";
            var builder = new DummyClientBuilder();

            // Act
            _registry.Register(name, builder);
            var result = _registry.TryGet(name, out var retrieved);

            // Assert
            result.Should().BeTrue();
            retrieved.Should().BeSameAs(builder);
            _registry.Clients.Should().ContainKey(name).WhoseValue.Should().BeSameAs(builder);
        }

        [Fact]
        public void TryGet_NullName_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => _registry.TryGet(null!, out _);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("name");
        }

        [Fact]
        public void TryGet_UnknownName_ShouldReturnFalseAndNull()
        {
            // Act
            var result = _registry.TryGet("unknown", out var builder);

            // Assert
            result.Should().BeFalse();
            builder.Should().BeNull();
        }

        // Dummy IMangoHttpClientBuilder implementation
        private class DummyClientBuilder : IMangoHttpClientBuilder
        {
            public string Name { get; } = "DummyClient";
            public IServiceCollection Services { get; } = new ServiceCollection();
        }
    }
}
