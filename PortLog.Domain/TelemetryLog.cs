using System;

namespace PortLog.Domain
{
    public class TelemetryLog
    {
        public Guid Id { get; set; }
        public Guid ShipId { get; set; }
        public Guid LoggerId { get; set; }
        public float PositionLat { get; set; }
        public float PositionLong { get; set; }
        public float Speed { get; set; }
        public float Heading { get; set; }
        public float DistanceFromLastPosition { get; set; }
        public Weather Weather { get; set; }
        public EngineStatus EngineStatus { get; set; }
        public float FuelConsumption { get; set; }
        public string Notes { get; set; }

        public float DistanceTo(float lat1, float long1, float lat2, float long2)
        {
            // Haversine formula for distance between two lat/long points (in kilometers)
            double R = 6371; // Radius of Earth in km
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(long2 - long1);
            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return (float)(R * c);
        }

        private double DegreesToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}