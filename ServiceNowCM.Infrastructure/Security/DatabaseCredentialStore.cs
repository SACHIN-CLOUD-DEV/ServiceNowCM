using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Infrastructure.Persistence;

namespace ServiceNowCM.Infrastructure.Security;

public class DatabaseCredentialStore : ICredentialStore
{
    private readonly ServiceNowCMDbContext _dbContext;
    private readonly IDataProtector _protector;

    public DatabaseCredentialStore(
        ServiceNowCMDbContext dbContext,
        IDataProtectionProvider dataProtectionProvider)
    {
        _dbContext = dbContext;

        _protector = dataProtectionProvider.CreateProtector("ServiceNowCM.CredentialStore.v1");
    }

    public async Task<string> StoreAsync(string key,string secret)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException(
                "Credential key is required.",
                nameof(key));

        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException(
                "Credential secret is required.",
                nameof(secret));

        var encryptedSecret = _protector.Protect(secret);

        var existingCredential =
            await _dbContext.StoredCredentials.FirstOrDefaultAsync(x => x.Reference == key);

        if (existingCredential == null)
        {
            var credential = new StoredCredential(
                key,
                encryptedSecret);

            await _dbContext.StoredCredentials.AddAsync(
                credential);
        }
        else
        {
            existingCredential.UpdateSecret(
                encryptedSecret);
        }

        await _dbContext.SaveChangesAsync();

        return key;
    }

    public async Task<string?> GetAsync(
        string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return null;

        var credential =
            await _dbContext.StoredCredentials
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Reference == reference);

        if (credential == null)
            return null;

        return _protector.Unprotect(
            credential.EncryptedSecret);
    }

    public async Task DeleteAsync(
        string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return;

        var credential =
            await _dbContext.StoredCredentials
                .FirstOrDefaultAsync(
                    x => x.Reference == reference);

        if (credential == null)
            return;

        _dbContext.StoredCredentials.Remove(
            credential);

        await _dbContext.SaveChangesAsync();
    }
}