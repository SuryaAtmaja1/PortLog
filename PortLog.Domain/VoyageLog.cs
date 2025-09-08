
namespace PortLog.Domain;

public class VoyageLog
{
    private Guid _id;
    private Guid[] _telemetryLogIds;
    private DateTime _departureTime;
    private DateTime _arrivalTime;
    private string _departurePort;
    private string _arrivalPort;
    private TimeSpan _tripTime;
    private float _totalDistanceTraveled;
    private float _averageFuelConsumption;
    private float _averageSpeed;
    private string _notes;

    public Guid Id
    {
        get => _id;
    }
    public Guid[] TelemetryLogIds
    {
        get => _telemetryLogIds;
    }
    public DateTime DepartureTime
    {
        get => _departureTime;
        set => _departureTime = value;
    }
    public DateTime ArrivalTime
    {
        get => _arrivalTime;
        set => _arrivalTime = value;
    }
    public string DeparturePort
    {
        get => _departurePort;
        set => _departurePort = value;
    }
    public string ArrivalPort
    {
        get => _arrivalPort;
        set => _arrivalPort = value;
    }
    public TimeSpan TripTime
    {
        get => _tripTime;
        set => _tripTime = _arrivalTime - _departureTime;
    }
    public float TotalDistanceTraveled
    {
        get => _totalDistanceTraveled;
    }
    public float AverageFuelConsumption
    {
        get => _averageFuelConsumption;
    }
    public float AverageSpeed
    {
        get => _averageSpeed;
    }
    public string Notes
    {
        get => _notes;
        set => _notes = value;
    }
}

