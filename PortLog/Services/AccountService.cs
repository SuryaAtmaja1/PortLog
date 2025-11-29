using PortLog.Enumerations;
using PortLog.Models;
using Supabase;
using Postgrest = Supabase.Postgrest;
using System.Diagnostics;
using PortLog.Supabase;

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
                Debug.WriteLine($"[LoginAsync] Attempting login for email: {email}");
                Debug.WriteLine($"[LoginAsync] Password length: {password.Length}");
                Debug.WriteLine($"[LoginAsync] Account table name: {_supabase.Table<Account>().TableName}");

                // Try to get ALL accounts first (to verify table access)
                Debug.WriteLine("[LoginAsync] Attempting to fetch all accounts (limit 5)...");
                var allAccounts = await _supabase
                    .Table<Account>()
                    .Limit(5)
                    .Get();

                Debug.WriteLine($"[LoginAsync] Total accounts in table: {allAccounts.Models?.Count ?? 0}");

                if (allAccounts.Models != null && allAccounts.Models.Any())
                {
                    Debug.WriteLine("[LoginAsync] Sample accounts found:");
                    foreach (var sample in allAccounts.Models)
                    {
                        Debug.WriteLine($"  - Email: '{sample.Email}', Name: '{sample.Name}', Role: '{sample.Role}'");
                    }
                }
                else
                {
                    Debug.WriteLine("[LoginAsync] WARNING: No accounts found in table at all!");
                    Debug.WriteLine("[LoginAsync] This suggests either:");
                    Debug.WriteLine("  1. The table is empty");
                    Debug.WriteLine("  2. The table name is wrong");
                    Debug.WriteLine("  3. The Account model mapping is incorrect");
                }

                // First, let's check if the email exists at all
                Debug.WriteLine($"[LoginAsync] Now searching for specific email: {email}");
                var emailCheck = await _supabase
                    .Table<Account>()
                    .Where(a => a.Email == email)
                    .Get();

                Debug.WriteLine($"[LoginAsync] Email check - Found {emailCheck.Models?.Count ?? 0} accounts with this email");

                if (emailCheck.Models != null && emailCheck.Models.Any())
                {
                    var foundAccount = emailCheck.Models.First();
                    Debug.WriteLine($"[LoginAsync] Account details:");
                    Debug.WriteLine($"  - Email: '{foundAccount.Email}'");
                    Debug.WriteLine($"  - Name: '{foundAccount.Name}'");
                    Debug.WriteLine($"  - Role: '{foundAccount.Role}'");
                    Debug.WriteLine($"  - Password length in DB: {foundAccount.Password?.Length ?? 0}");
                    Debug.WriteLine($"  - Password match: {foundAccount.Password == password}");

                    // Check for whitespace issues
                    if (foundAccount.Password != password)
                    {
                        Debug.WriteLine($"  - DB Password (with quotes): '{foundAccount.Password}'");
                        Debug.WriteLine($"  - Input Password (with quotes): '{password}'");
                        Debug.WriteLine($"  - DB Password trimmed match: {foundAccount.Password?.Trim() == password.Trim()}");
                    }
                }
                else
                {
                    Debug.WriteLine("[LoginAsync] Email not found in database at all!");
                }

                // Now try the original query with both email and password
                var response = await _supabase
                    .Table<Account>()
                    .Where(a => a.Email == email && a.Password == password)
                    .Get();

                Debug.WriteLine($"[LoginAsync] Full query executed. Response status: {response.ResponseMessage?.StatusCode}");
                Debug.WriteLine($"[LoginAsync] Number of models returned: {response.Models?.Count ?? 0}");

                var acc = response.Models.FirstOrDefault();

                if (acc == null)
                {
                    Debug.WriteLine("[LoginAsync] No account found matching both email AND password");
                    return (false, "Email atau password salah!");
                }

                LoggedInAccount = acc;
                Debug.WriteLine($"[LoginAsync] Login successful for: {acc.Email}");
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
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password kosong!");
            if (password.Length < 8)
                return (false, "Password minimal 8 karakter!");

            try
            {
                Debug.WriteLine($"[RegisterAsync] Checking email uniqueness for: {email}");

                // Check email uniqueness
                var check = await _supabase
                    .Table<Account>()
                    .Select("email")
                    .Filter("email", Postgrest.Constants.Operator.Equals, email)
                    .Get();

                Debug.WriteLine($"[RegisterAsync] Email check returned {check.Models?.Count ?? 0} results");

                if (check.Models.Any())
                {
                    Debug.WriteLine("[RegisterAsync] Email already exists");
                    return (false, "Email telah dipakai!");
                }

                // Create new account
                var newAccount = new Account
                {
                    Email = email,
                    Password = password,
                    Name = name,
                    Role = role.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                Debug.WriteLine($"[RegisterAsync] Inserting new account: {email}, Role: {role}");

                var result = await _supabase
                    .Table<Account>()
                    .Insert(newAccount);

                Debug.WriteLine($"[RegisterAsync] Insert completed. Models count: {result.Models?.Count ?? 0}");

                LoggedInAccount = result.Models.FirstOrDefault();

                if (LoggedInAccount != null)
                {
                    Debug.WriteLine($"[RegisterAsync] Registration successful for: {LoggedInAccount.Email}");
                }

                return (true, "");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RegisterAsync] Exception occurred: {ex.GetType().Name}");
                Debug.WriteLine($"[RegisterAsync] Exception message: {ex.Message}");
                Debug.WriteLine($"[RegisterAsync] Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[RegisterAsync] Inner exception: {ex.InnerException.Message}");
                }

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
                    .Where(a => a.Id == captainId)
                    .Get();
                var captain = response.Models.FirstOrDefault();
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
            Debug.WriteLine($"[Logout] Logging out user: {LoggedInAccount?.Email ?? "None"}");
            LoggedInAccount = null;
        }
    }
}