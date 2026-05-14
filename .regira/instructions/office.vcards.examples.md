# Office.VCards — Example: Contact Directory Export

> Context: A CRM lets users download a single contact or their entire contact list as a `.vcf` file.

## Export a single contact

```csharp
public string ExportContact(Contact contact)
{
    var manager = new VCardManager();

    var vCard = new VCard();
    vCard.NameViews = new NameProperty(contact.LastName, contact.FirstName);
    vCard.EmailAddresses = [new TextProperty(contact.Email)];
    vCard.Phones = [new TextProperty(contact.Phone)];

    return manager.Write(vCard, VCardVersion.V3_0);
}
```

## Export multiple contacts

```csharp
var contacts = _contactService.List();
var manager  = new VCardManager();

var cards = contacts.Select(c =>
{
    var v = new VCard();
    v.NameViews      = new NameProperty(c.LastName, c.FirstName);
    v.EmailAddresses = [new TextProperty(c.Email)];
    return v;
});

string vcf = manager.Write(cards, VCardVersion.V3_0);
```

## Import contacts from an uploaded .vcf file

```csharp
using var reader = new StreamReader(file.OpenReadStream());
string vcfContent = await reader.ReadToEndAsync();

var manager = new VCardManager();
return manager.ReadMany(vcfContent);
```
