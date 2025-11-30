using PortLog.Enumerations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace PortLog.Models
{
    [Table("company_contacts")]
    public class CompanyContact: BaseModel
    {
        [PrimaryKey("id")]
        [Column("id", ignoreOnInsert:true)]
        public long Id { get; set; }

        [Column("company_id")]
        public Guid CompanyId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("phone")]
        public string Phone { get; set; }
    }
}
