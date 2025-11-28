using PortLog.Enumerations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;

namespace PortLog.Models
{
    /// <summary>
    /// Class <c>CompanyId</c> represents a company entity with properties:.
    /// </summary>
    [Table("company")]
    public class Company: BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("total_fleet")]
        public short TotalFleet { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }

        [Column("join_code")]
        public string JoinCode { get; set; }

        [Column("provinsi")]
        public string Provinsi { get; set; }

        [JsonIgnore]
        public List<CompanyContact> Contacts { get; set; } = new List<CompanyContact>();
    }
}