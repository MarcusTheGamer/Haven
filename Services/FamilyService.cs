using System.Security.Cryptography;
using Haven.Models;

namespace Haven.Services;

public class FamilyService : IFamilyService
{
    private readonly SupabaseService _supabase;

    public FamilyService(SupabaseService supabase) => _supabase = supabase;

    private Guid CurrentUserId => Guid.Parse(_supabase.Client.Auth.CurrentUser?.Id ?? throw new InvalidOperationException("Not signed in."));

    public async Task<Family?> GetCurrentFamilyAsync()
    {
        var userId = _supabase.Client.Auth.CurrentUser?.Id;
        if (string.IsNullOrEmpty(userId)) return null;

        var membership = (await _supabase.Client
            .From<FamilyMember>()
            .Where(x => x.UserId == Guid.Parse(userId))
            .Limit(1)
            .Get()).Models.FirstOrDefault();

        if (membership == null) return null;

        return await _supabase.Client
            .From<Family>()
            .Where(x => x.Id == membership.FamilyId)
            .Single();
    }

    public async Task<List<Family>> GetMyFamiliesAsync()
    {
        var userId = _supabase.Client.Auth.CurrentUser?.Id;
        if (string.IsNullOrEmpty(userId)) return new List<Family>();

        var memberships = (await _supabase.Client
            .From<FamilyMember>()
            .Where(x => x.UserId == Guid.Parse(userId))
            .Get()).Models;

        var families = new List<Family>();

        foreach (var familyId in memberships.Select(m => m.FamilyId).Distinct())
        {
            var family = await _supabase.Client
                .From<Family>()
                .Where(x => x.Id == familyId)
                .Single();

            if (family != null) families.Add(family);
        }

        return families;
    }

    public async Task<Family> CreateFamilyAsync(string name)
    {
        var ownerId = CurrentUserId;

        var family = (await _supabase.Client
            .From<Family>()
            .Insert(new Family { Name = name, OwnerId = ownerId }))
            .Models.First();

        await _supabase.Client
            .From<FamilyMember>()
            .Insert(new FamilyMember { UserId = ownerId, FamilyId = family.Id, Role = "owner" });

        return family;
    }

    public async Task<string> GeneratePairingCodeAsync(long familyId, TimeSpan? validFor = null)
    {
        var code = GenerateCode();

        await _supabase.Client
            .From<FamilyPairingCode>()
            .Insert(new FamilyPairingCode
            {
                FamilyId = familyId,
                CreatedBy = CurrentUserId,
                Code = code,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.Add(validFor ?? TimeSpan.FromMinutes(10))
            });

        return code;
    }

    public async Task<Family?> RedeemPairingCodeAsync(string code)
    {
        var trimmed = code.Trim().ToUpperInvariant();

        var pairing = (await _supabase.Client
            .From<FamilyPairingCode>()
            .Where(x => x.Code == trimmed)
            .Limit(1)
            .Get()).Models.FirstOrDefault();

        if (pairing == null || pairing.UsedAt != null || pairing.ExpiresAt < DateTimeOffset.UtcNow)
            return null;

        var joiningUserId = CurrentUserId;

        await _supabase.Client
            .From<FamilyMember>()
            .Insert(new FamilyMember { UserId = joiningUserId, FamilyId = pairing.FamilyId, Role = "member" });

        pairing.UsedAt = DateTimeOffset.UtcNow;
        pairing.UsedBy = joiningUserId;

        await _supabase.Client
            .From<FamilyPairingCode>()
            .Where(x => x.Id == pairing.Id)
            .Update(pairing);

        return await _supabase.Client
            .From<Family>()
            .Where(x => x.Id == pairing.FamilyId)
            .Single();
    }

    public async Task RenameFamilyAsync(long familyId, string newName)
    {
        var family = await _supabase.Client
            .From<Family>()
            .Where(x => x.Id == familyId)
            .Single();

        if (family == null) return;

        family.Name = newName;

        await _supabase.Client
            .From<Family>()
            .Where(x => x.Id == familyId)
            .Update(family);
    }

    public async Task LeaveFamilyAsync(long familyId)
    {
        var userId = CurrentUserId;

        await _supabase.Client
            .From<FamilyMember>()
            .Where(x => x.FamilyId == familyId)
            .Where(x => x.UserId == userId)
            .Delete();
    }

    // generate a short code
    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var buffer = new char[6];

        for (var i = 0; i < buffer.Length; i++)
            buffer[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];

        return new string(buffer);
    }
}