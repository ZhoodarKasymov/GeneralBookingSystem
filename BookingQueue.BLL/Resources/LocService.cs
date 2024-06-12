using System.Reflection;
using Microsoft.Extensions.Localization;

namespace BookingQueue.BLL.Resources;

public class LocService
{
    private readonly IStringLocalizer _localizer;

    public LocService(IStringLocalizerFactory factory)
    {
        var type = typeof(SharedResource);
        var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
        _localizer = factory.Create("SharedResource", assemblyName.Name);
    }

    public LocalizedString GetLocalizedString(string key)
    {
        return _localizer[key];
    }
}