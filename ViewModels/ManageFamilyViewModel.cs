using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Haven.Models;
using Haven.Services;

namespace Haven.ViewModels;

public partial class ManageFamilyViewModel : ObservableObject
{
    private readonly IFamilyService _families;

    [ObservableProperty] private bool isLoading;

    public ObservableCollection<Family> Families { get; } = new();

    public ManageFamilyViewModel(IFamilyService families) => _families = families;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsLoading) return;

        IsLoading = true;

        try
        {
            Families.Clear();

            foreach (var family in await _families.GetMyFamiliesAsync())
                Families.Add(family);
        }
        catch
        {
            // family loading failed
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RenameAsync(Family family, string newName)
    {
        await _families.RenameFamilyAsync(family.Id, newName);
        family.Name = newName;
    }

    public async Task LeaveAsync(Family family)
    {
        await _families.LeaveFamilyAsync(family.Id);
        Families.Remove(family);
    }
}