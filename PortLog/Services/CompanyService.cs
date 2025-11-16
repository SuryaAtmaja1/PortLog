using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortLog.Models;
using Supabase;

namespace PortLog.Services
{
    public class CompanyService
    {
        private Client Client => SupabaseClient.Instance;

        public async Task<List<Company>> SearchCompaniesAsync(string joinCode)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
                return new List<Company>();

            try
            {
                var response = await Client
                    .From<Company>()
                    .Get();

                // Filter in memory (Supabase client doesn't support LIKE operator easily)
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

        public async Task<Company> GetCompanyByIdAsync(Guid id)
        {
            try
            {
                var response = await Client
                    .From<Company>()
                    .Where(c => c.Id == id)
                    .Single();

                return response;
            }
            catch
            {
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

                var response = await Client
                    .From<Company>()
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

        public async Task<bool> JoinCompanyAsync(Guid companyId, Account user)
        {
            try
            {
                await Client
                    .From<Account>()
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
