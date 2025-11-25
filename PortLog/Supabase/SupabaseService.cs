using Supabase;
using Supabase.Postgrest.Interfaces;
using Supabase.Postgrest.Models;

namespace PortLog.Supabase
{
    public class SupabaseService
    {
        private readonly Client _client;

        public SupabaseService(Client client)
        {
            _client = client;
        }
        public IPostgrestTable<T> Table<T>()
            where T : BaseModel, new()
        {
            return _client.From<T>();
        }
        public Client Client => _client;
    }
}
