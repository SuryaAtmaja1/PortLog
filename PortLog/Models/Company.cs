namespace PortLog.Models
{
    /// <summary>
    /// Class <c>Company</c> represents a company entity with properties:.
    /// </summary>
    class Company
    {
        private readonly List<string> _emails = new();
        private readonly List<string> _contacts = new();
        // Getters and Setters
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public IReadOnlyList<string> Emails => _emails.AsReadOnly();
        public IReadOnlyList<string> Contacts => _contacts.AsReadOnly();
        public int TotalFleet { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Constructor <c>Company</c> for immediate instantiation.
        /// </summary>
        public Company(string name, string address, string[] emails, string[] contacts)
        {
            Name = name;
            Address = address;
            if (emails != null) _emails.AddRange(emails);
            if (contacts != null) _contacts.AddRange(contacts);
            TotalFleet = 0;

            DateTime now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;
        }

        /// <summary>
        /// Constructor <c>Company</c> for database instantiation.
        /// </summary>
        public Company(Guid id, string name, string address, string[] emails, string[] contacts, int totalFleet, DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            Name = name;
            Address = address;
            if (emails != null) _emails.AddRange(emails);
            if (contacts != null) _contacts.AddRange(contacts);
            TotalFleet = totalFleet;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public void AddShip(Ship ship)
        {
            ArgumentNullException.ThrowIfNull(ship);
            if (ship.CompanyId != Id) throw new InvalidOperationException($"Ship {ship.Id}: {ship.Name} does not belong to this company.");
            TotalFleet++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveShip(Ship ship)
        {
            ArgumentNullException.ThrowIfNull(ship);
            if (ship.CompanyId != Id) throw new InvalidOperationException($"Ship {ship.Id}: {ship.Name} does not belong to this company.");
            if (TotalFleet > 0) TotalFleet--;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateInfo(string name, string address)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address)) throw new ArgumentException($"Name or address field is empty");
            Name = name;
            Address = address;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException($"Invalid email input");
            _emails.Add(email);
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveEmail(string email)
        {
            if (_emails.Remove(email)) UpdatedAt = DateTime.UtcNow;
        }

        public void AddContact(string contact)
        {
            if (string.IsNullOrWhiteSpace(contact)) throw new ArgumentException($"Invalid contact input");
            _contacts.Add(contact);
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveContact(string contact)
        {
            if (_contacts.Remove(contact)) UpdatedAt = DateTime.UtcNow;
        }
    }
}