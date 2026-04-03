using PIM.Core.Constants;

namespace PIM.Core.Abstractions;

public interface IAppContext
{
    public PimAppTypes AppType { get; set; }
}