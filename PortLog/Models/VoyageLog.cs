using PortLog.Enumerations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace PortLog.Models
{
    public class VoyageLog : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public long Id { get; set; }

        [Column("ship_id")]
        public long ShipId { get; set; }

        [Column("departure_time")]
        public DateTime DepartureTime { get; set; }

        [Column("arrival_time")]
        public DateTime ArrivalTime { get; set; }

        [Column("departure_port")]
        public string DeparturePort { get; set;; }

        [Column("arrival_port")]
        public string ArrivalPort { get; set; }

        [Column("notes")]
        public string Notes { get; set; }

        [Column("total_distance_traveled")]
        public double TotalDistanceTraveled { get; set; }

        [Column("average_fuel_consumption")]
        public double AverageFuelConsumption { get; set; }

        [Column("revenue_idr_cents")]
        public long Revenue { get; set; }

        [JsonIgnore]
        public decimal RevenueIdr
        {
            get => Revenue / 100m;
            set => Revenue = (long)(value * 100);
        }
    }
}

