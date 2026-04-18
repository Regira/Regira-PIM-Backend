using PIM.Core.Extensions;
using PIM.Data;
using PIM.Models.Stakeholders.Parties;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.EFcore.Preppers.Abstractions;

namespace PIM.Services.Entities.Stakeholders.Parties;

public class PartyPrepper(PimDbContext dbContext) : EntityPrepperBase<Party>
{
    public override Task Prepare(Party modified, Party? original)
    {
        modified.ChildRelationships?.PrepareContactData(dbContext, original?.ChildRelationships);
        modified.ParentRelationships?.PrepareContactData(dbContext, original?.ParentRelationships);

        return Task.CompletedTask;
    }
}

public static class PartyRelationPrepperExtensions
{
    public static void PrepareContactData(this ICollection<PartyRelationship> relationships, PimDbContext dbContext, ICollection<PartyRelationship>? originalRelationships)
    {
        foreach (var relationship in relationships)
        {
            var originalRelationship = originalRelationships?.FirstOrDefault(cr => cr.Id == relationship.Id);
            if (originalRelationship != null && relationship.ContactData != null)
            {
                relationship.ContactData.Prepare();
                dbContext.UpdateRelatedCollection(relationship, originalRelationship, r => r.ContactData);
            }
        }
    }
}