using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Haven.Models;

[Table("family_members")]
public class FamilyMember : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("family_id")]
    public long FamilyId { get; set; }

    [Column("role")]
    public string Role { get; set; } = "member";
}