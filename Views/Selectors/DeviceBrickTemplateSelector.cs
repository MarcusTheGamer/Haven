using Haven.Models;
using HavenDeviceInfo = Haven.Models.DeviceInfo;

namespace Haven.Views.Selectors;

/// <summary>
/// Picks which pre-designed brick DataTemplate to show for a device, purely
/// by looking up its brick key (DeviceBrickRegistry.BrickKeyFor) in the
/// Templates dictionary. Populate Templates from MainPage.xaml, one entry
/// per brick key — adding a new product's brick is one DataTemplate entry
/// there plus one line in DeviceBrickRegistry, never a change here.
/// </summary>
public class DeviceBrickTemplateSelector : DataTemplateSelector
{
    // ResourceDictionary, not Dictionary<string, DataTemplate>: MAUI's XAML
    // parser populates x:Key-keyed children by calling IDictionary.Add,
    // which Dictionary<TKey,TValue> only implements explicitly (invisible
    // to the reflection-based lookup XAML uses). ResourceDictionary exposes
    // a real public Add(string, object) and is the type MAUI itself uses
    // for this exact pattern (e.g. ContentPage.Resources).
    public ResourceDictionary Templates { get; } = new();

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var brickKey = DeviceBrickRegistry.BrickKeyFor((item as HavenDeviceInfo)?.Type);

        if (Templates.TryGetValue(brickKey, out var value) && value is DataTemplate template)
            return template;

        if (Templates.TryGetValue(DeviceBrickRegistry.FallbackBrickKey, out var fallbackValue) &&
            fallbackValue is DataTemplate fallback)
            return fallback;

        throw new InvalidOperationException(
            $"No brick template registered for key '{brickKey}', and no '{DeviceBrickRegistry.FallbackBrickKey}' fallback template either.");
    }
}