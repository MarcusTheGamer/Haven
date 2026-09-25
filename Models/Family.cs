using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Haven.Models;

[Table("families")]
public class Family : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("owner_id")]
    public Guid OwnerId { get; set; }
}