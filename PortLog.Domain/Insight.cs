using System;

namespace PortLog.Domain
{
    public class Insight
    {
        public DateRange TimePeriod { get; set; }
        public int TotalTrips { get; set; }
        public DateTime TotalHours { get; set; }
        public float TotalMileage { get; set; }
        public float AverageSpeed { get; set; }
        public float AverageFuelConsumption { get; set; }

        public void SaveToDatabase(Insight data)
        {
            // Implement database save logic here
        }

        public static Insight Generate(Guid voyageId)
        {
            // Implement insight generation logic here
            return new Insight();
        }

        public void Export(string format)
        {
            // Implement export logic here (e.g., CSV, JSON)
        }
    }
}