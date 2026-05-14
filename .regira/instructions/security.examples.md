# Security — Example: Staff Portal Authentication

> Context: A back-office web API uses JWT for staff login and BCrypt for password storage. A CI/CD integration uses API Key authentication.

## DI Registration

```csharp
// Program.cs
services.AddSingleton<IHasher, Regira.Security.Hashing.BCryptNet.Hasher>();

services.AddJwtAuthentication(options =>
{
    options.Secret    = configuration["Jwt:Secret"]!;
    options.Authority = configuration["Jwt:Issuer"];
    options.LifeSpan  = 3600;
});

services.AddApiKeyAuthentication()
        .AddInMemoryApiKeyAuthentication(
            configuration.GetSection("ApiKeys").ToApiKeyOwners());
```

`appsettings.json`:
```json
"ApiKeys": {
  "ci-pipeline": { "Key": "ci-secret-key", "Roles": ["read"] }
}
```

## Register a new staff member

```csharp
public async Task Register(string email, string plainPassword)
{
    var stored = _hasher.Hash(plainPassword);
    await _userRepository.Add(new StaffUser { Email = email, PasswordHash = stored });
}
```

## Login and issue a JWT

```csharp
public async Task<string?> Login(string email, string plainPassword)
{
    var user = await _userRepository.FindByEmail(email);
    if (user == null || !_hasher.Verify(plainPassword, user.PasswordHash!))
        return null;

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name,           user.DisplayName ?? email),
        new Claim(ClaimTypes.Email,          email),
        new Claim(ClaimTypes.Role,           user.Role)
    };
    return _tokenHelper.Create(claims);
}
```

## Encrypt a third-party API key at rest

```csharp
// AesEncrypter produces a different ciphertext each call — safe for stored secrets
var enc = new AesEncrypter(new CryptoOptions { Secret = configuration["Crypto:Secret"] });

string stored = enc.Encrypt(apiKey);   // store in DB
string plain  = enc.Decrypt(stored);   // retrieve when needed
```
