# Regira Security AI Agent Instructions

> Encryption, password hashing, JWT authentication, and API Key authentication for .NET applications.

## Projects

| Project | Package | Purpose |
|---------|---------|----------|
| `Common.Security` | `Regira.Security` | Symmetric encryption and PBKDF2 hashing |
| `Security.Hashing.BCryptNet` | `Regira.Security.Hashing.BCryptNet` | BCrypt password hashing |
| `Security.Authentication` | `Regira.Security.Authentication` | JWT tokens and API Key auth |
| `Security.Authentication.Web` | `Regira.Security.Authentication.Web` | Pre-built auth controllers |

---

## Installation

```xml
<!-- Core encryption + PBKDF2 hashing -->
<PackageReference Include="Regira.Security" Version="5.*" />

<!-- BCrypt password hashing (recommended for passwords) -->
<PackageReference Include="Regira.Security.Hashing.BCryptNet" Version="5.*" />

<!-- JWT + API Key authentication -->
<PackageReference Include="Regira.Security.Authentication" Version="5.*" />

<!-- Pre-built auth controllers (AccountController, UserController, PasswordController) -->
<PackageReference Include="Regira.Security.Authentication.Web" Version="5.*" />
```

> Add the Regira feed to `NuGet.Config`:
> ```xml
> <add key="Regira" value="https://packages.regira.com/v3/index.json" />
> ```

---

## Encryption

### `IEncrypter`

```csharp
string Encrypt(string plainText, string? key = null);
string Decrypt(string encryptedText, string? key = null);
```

### `SymmetricEncrypter` — AES-256, static key

Fast. Same key always produces the same ciphertext. Use for non-sensitive reversible encoding.

```csharp
using Regira.Security.Encryption; // SymmetricEncrypter
using Regira.Security.Core;       // CryptoOptions

var enc = new SymmetricEncrypter(new CryptoOptions { Secret = "my-app-key" });
string cipher = enc.Encrypt("sensitive value");
string plain  = enc.Decrypt(cipher);
```

### `AesEncrypter` — AES with random salt

Slower but produces different ciphertext on each call. **Recommended for stored secrets.**

```csharp
using Regira.Security.Encryption; // AesEncrypter
using Regira.Security.Core;       // CryptoOptions

var enc = new AesEncrypter(new CryptoOptions { Secret = "my-app-key" });
string cipher = enc.Encrypt("sensitive value");
string plain  = enc.Decrypt(cipher);
```

### `CryptoOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Secret` | `string?` | built-in salt key | Signing / derivation secret |
| `AlgorithmType` | `string?` | `"SHA512"` | Hash algorithm for key derivation |
| `Encoding` | `Encoding?` | UTF-8 | Text encoding |

---

## Hashing

### `IHasher` — `Regira.Security.Abstractions`

```csharp
using Regira.Security.Abstractions; // IHasher

string Hash(string? plainText);
bool   Verify(string? plainText, string hashedValue);
```

### `Hasher` — PBKDF2 (in `Regira.Security`)

Per-hash random salt + PBKDF2 digest (10 000 iterations, SHA-512, 64-byte output). Constant-time verification.

```csharp
var hasher = new Regira.Security.Hashing.Hasher();
string stored = hasher.Hash("myPassword123");
bool ok       = hasher.Verify("myPassword123", stored);  // true
```

### `BCryptNet.Hasher` — BCrypt (in `Regira.Security.Hashing.BCryptNet`)

Enhanced BCrypt (SHA-384 by default). **Recommended for passwords.**

```csharp
var hasher = new Regira.Security.Hashing.BCryptNet.Hasher();
string stored = hasher.Hash("myPassword123");
bool ok       = hasher.Verify("myPassword123", stored);
```

### `SimpleHasher` — double-SHA

Fast but weaker. Use for non-password data only (e.g. cache keys, checksums).

---

## Hashing Decision Guide

| Use case | Recommended |
|----------|-------------|
| User passwords | `BCryptNet.Hasher` |
| General data hashing with security | `Hasher` (PBKDF2) |
| Cache keys / non-security checksums | `SimpleHasher` |

---

## JWT Authentication

### `JwtTokenOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Secret` | `string` | *(required)* | HMAC signing key |
| `Algorithm` | `string?` | `HmacSha512` | Signing algorithm |
| `Authority` | `string?` | `null` | Token issuer |
| `Audience` | `string?` | `null` | Single audience |
| `Audiences` | `ICollection<string>?` | `null` | Multiple audiences |
| `LifeSpan` | `int` | `7200` | Token lifetime in seconds |
| `NameClaimType` | `string` | `"name"` | Claim used as user name |
| `RoleClaimType` | `string` | `"role"` | Claim used as role |

### `ITokenHelper` — `Regira.Security.Authentication.Jwt.Abstraction`

```csharp
using Regira.Security.Authentication.Jwt.Abstraction; // ITokenHelper
using Regira.Security.Authentication.Jwt.Models;      // JwtTokenOptions

string      Create(IEnumerable<Claim> claims, string? audience = null, int? lifeSpan = null);
Task<bool>  Validate(string token);
```

### DI Registration

```csharp
services.AddJwtAuthentication(options =>
{
    options.Secret    = configuration["Jwt:Secret"]!;
    options.Authority = configuration["Jwt:Issuer"];
    options.LifeSpan  = 3600;
});
// Registers ITokenHelper as transient and configures JwtBearer scheme.
```

### `ClaimsPrincipal` Extension Methods — `Regira.Security.Authentication.Jwt.Extensions`

```csharp
using Regira.Security.Authentication.Jwt.Extensions; // FindUserId, FindUserName, FindEmail

string? userId = User.FindUserId();    // NameIdentifier claim
string? name   = User.FindUserName();  // Name claim
string? email  = User.FindEmail();     // Email claim
```

---

## API Key Authentication

### `ApiKeyAuthenticationOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ApiKeyHeaderName` | `string` | `"X-Api-Key"` | Request header name |
| `AuthenticationType` | `string` | `"ApiKey"` | Authentication type string |

### `IApiKeyOwnerService`

```csharp
Task<ApiKeyOwner?> FindByOwner(string id);
Task<ApiKeyOwner?> FindByKey(string apiKey);
Task<bool>         Validate(string id, string apiKey);
```

### `ApiKeyOwner` Model

| Property | Type | Description |
|----------|------|-------------|
| `OwnerId` | `string` | Owner identifier |
| `Key` | `string` | API key value |
| `Roles` | `ICollection<string>` | Roles assigned to this key |

### DI Registration

```csharp
// In-memory keys from code
services.AddApiKeyAuthentication()
        .AddInMemoryApiKeyAuthentication(new[]
        {
            new ApiKeyOwner { OwnerId = "client-a", Key = "key-abc", Roles = ["read"] }
        });

// From appsettings.json
var keys = configuration.GetSection("ApiKeys").ToApiKeyOwners();
services.AddApiKeyAuthentication()
        .AddInMemoryApiKeyAuthentication(keys);
```

`appsettings.json` shape:
```json
"ApiKeys": {
  "client-a": { "Key": "key-abc", "Roles": ["read", "write"] }
}
```

---

## Pre-built Auth Controllers (`Security.Authentication.Web`)

Each base controller requires `[ApiController]` and a `[Route]` attribute on the concrete class.

### `AccountControllerBase<TUser>`

| Endpoint | Method | Description |
|----------|--------|-------------|
| `auth` | POST | Authenticate → returns JWT or lockout info |
| `auth/validate` | POST | Validates current token; returns 200 if user exists |
| `auth/refresh` | POST | Issues a fresh JWT for the current user |
| `auth/personal-data` | GET | Returns given/family names from claims |

### `UserControllerBase<TUser>`

| Endpoint | Method | Description |
|----------|--------|-------------|
| `users` | POST | Create user, send email confirmation |
| `users/confirm-email` | POST | Confirm email with token |

### `PasswordControllerBase<TUser>`

| Endpoint | Method | Description |
|----------|--------|-------------|
| `auth/password` | POST | Change password (authenticated) |
| `auth/password/recover` | POST | Send password reset email |
| `auth/password/reset` | POST | Reset password with token |

---

## Namespace Quick Reference

> **AI Agent Rule**: Always use exact namespaces. Do NOT guess or invent namespaces.

### `Regira.Security` package

| Type | Namespace |
|------|-----------|
| `IHasher` | `Regira.Security.Abstractions` |
| `Hasher` (PBKDF2) | `Regira.Security.Hashing` |
| `SimpleHasher` (double-SHA) | `Regira.Security.Hashing` |
| `IEncrypter` | `Regira.Security.Encryption` |
| `SymmetricEncrypter` | `Regira.Security.Encryption` |
| `AesEncrypter` | `Regira.Security.Encryption` |
| `CryptoOptions` | `Regira.Security.Core` |

### `Regira.Security.Hashing.BCryptNet` package

| Type | Namespace |
|------|-----------|
| `Hasher` (BCrypt) | `Regira.Security.Hashing.BCryptNet` |

### `Regira.Security.Authentication` package

| Type | Namespace |
|------|-----------|
| `ITokenHelper` | `Regira.Security.Authentication.Jwt.Abstraction` |
| `JwtTokenOptions` | `Regira.Security.Authentication.Jwt.Models` |
| `AddJwtAuthentication()` (extension) | `Regira.Security.Authentication.Jwt.Extensions` |
| `FindUserId()` / `FindUserName()` / `FindEmail()` (extensions on `ClaimsPrincipal`) | `Regira.Security.Authentication.Jwt.Extensions` |
| `IApiKeyOwnerService` | `Regira.Security.Authentication.ApiKey.Abstraction` |
| `ApiKeyOwner` | `Regira.Security.Authentication.ApiKey.Models` |
| `ApiKeyAuthenticationOptions` | `Regira.Security.Authentication.ApiKey.Models` |
| `AddApiKeyAuthentication()` (extension) | `Regira.Security.Authentication.ApiKey.Extensions` |
