using Haven.Models;

namespace Haven.Services;

public interface ICurrentFamilyService
{
    Family? CurrentFamily { get; set; }
    event Action? Changed;
}

public class CurrentFamilyService : ICurrentFamilyService
{
    private Family? _currentFamily;

    public Family? CurrentFamily
    {
        get => _currentFamily;
        set
        {
            _currentFamily = value;
            Changed?.Invoke();
        }
    }

    public event Action? Changed;
}