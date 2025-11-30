using PortLog.Enumerations;
using PortLog.Models;
using Supabase;
using Postgrest = Supabase.Postgrest;
using System.Diagnostics;
using PortLog.Supabase;
using PortLog.Helpers;

namespace PortLog.Services
{
    public class AccountService
    {
        private readonly SupabaseService _supabase;
        public Account? LoggedInAccount { get; private set; }

        public AccountService(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public async Task<(bool success, string error)> LoginAsync(
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Email atau password kosong!");

            try
            {
                // Fetch account by email
                var response = await _supabase
                    .Table<Account>()
                    .Where(a => a.Email == email)
                    .Get();
                var account = response.Models.FirstOrDefault();
                
                if (account == null)
                {
                    return (false, "Email atau password salah!");
                }

                // Verify password
                if (!PasswordHasher.Verify(password, account.Password))
                {
                    return (false, "Email atau password salah!");
                }

                LoggedInAccount = account;
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoginAsync] Exception occurred: {ex.GetType().Name}");
                Debug.WriteLine($"[LoginAsync] Exception message: {ex.Message}");
                Debug.WriteLine($"[LoginAsync] Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[LoginAsync] Inner exception: {ex.InnerException.Message}");
                }

                return (false, $"Login gagal: {ex.Message}");
            }
        }

        public async Task<(bool success, string error)> RegisterAsync(
            string name,
            string email,
            AccountRole role,
            string password)
        {
            // Password input validation
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password kosong!");
            if (password.Length < 8)
                return (false, "Password minimal 8 karakter!");

            try
            {
                // Email uniqueness check
                var check = await _supabase
                    .Table<Account>()
                    .Select("email")
                    .Filter("email", Postgrest.Constants.Operator.Equals, email)
                    .Get();

                if (check.Models.Any())
                    return (false, "Email telah dipakai!");

                // Hash password
                string hashedPassword = PasswordHasher.Hash(password);

                // Create new account with client-generated GUID
                var newAccount = new Account
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Password = hashedPassword,
                    Name = name,
                    Role = role.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                Debug.WriteLine($"[RegisterAsync] Creating account with ID: {newAccount.Id}");

                // Insert into database
                var result = await _supabase
                    .Table<Account>()
                    .Insert(newAccount);

                var insertedAccount = result.Models.FirstOrDefault();

                if (insertedAccount != null && insertedAccount.Id != Guid.Empty)
                {
                    LoggedInAccount = insertedAccount;
                    Debug.WriteLine($"[RegisterAsync] Registration successful. Account ID: {LoggedInAccount.Id}");
                    return (true, "");
                }
                else
                {
                    // Fallback: re-fetch by email
                    Debug.WriteLine("[RegisterAsync] Re-fetching account by email");
                    var fetchResult = await _supabase
                        .Table<Account>()
                        .Where(a => a.Email == email)
                        .Single();

                    if (fetchResult == null || fetchResult.Id == Guid.Empty)
                        return (false, "Registrasi berhasil tetapi gagal mengambil data");

                    LoggedInAccount = fetchResult;
                    Debug.WriteLine($"[RegisterAsync] Registration successful. Account ID: {LoggedInAccount.Id}");
                    return (true, "");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RegisterAsync] Exception: {ex.Message}");
                return (false, $"Register gagal: {ex.Message}");
            }
        }

        public async Task<bool> UpdateUserCompanyAsync(Guid companyId)
        {
            if (LoggedInAccount == null)
            {
                Debug.WriteLine("[UpdateUserCompanyAsync] No logged in account");
                return false;
            }

            try
            {
                Debug.WriteLine($"[UpdateUserCompanyAsync] Updating company ID to: {companyId} for user: {LoggedInAccount.Email}");

                LoggedInAccount.CompanyId = companyId;

                await _supabase
                    .Table<Account>()
                    .Update(LoggedInAccount);

                Debug.WriteLine("[UpdateUserCompanyAsync] Update successful");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateUserCompanyAsync] Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<Account?> GetCaptainByIdAsync(Guid captainId)
        {
            try
            {
                Debug.WriteLine($"[GetCaptainByIdAsync] Fetching captain with ID: {captainId}");
                var response = await _supabase
                    .Table<Account>()
                    .Where(a => a.Id == captainId && a.RoleEnum == AccountRole.CAPTAIN)
                    .Get();
                var captain = response.Models.First();
                if (captain != null)
                {
                    Debug.WriteLine($"[GetCaptainByIdAsync] Captain found: {captain.Name} ({captain.Email})");
                }
                else
                {
                    Debug.WriteLine("[GetCaptainByIdAsync] No captain found with the given ID");
                }
                return captain;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetCaptainByIdAsync] Exception: {ex.Message}");
                return null;

            }
        }

        public async Task<List<Account>> GetAccountsByCompanyIdAsync(Guid companyId)
        {
            var accounts = new List<Account>();
            try
            {
                Debug.WriteLine($"[GetAccountsByCompanyIdAsync] Fetching accounts for company ID: {companyId}");
                var response = await _supabase
                    .Table<Account>()
                    .Filter("company_id", Postgrest.Constants.Operator.Equals, companyId.ToString())
                    .Get();
                accounts = response.Models.ToList();
                Debug.WriteLine($"[GetAccountsByCompanyIdAsync] Found {accounts.Count} accounts");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetAccountsByCompanyIdAsync] Exception: {ex.Message}");
            }
            return accounts;
        }

        public async Task<List<Account>> GetCaptainsByCompanyIdAsync(Guid companyId)
        {
            var captains = new List<Account>();
            try
            {
                Debug.WriteLine($"[GetCaptainsByCompanyIdAsync] Fetching captains for company ID: {companyId}");
                var response = await _supabase
                    .Table<Account>()
                    .Filter("company_id", Postgrest.Constants.Operator.Equals, companyId.ToString())
                    .Filter("account_role", Postgrest.Constants.Operator.Equals, "CAPTAIN")
                    .Get();
                captains = response.Models.ToList();
                Debug.WriteLine($"[GetCaptainsByCompanyIdAsync] Found {captains.Count} captains");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetCaptainsByCompanyIdAsync] Exception: {ex.Message}");
            }
            return captains;
        }

        public async Task<List<Account>> GetAccountsWithoutCompanyAsync()
        {
            var accounts = new List<Account>();
            try
            {
                Debug.WriteLine("[GetAccountNoCompanyAsync] Fetching accounts with no company");
                var response = await _supabase
                    .Table<Account>()
                    .Filter("company_id", Postgrest.Constants.Operator.Is, (Guid?)null)
                    .Get();
                accounts = response.Models.ToList();
                Debug.WriteLine($"[GetAccountNoCompanyAsync] Found {accounts.Count} accounts with no company");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetAccountNoCompanyAsync] Exception: {ex.Message}");
            }
            return accounts;
        }
        public void Logout()
        {
            LoggedInAccount = null;
        }
    }
}