using Haven.Models;

namespace Haven.Services;

public interface IFamilyService
{
    Task<Family?> GetCurrentFamilyAsync();
    Task<List<Family>> GetMyFamiliesAsync();
    Task<Family> CreateFamilyAsync(string name);
    Task<string> GeneratePairingCodeAsync(long familyId, TimeSpan? validFor = null);
    Task<Family?> RedeemPairingCodeAsync(string code);
    Task RenameFamilyAsync(long familyId, string newName);
    Task LeaveFamilyAsync(long familyId);
}