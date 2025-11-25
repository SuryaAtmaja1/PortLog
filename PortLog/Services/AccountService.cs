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

        public void Logout()
        {
            Debug.WriteLine($"[Logout] Logging out user: {LoggedInAccount?.Email ?? "None"}");
            LoggedInAccount = null;
        }
    }
}