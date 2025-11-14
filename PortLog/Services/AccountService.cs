using PortLog.Models;
using Supabase;
using Supabase.Gotrue.Interfaces;
using System;
using System.Threading.Tasks;

namespace PortLog.Services
{
    public class AccountService
    {
        private Client Client => SupabaseClient.Instance;

        public Account LoggedInAccount { get; private set; }

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
    }
}
