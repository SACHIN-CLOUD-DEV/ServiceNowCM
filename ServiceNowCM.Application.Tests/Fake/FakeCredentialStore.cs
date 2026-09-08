using ServiceNowCM.Application.Interfaces;

namespace ServiceNowCM.Application.Tests.Fakes
{
    public class FakeCredentialStore : ICredentialStore
    {
        private readonly Dictionary<string, string> _secrets = new();

        public Task<string> StoreAsync(
            string key,
            string secret)
        {
            _secrets[key] = secret;

            return Task.FromResult(key);
        }

        public Task<string?> GetAsync(
            string reference)
        {
            _secrets.TryGetValue(
                reference,
                out var secret);

            return Task.FromResult(secret);
        }

        public Task DeleteAsync(
            string reference)
        {
            _secrets.Remove(reference);

            return Task.CompletedTask;
        }
    }
}
