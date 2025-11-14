using PortLog.Enumerations;
using PortLog.ValueObjects;

namespace PortLog.Models
{
    public class Ship
    {
        public Guid Id { get; private set; }
        private readonly List<ShipRegistrationId> _registeredIds = [];
        public IReadOnlyCollection<ShipRegistrationId> RegisteredIds => _registeredIds.AsReadOnly();
        public Guid CompanyId { get; private set; }
        public string Name { get; private set; }
        public ShipType Type { get; private set; }
        public Guid CaptainId { get; private set; }
        public int PassengerCapacity { get; private set; }
        public ShipStatus Status { get; private set; }
        private readonly List<TelemetryLog> _telemetryLogs = [];
        public IReadOnlyCollection<TelemetryLog> TelemetryLogs => _telemetryLogs.AsReadOnly();

        public Ship(Guid companyId, string name, ShipType type, Guid captainId)
        {
            CompanyId = companyId;
            Name = name;
            Type = type;
            CaptainId = captainId;
            Status = ShipStatus.STANDBY;
        }

        // Behavior: Tambah registrasi baru
        public void AddRegistration(ShipRegistrationId registration)
        {
            if (_registeredIds.Contains(registration))
                throw new InvalidOperationException("Registration already exists for this ship.");

            _registeredIds.Add(registration);
        }

        // Behavior: Hapus registrasi
        public void RemoveRegistration(ShipRegistrationId registration)
        {
            _registeredIds.Remove(registration);
        }

        // Behavior: Cari registrasi berdasarkan organisasi
        public ShipRegistrationId? GetRegistrationByOrg(string organization)
        {
            return _registeredIds.FirstOrDefault(r => r.Organization == organization);
        }
    }
}