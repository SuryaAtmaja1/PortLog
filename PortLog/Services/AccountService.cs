using PortLog.Enumerations;
using PortLog.Models;
using Supabase;

namespace PortLog.Services
{
    public class AccountService
    {
        private Client Client => SupabaseClient.Instance;

        public Account? LoggedInAccount { get; private set; }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var query = await Client
                .From<Account>()
                .Where(a => a.Email == email)
                .Get();

            var acc = query.Models.FirstOrDefault();

            if (acc == null)
                return false;

            if (acc.Password != password)
                return false;

            LoggedInAccount = acc;

            return true;
        }

        public async Task<(bool success, string error)> RegisterAsync(
            string name,
            string email,
            AccountRole role,
            string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "Password kosong!");
            }

            try
            {
                // Check if email already exists
                var existingUser = await Client
                    .From<Account>()
                    .Where(a => a.Email == email)
                    .Get();

                if (existingUser.Models.Any())
                {
                    return (false, "Email telah dipakai!");
                }

                // Create new account
                var newAccount = new Account
                {
                    Email = email,
                    Password = password,
                    Name = name,
                    Role = role.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                var response = await Client
                    .From<Account>()
                    .Insert(newAccount);

                var createdAccount = response.Models.FirstOrDefault();

                if (createdAccount != null)
                {
                    LoggedInAccount = createdAccount;
                    return (true, string.Empty);
                }

                return (false, "Gagal membuat akun!");
            }
            catch (Exception ex)
            {
                return (false, $"Register gagal: {ex.Message}");
            }
        }
        public async Task<bool> UpdateUserCompanyAsync(
            Guid companyId
            )
        {
            if (LoggedInAccount == null) return false;

            try
            {
                LoggedInAccount.CompanyId = companyId;

                await Client
                    .From<Account>()
                    .Where(a => a.Id == LoggedInAccount.Id)
                    .Set(a => a.CompanyId, companyId)
                    .Update();

                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Logout()
        {
            LoggedInAccount = null;
        }
    }
}
