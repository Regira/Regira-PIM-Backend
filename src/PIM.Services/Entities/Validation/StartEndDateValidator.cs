using PIM.Core.Constants;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Validators.Abstractions;

namespace PIM.Services.Entities.Validation;

/// <summary>
/// An end date cannot come before the start date — for every entity with both, whichever service saves it
/// (registered app-wide: a validator applies to every entity its scope type covers).
/// </summary>
public class StartEndDateValidator : EntityValidatorBase<IHasStartEndDate>
{
    public override Task Validate(IEntityValidatorContext<IHasStartEndDate> context, CancellationToken token = default)
    {
        if (context.Operation != EntityWriteOperation.Remove && IsEndBeforeStart(context.Item))
        {
            context.AddError(nameof(IHasStartEndDate.EndDate), ValidationMessages.EndBeforeStart);
        }
        return Task.CompletedTask;
    }

    public static bool IsEndBeforeStart(IHasStartEndDate item)
        => item.StartDate.HasValue && item.EndDate.HasValue && item.EndDate.Value < item.StartDate.Value;
}
