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

        public async Task<List<Company>> SearchCompaniesAsync(
            string joinCode)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
                return new List<Company>();

            try
            {
                var response = await _supabase
                    .Table<Company>()
                    .Get();

                // Filter by Join code
                var companies = response.Models
                    .Where(c => c.JoinCode == joinCode)
                    .ToList();

                return companies;
            }
            catch
            {
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
            {
                return (null, "Company name is required.");
            }

            try
            {
                var newCompany = new Company
                {
                    Name = name,
                    Address = address,
                    // TODO: Randomize Join Code, Create helper function
                    JoinCode = "idaohzgk",
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                var response = await _supabase
                    .Table<Company>()
                    .Insert(newCompany);

                var createdCompany = response.Models.FirstOrDefault();

                if (createdCompany != null)
                {
                    return (createdCompany, string.Empty);
                }

                return (null, "Failed to create company.");
            }
            catch (Exception ex)
            {
                return (null, $"Error creating company: {ex.Message}");
            }
        }

        public async Task<bool> JoinCompanyAsync(
            Guid companyId,
            Account user)
        {
            try
            {
                await _supabase
                    .Table<Account>()
                    .Where(a => a.Id == user.Id)
                    .Set(a => a.CompanyId, companyId)
                    .Update();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
