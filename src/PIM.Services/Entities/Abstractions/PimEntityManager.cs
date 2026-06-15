using FluentValidation;
using PIM.Services.Entities.Validation;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Services.Abstractions;

namespace PIM.Services.Entities.Abstractions;

public abstract class PimEntityManager<TEntity, TSearchObject, TSortBy, TIncludes>(
    IEntityService<TEntity, int, TSearchObject, TSortBy, TIncludes> service, IValidator<TEntity>? validator = null)
    : EntityWrappingServiceBase<TEntity, TSearchObject, TSortBy, TIncludes>(service)
    where TEntity : class, IEntity<int>
    where TSearchObject : class, ISearchObject<int>, new()
    where TSortBy : struct, Enum
    where TIncludes : struct, Enum
{
    public override Task Add(TEntity item, CancellationToken token = default)
    {
        Validate(item);
        return base.Add(item, token);
    }

    public override Task<TEntity?> Modify(TEntity item, CancellationToken token = default)
    {
        Validate(item);
        return base.Modify(item, token);
    }

    public override Task Save(TEntity item, CancellationToken token = default)
    {
        Validate(item);
        return base.Save(item, token);
    }

    public void Validate(TEntity item)
    {
        if (validator == null)
            return;

        var result = validator.Validate(item);
        if (!result.IsValid)
        {
            throw new EntityInputException<TEntity>("Validation failed")
            {
                InputErrors = result.ToInputErrors(),
                Item = item
            };
        }
    }
}
