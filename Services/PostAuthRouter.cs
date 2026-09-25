using Haven.Models;

namespace Haven.Services;

public interface IPostAuthRouter
{
    Task RouteAsync();
}

public class PostAuthRouter : IPostAuthRouter
{
    private readonly IFamilyService _families;
    private readonly ICurrentFamilyService _currentFamily;

    public PostAuthRouter(IFamilyService families, ICurrentFamilyService currentFamily)
    {
        _families = families;
        _currentFamily = currentFamily;
    }

    public async Task RouteAsync()
    {
        Family? family = null;

        try
        {
            family = await _families.GetCurrentFamilyAsync();
        }
        catch
        {
            // family check failed
        }

        if (family != null)
        {
            _currentFamily.CurrentFamily = family;
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await Shell.Current.GoToAsync("//FamilySetupPage");
        }
    }
}