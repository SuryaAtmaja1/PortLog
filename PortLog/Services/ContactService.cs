using PortLog.Models;
using PortLog.Supabase;
using Supabase.Postgrest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace PortLog.Services
{
    public class ContactService
    {
        private readonly SupabaseService _supabase;

        public ContactService(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        // ===============================
        // GET ALL CONTACTS BY COMPANY ID
        // ===============================
        public async Task<List<CompanyContact>> GetContactsByCompanyAsync(Guid companyId)
        {
            try
            {
                var res = await _supabase
                    .Table<CompanyContact>()
                    .Filter("company_id", Operator.Equals, companyId.ToString())
                    .Order("name", Ordering.Ascending)
                    .Get();

                return res.Models ?? new List<CompanyContact>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetContactsByCompanyAsync] {ex.Message}");
                return new List<CompanyContact>();
            }
        }

        // ===============================
        // GET BY ID
        // ===============================
        public async Task<CompanyContact?> GetContactByIdAsync(long id)
        {
            try
            {
                var res = await _supabase
                    .Table<CompanyContact>()
                    .Filter("id", Operator.Equals, id.ToString())
                    .Limit(1)
                    .Get();

                return res.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetContactByIdAsync] {ex.Message}");
                return null;
            }
        }

        // ===============================
        // ADD NEW CONTACT
        // ===============================
        public async Task<bool> AddContactAsync(CompanyContact contact)
        {
            try
            {
                await _supabase.Table<CompanyContact>().Insert(contact);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AddContactAsync] {ex.Message}");
                return false;
            }
        }

        // ===============================
        // UPDATE CONTACT
        // ===============================
        public async Task<bool> UpdateContactAsync(CompanyContact contact)
        {
            try
            {
                await _supabase.Table<CompanyContact>().Update(contact);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateContactAsync] {ex.Message}");
                return false;
            }
        }

        // ===============================
        // DELETE CONTACT
        // ===============================
        public async Task<bool> DeleteContactAsync(CompanyContact contact)
        {
            try
            {
                await _supabase.Table<CompanyContact>().Delete(contact);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DeleteContactAsync] {ex.Message}");
                return false;
            }
        }

        // ===============================
        // SEARCH CONTACTS
        // ===============================
        public async Task<List<CompanyContact>> SearchContactsAsync(Guid companyId, string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    return await GetContactsByCompanyAsync(companyId);

                var res = await _supabase
                    .Table<CompanyContact>()
                    .Filter("company_id", Operator.Equals, companyId.ToString())
                    .Filter("name", Operator.ILike, $"%{keyword}%")
                    .Get();

                return res.Models ?? new List<CompanyContact>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SearchContactsAsync] {ex.Message}");
                return new List<CompanyContact>();
            }
        }
    }
}
