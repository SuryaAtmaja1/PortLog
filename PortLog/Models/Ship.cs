using PortLog.Enumerations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace PortLog.Models
{
    public class Ship : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public long Id { get; set; }

        [Column("company_id")]
        public Guid CompanyId { get; set; }

        [Column("captain_id")]
        public Guid CaptainId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("passenger_capacity")]
        public int PassengerCapacity { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [JsonIgnore]
        public ShipType ShipTypeEnum
        {
            get => Enum.TryParse<ShipType>(Type, out var type) ? type : ShipType.MULTI_PURPOSE_VESSEL;
            set => Type = value.ToString();
        }
    }
}