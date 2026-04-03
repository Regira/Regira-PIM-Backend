using PIM.Core.Abstractions;
using PIM.Core.Constants;

namespace PIM.Web.Models;

public class PimAppContext : IAppContext
{
    public PimAppTypes AppType { get; set; }
}