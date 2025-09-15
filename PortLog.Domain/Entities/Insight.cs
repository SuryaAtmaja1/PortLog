namespace PortLog.Domain.Entities
{
    public class Insight
    {
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public int TotalTrips { get; private set; }
        public TimeSpan TotalHours { get; private set; }
        public float TotalDistance { get; private set; }
        public float AverageSpeed { get; private set; }
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