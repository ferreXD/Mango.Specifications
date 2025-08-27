// ReSharper disable once CheckNamespace
namespace Mango.Http.Registry
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal sealed class DefaultMangoHttpClientRegistry : IMangoHttpClientRegistry
    {
        private readonly ConcurrentDictionary<string, IMangoHttpClientBuilder> _clients = new();

        /// <summary>
        /// Register a new HTTP-client builder under the given name.
        /// Throws if that name is already in use.
        /// </summary>
        public void Register(string name, IMangoHttpClientBuilder builder)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            if (!_clients.TryAdd(name, builder))
                throw new InvalidOperationException($"A client with the name '{name}' is already registered.");
        }

        /// <summary>
        /// Try to look up a registered builder by name.
        /// </summary>
        public bool TryGet(string name, out IMangoHttpClientBuilder? builder)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));
            return _clients.TryGetValue(name, out builder);
        }

        /// <summary>
        /// A read-only snapshot of all registered builders.
        /// </summary>
        public IReadOnlyDictionary<string, IMangoHttpClientBuilder> Clients
            => new ReadOnlyDictionary<string, IMangoHttpClientBuilder>(_clients);
    }
}
