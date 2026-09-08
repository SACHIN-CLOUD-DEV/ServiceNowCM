namespace ServiceNowCM.Domain.Entities;

public class StoredCredential
{
    public string Reference { get; private set; } = null!;

    public string EncryptedSecret { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime ModifiedAtUtc { get; private set; }

    private StoredCredential()
    {
        // Required by EF Core
    }

    public StoredCredential(
        string reference,
        string encryptedSecret)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException(
                "Credential reference is required.",
                nameof(reference));

        if (string.IsNullOrWhiteSpace(encryptedSecret))
            throw new ArgumentException(
                "Encrypted secret is required.",
                nameof(encryptedSecret));

        Reference = reference.Trim();
        EncryptedSecret = encryptedSecret;

        CreatedAtUtc = DateTime.UtcNow;
        ModifiedAtUtc = DateTime.UtcNow;
    }

    public void UpdateSecret(string encryptedSecret)
    {
        if (string.IsNullOrWhiteSpace(encryptedSecret))
            throw new ArgumentException(
                "Encrypted secret is required.",
                nameof(encryptedSecret));

        EncryptedSecret = encryptedSecret;
        ModifiedAtUtc = DateTime.UtcNow;
    }
}