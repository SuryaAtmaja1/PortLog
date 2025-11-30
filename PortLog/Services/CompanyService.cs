using System.Diagnostics;
using PortLog.Helpers;
using PortLog.Models;
using PortLog.Supabase;
using Supabase.Postgrest;

namespace PortLog.Services
{
    public class CompanyService
    {
        private readonly SupabaseService _supabase;

        public CompanyService(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public async Task<List<Company>> SearchCompaniesAsync(string joinCode)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
                return new List<Company>();

            try
            {
                Debug.WriteLine($"[SearchCompaniesAsync] Searching for join code: {joinCode}");

                var response = await _supabase
                    .Table<Company>()
                    .Filter("join_code", Constants.Operator.Equals, joinCode)
                    .Get();

                var companies = response.Models.ToList();
                Debug.WriteLine($"[SearchCompaniesAsync] Found {companies.Count} companies");

                return companies;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SearchCompaniesAsync] Exception: {ex.Message}");
                return new List<Company>();
            }
        }

        public async Task<Company?> GetCompanyByIdAsync(Guid id)
        {
            try
            {

                var response = await _supabase
                    .Table<Company>()
                    .Filter("id", Constants.Operator.Equals, id.ToString())
                    .Get();

                if (response == null)
                {
                    System.Diagnostics.Debug.WriteLine("GetCompanyByIdAsync: response is null");
                    return null;
                }

                var company = response.Models?.FirstOrDefault();

                return company;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCompanyByIdAsync: exception: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return null;
            }
        }


        public async Task<(Company? company, string error)> CreateCompanyAsync(
            string name,
            string address,
            string provinsi
            )
        {
            if (string.IsNullOrWhiteSpace(name))
                return (null, "Nama perusahaan kosong!");
            if (string.IsNullOrWhiteSpace(address))
                return (null, "Alamat kosong!");
            if (string.IsNullOrWhiteSpace(provinsi))
                return (null, "Provinsi kosong!");

            try
            {
                // Generate GUID
                Guid newCompanyId = Guid.NewGuid();
                string joinCode = JoinCode.CreateFromGuid(newCompanyId);

                var newCompany = new Company
                {
                    Id = newCompanyId,
                    Name = name,
                    Address = address,
                    Provinsi = provinsi.ToUpper(),
                    JoinCode = joinCode,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                Debug.WriteLine($"[CreateCompanyAsync] Creating company with ID: {newCompanyId} and Join Code: {joinCode}");

                var response = await _supabase
                    .Table<Company>()
                    .Insert(newCompany);

                var createdCompany = response.Models.FirstOrDefault();

                if (createdCompany != null && createdCompany.Id != Guid.Empty)
                {
                    Debug.WriteLine($"[CreateCompanyAsync] Company created successfully with ID: {createdCompany.Id} & Join code: {createdCompany.JoinCode}");
                    return (createdCompany, string.Empty);
                }
                else
                {
                    // Fallback: fetch by ID
                    Debug.WriteLine("[CreateCompanyAsync] Re-fetching company by ID");
                    var fetchedCompany = await _supabase
                        .Table<Company>()
                        .Where(x => x.Id == newCompanyId)
                        .Single();

                    if (fetchedCompany != null)
                    {
                        Debug.WriteLine($"[CreateCompanyAsync] Company created successfully with ID: {fetchedCompany.Id} & Join code: {fetchedCompany.JoinCode}");
                        return (fetchedCompany, string.Empty);
                    }

                    return (null, "Gagal membuat company.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CreateCompanyAsync] Exception: {ex.Message}");
                Debug.WriteLine($"[CreateCompanyAsync] Stack trace: {ex.StackTrace}");
                return (null, $"Kesalahan saat membuat perusahaan: {ex.Message}");
            }
        }
        public async Task<bool> JoinCompanyAsync(Guid companyId, Account user)
        {
            try
            {
                Debug.WriteLine($"[JoinCompanyAsync] User {user.Email} joining company {companyId}");

                user.CompanyId = companyId;
                user.LastUpdated = DateTime.UtcNow;

                await _supabase
                    .Table<Account>()
                    .Update(user);

                Debug.WriteLine("[JoinCompanyAsync] Successfully joined company");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JoinCompanyAsync] Exception: {ex.Message}");
                return false;
            }
        }
    }
}
