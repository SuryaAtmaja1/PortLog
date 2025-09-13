namespace PortLog.Domain.Entities
{
    public class VoyageLog
    {
        private readonly List<TelemetryLog> _telemetryLogs = new();
        private float _totalDistanceTraveled;
        private float _averageFuelConsumption;
        private float _averageSpeed;

        public Guid Id { get; }
        public DateTime DepartureTime { get; private set; }
        public DateTime? ArrivalTime { get; private set; }
        public string DeparturePort { get; private set; }
        public string? ArrivalPort { get; private set; }
        public string? Notes { get; private set; }

        public IReadOnlyCollection<TelemetryLog> TelemetryLogs => _telemetryLogs.AsReadOnly();
        public TimeSpan? TripTime => ArrivalTime.HasValue ? ArrivalTime - DepartureTime : null;
        public float TotalDistanceTraveled => _totalDistanceTraveled;
        public float AverageFuelConsumption => _averageFuelConsumption;
        public float AverageSpeed => _averageSpeed;

        private VoyageLog(Guid id, DateTime departureTime, string departurePort, string? notes = null)
        {
            Id = id;
            DepartureTime = departureTime;
            DeparturePort = departurePort;
            Notes = notes;
        }

        public static VoyageLog Start(Guid id, DateTime departureTime, string departurePort, string? notes = null)
        {
            return new VoyageLog(id, departureTime, departurePort, notes);
        }

        public void AddTelemetryLog(TelemetryLog log)
        {
            if (ArrivalTime != null)
                throw new InvalidOperationException("Voyage already completed.");

            if (log.ShipId == Guid.Empty)
                throw new ArgumentException("Telemetry log must be linked to a ship.");

            if (_telemetryLogs.Any())
            {
                var lastLog = _telemetryLogs.Last();
                var distance = log.HaversineDistance(lastLog.PositionLat, lastLog.PositionLong,
                                            log.PositionLat, log.PositionLong);
                _totalDistanceTraveled += distance;
            }

            _telemetryLogs.Add(log);
            _averageSpeed = _telemetryLogs.Average(l => l.Speed);
            _averageFuelConsumption = _telemetryLogs.Average(l => l.FuelConsumption);
        }

        public void CompleteVoyage(DateTime arrivalTime, string arrivalPort)
        {
            if (arrivalTime <= DepartureTime)
                throw new ArgumentException("Arrival time must be after departure time.");

            ArrivalTime = arrivalTime;
            ArrivalPort = arrivalPort;
        }
    }
}

