using PortLog.Enumerations;

namespace PortLog.Models
{
    public class TelemetryLog
    {
        public Guid Id { get; private set; }
        public Guid ShipId { get; private set; }
        public Guid LoggerId { get; private set; }
        public DateTime Timestamp { get; private set; }
        public float PositionLat { get; private set; }
        public float PositionLong { get; private set; }
        public float Speed { get; private set; }
        public float Heading { get; private set; }
        public float DistanceFromLastPosition { get; private set; }
        public Weather Weather { get; private set; }
        public EngineStatus EngineStatus { get; private set; }
        public float FuelConsumption { get; private set; }
        public string Notes { get; private set; }

        public TelemetryLog(Guid shipId, Guid loggerId, float lat, float lon, float speed, float heading, Weather weather, EngineStatus engineStatus, float fuelConsumption, string notes)
        {
            ShipId = shipId;
            LoggerId = loggerId;
            PositionLat = lat;
            PositionLong = lon;
            Speed = speed;
            Heading = heading;
            Weather = weather;
            EngineStatus = engineStatus;
            FuelConsumption = fuelConsumption;
            Notes = notes;
            Timestamp = DateTime.UtcNow;
        }
        public TelemetryLog(Guid id, Guid shipId, Guid loggerId, DateTime timestamp, float lat, float lon, float speed, float heading, float distanceFromLastPosition, Weather weather, EngineStatus engineStatus, float fuelConsumption, string notes)
        {
            Id = id;
            ShipId = shipId;
            LoggerId = loggerId;
            Timestamp = timestamp;
            PositionLat = lat;
            PositionLong = lon;
            Speed = speed;
            Heading = heading;
            DistanceFromLastPosition = distanceFromLastPosition;
            Weather = weather;
            EngineStatus = engineStatus;
            FuelConsumption = fuelConsumption;
            Notes = notes;
        }

        static private double DegreesToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }
        public float HaversineDistance(float lat1, float long1, float lat2, float long2)
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

        public float DistanceTo(TelemetryLog otherLog)
        {
            return HaversineDistance(PositionLat, PositionLong, otherLog.PositionLat, otherLog.PositionLong);
        }

    }
}