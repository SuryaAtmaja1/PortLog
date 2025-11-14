namespace PortLog.ValueObjects
{
    public class ShipRegistrationId
    {
        public string Organization { get; }
        public string RegistrationType { get; }
        public string RegistrationNumber { get; }

        public ShipRegistrationId(string organization, string type, string number)
        {
            if (string.IsNullOrWhiteSpace(organization))
                throw new ArgumentException("Organization cannot be empty", nameof(organization));

            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Registration type cannot be empty", nameof(type));

            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Registration number cannot be empty", nameof(number));

            Organization = organization;
            RegistrationType = type;
            RegistrationNumber = number;
        }

        public override string ToString() => $"{Organization}:{RegistrationType}-{RegistrationNumber}";

        public override bool Equals(object? obj)
        {
            if (obj is not ShipRegistrationId other) return false;
            return Organization == other.Organization &&
                   RegistrationType == other.RegistrationType &&
                   RegistrationNumber == other.RegistrationNumber;
        }

        public override int GetHashCode() =>
            HashCode.Combine(Organization, RegistrationType, RegistrationNumber);
    }
}