using ServiceNowCM.Application.Interfaces;
using System.Collections.Concurrent;

namespace ServiceNowCM.Infrastructure.Security
{
    public class InMemoryCredentialStore : ICredentialStore
    {
        private readonly ConcurrentDictionary<string, string> _secrets = new();

        public Task<string> StoreAsync(string key,string secret)
        {
            _secrets[key] = secret;
            return Task.FromResult(key);
        }

        public Task<string?> GetAsync(string reference)
        {
            _secrets.TryGetValue(reference,out var secret);
            return Task.FromResult(secret);
        }

        public Task DeleteAsync(string reference)
        {
            _secrets.TryRemove(reference,out _);
            return Task.CompletedTask;
        }
    }
}