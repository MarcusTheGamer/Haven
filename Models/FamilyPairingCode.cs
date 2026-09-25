using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Haven.Models;

[Table("family_pairing_codes")]
public class FamilyPairingCode : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("family_id")]
    public long FamilyId { get; set; }

    [Column("created_by")]
    public Guid CreatedBy { get; set; }

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }

    [Column("used_at")]
    public DateTimeOffset? UsedAt { get; set; }

    [Column("used_by")]
    public Guid? UsedBy { get; set; }
}