using PortLog.Enumerations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;

namespace PortLog.Models
{
    [Table("voyage_log")]
    public class VoyageLog : BaseModel
    {
        [PrimaryKey("id", shouldInsert: false)]
        [Column("id", ignoreOnInsert: true)]
        public long Id { get; set; }

        [Column("ship_id")]
        public long ShipId { get; set; }

        [Column("departure_time")]
        public DateTime DepartureTime { get; set; }

        [Column("arrival_time")]
        public DateTime? ArrivalTime { get; set; }

        [Column("departure_port")]
        public string DeparturePort { get; set; }

        [Column("arrival_port")]
        public string? ArrivalPort { get; set; }

        [Column("notes")]
        public string Notes { get; set; }

        [Column("total_distance_traveled")]
        public double? TotalDistanceTraveled { get; set; }

        [Column("average_fuel_consumption")]
        public double? AverageFuelConsumption { get; set; }

        [Column("revenue_idr_cents")]
        public long? Revenue { get; set; }

        [JsonIgnore]
        public decimal RevenueIdr
        {
            get => (Revenue ?? 0) / 100m;
            set => Revenue = (long)(value * 100);
        }
    }
}

