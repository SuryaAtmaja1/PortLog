using PortLog.Enumerations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace PortLog.Models
{
    [Table("account")]
    public class Account : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("account_role")]
        public string Role { get; set; }

        [Column("company_id")]
        public Guid CompanyId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }

        [Column("contact")]
        public string Contact { get; set; }

        // Helper property for type-safe access (not stored in DB)
        [JsonIgnore]
        public AccountRole RoleEnum
        {
            get => Enum.TryParse<AccountRole>(Role, out var role) ? role : AccountRole.CAPTAIN;
            set => Role = value.ToString();
        }
    }
}
