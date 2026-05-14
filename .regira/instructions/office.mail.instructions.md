# Regira Office.Mail AI Agent Instructions

---

## Module Context

Part of **Regira Office**. For routing and full module overview, see [`office.instructions.md`](./office.instructions.md).

| Namespace | Covers |
|-----------|--------|
| `Regira.Office.Mail` | Email sending via SendGrid and Mailgun |

**Related:**
- [IO.Storage](../../Common.IO.Storage/ai/io.storage.instructions.md) — `INamedFile` used for email attachments

---

## Installation

```xml
<!-- SendGrid -->
<PackageReference Include="Regira.Mail.SendGrid" Version="5.*" />

<!-- Mailgun -->
<PackageReference Include="Regira.Mail.MailGun" Version="5.*" />
```

> Add the Regira feed to `NuGet.Config`:
> ```xml
> <add key="Regira" value="https://packages.regira.com/v3/index.json" />
> ```

---

## `IMailer`

Both backends implement this interface.

```csharp
// Parameter-based
Task<IMailResponse> Send(
    IMailAddress                sender,
    IEnumerable<IMailRecipient> recipients,
    string?                     subject,
    string?                     message,
    bool                        isHtml      = true,
    IEnumerable<INamedFile>?    attachments = null,
    CancellationToken           cancellationToken = default);

// Full message object
Task<IMailResponse> Send(IMessageObject message, CancellationToken cancellationToken = default);
```

### `IMailResponse`

| Property | Type | Description |
|----------|------|-------------|
| `Success` | `bool` | `true` when the provider accepted the message |
| `Status` | `string?` | HTTP status code or provider status text |
| `Content` | `string?` | Raw response body |
| `Exception` | `Exception?` | Set when sending fails |

---

## Core Models

### `IMessageObject` / `MessageObject`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `From` | `IMailAddress?` | `null` | Sender address |
| `To` | `ICollection<IMailRecipient>` | `[]` | Recipients (To / Cc / Bcc) |
| `ReplyTo` | `IMailAddress?` | `null` | Reply-To address |
| `Subject` | `string?` | `null` | Email subject |
| `Body` | `string?` | `null` | Message body |
| `IsHtml` | `bool` | `true` | HTML vs plain text |
| `Attachments` | `ICollection<INamedFile>?` | `null` | File attachments |

### `IMailAddress` / `MailAddress`

| Property | Type | Description |
|----------|------|-------------|
| `Email` | `string` | Email address — validated on assignment |
| `DisplayName` | `string?` | Optional display name |

```csharp
MailAddress addr  = "alice@example.com";  // implicit from string
MailAddress named = new() { Email = "alice@example.com", DisplayName = "Alice" };
```

`ToString()` returns `"Alice <alice@example.com>"` when `DisplayName` is set.

### `IMailRecipient` / `MailRecipient`

Extends `IMailAddress` with a recipient type.

```csharp
MailRecipient to  = "alice@example.com";                                              // defaults to To
var cc  = new MailRecipient { Email = "bob@example.com",   RecipientType = RecipientTypes.Cc };
var bcc = new MailRecipient { Email = "carol@example.com", RecipientType = RecipientTypes.Bcc };
```

---

## Configuration

### `SendGridConfig`

| Property | Type | Description |
|----------|------|-------------|
| `Key` | `string` | SendGrid API key |

### `MailgunConfig`

| Property | Type | Description |
|----------|------|-------------|
| `Api` | `string` | Mailgun API endpoint (e.g. `https://api.mailgun.net/v3`) |
| `Key` | `string` | Mailgun API key |
| `Domain` | `string` | Sending domain |

---

## DI Registration

```csharp
// SendGrid
services.AddSendGrid(cfg => cfg.Key = configuration["Mail:SendGrid:Key"]!);

// Mailgun
services.AddMailGun(cfg =>
{
    cfg.Api    = configuration["Mail:MailGun:Api"]!;
    cfg.Key    = configuration["Mail:MailGun:Key"]!;
    cfg.Domain = configuration["Mail:MailGun:Domain"]!;
});
```

Both extension methods register `IMailer` as a singleton.

---

## Exceptions

### `MailException`

| Property | Type | Description |
|----------|------|-------------|
| `MessageObject` | `IMessageObject?` | The message that failed to send |
| `ResponseContent` | `string?` | Raw provider response body |

### `EmailFormatException`

| Property | Type | Description |
|----------|------|-------------|
| `EmailInput` | `string?` | The invalid email address value |

---

## Testing — `DummyMailer`

Implements `IMailer` and does nothing. Use in tests to suppress actual sending:

```csharp
services.AddSingleton<IMailer, DummyMailer>();
```

---

## Web DTOs — `MailInput`

Accept email requests over HTTP with `Mail.Web`:

```csharp
[HttpPost]
public async Task<IActionResult> Send([FromBody] MailInput input, IMailer mailer)
{
    var message = input.ToMessageObject();
    var result  = await mailer.Send(message);
    return result.Success ? Ok() : StatusCode(502);
}
```

---

## ASP.NET Identity Integration

```csharp
services.AddSingleton<IEmailSender>(provider =>
    new IdentityMailer(
        provider.GetRequiredService<IMailer>(),
        new IdentityMailerOptions { Sender = "no-reply@example.com" }
    ));
```

---
