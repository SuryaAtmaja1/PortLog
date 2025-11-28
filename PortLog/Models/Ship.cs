using PortLog.Enumerations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;

namespace PortLog.Models
{
    [Table("ship")]
    public class Ship : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        [Column("id", ignoreOnInsert:true)]
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

        [JsonIgnore]
        public ShipStatus StatusEnum
        {
            get => Enum.TryParse<ShipStatus>(Status, out var s) ? s : ShipStatus.STANDBY;
            set => Status = value.ToString();
        }
    }
}