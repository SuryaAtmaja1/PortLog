using PortLog.Enumerations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;

namespace PortLog.Models
{
    [Table("insights")]
    public class Insight
    {
        [Column("start_date")]
        public DateTime StartDate { get; private set; }
        [Column("end_date")]
        public DateTime EndDate { get; private set; }
        [Column("total_trips")]
        public int TotalTrips { get; private set; }
        [Column("total_hours")]
        public TimeSpan TotalHours { get; private set; }
        [Column("total_distance")]
        public float TotalDistance { get; private set; }
        [Column("average_speed")]
        public float AverageSpeed { get; private set; }
        [Column("average_fuel_consumption")]
        public float AverageFuelConsumption { get; private set; }

        /// <summary>
        /// Constructor <c>Insight</c> for immediate instantiation.
        /// </summary>
        public Insight(DateTime startDate, DateTime endDate, int totalTrips, TimeSpan totalHours, float totalDistance, float averageSpeed, float averageFuelConsumption)
        {
            if (endDate < startDate) throw new ArgumentException("End date cannot be earlier than start date");
            StartDate = startDate;
            EndDate = endDate;
            TotalTrips = totalTrips;
            TotalHours = totalHours;
            TotalDistance = totalDistance;
            AverageSpeed = averageSpeed;
            AverageFuelConsumption = averageFuelConsumption;
        }
    }
}