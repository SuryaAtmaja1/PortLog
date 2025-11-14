using PortLog.Enumerations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace PortLog.Models
{
    [Table("account")]
    public class Account : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("account_role")]
        public string AccountRoleString
        {
            get => AccountRole.ToString();
            set => AccountRole = Enum.Parse<AccountRole>(value);
        }

        public AccountRole AccountRole { get; set; }

        [Column("status")]
        public string StatusString
        {
            get => Status.ToString();
            set => Status = Enum.Parse<AccountStatus>(value);
        }

        public AccountStatus Status { get; set; }

        [Column("company")]
        public string Company { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }

        [Column("last_login")]
        public DateTime LastLogin { get; set; }

        public Account() { } 

        public Account(string name, string email, string password, AccountRole role, string company)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            Password = password;  
            AccountRole = role;
            Status = AccountStatus.OFFLINE;
            Company = company;

            var now = DateTime.UtcNow;
            CreatedAt = now;
            LastUpdated = now;
            LastLogin = now;
        }

        public void UpdatePassword(string newPassword)
        {
            Password = newPassword; 
            LastUpdated = DateTime.UtcNow;
        }
    }
}
