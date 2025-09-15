using PortLog.Domain.Enumerations;

namespace PortLog.Domain.Entities
{
    class Account
    {

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }
        public AccountRole AccountRole { get; private set; }
        public AccountStatus Status { get; set; }
        public string Company { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime LastUpdated { get; private set; }
        public DateTime LastLogin { get; private set; }

        public Account(string name, string email, string password, AccountRole role, string company)
        {
            Name = name;
            Email = email;
            Password = password;
            AccountRole = role;
            Company = company;
            DateTime now = DateTime.UtcNow;
            CreatedAt = now;
            LastUpdated = now;
            Status = AccountStatus.OFFLINE;
        }

        public Account(Guid id, string name, string email, string password, AccountRole role, string company, DateTime createdAt, DateTime lastUpdated, DateTime lastLogin)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            AccountRole = role;
            Company = company;
            CreatedAt = createdAt;
            LastUpdated = lastUpdated;
            LastLogin = lastLogin;
        }
        public void UpdatePassword(string newPassword)
        {
            Password = newPassword;
            LastUpdated = DateTime.UtcNow;
        }
    }
}
