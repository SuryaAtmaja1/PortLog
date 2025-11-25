using Supabase;
using DotNetEnv;

namespace PortLog.Supabase
{
    public static class SupabaseClient
    {
        public static Client? Instance { get; private set; }

        public static async Task InitAsync()
        {
            Env.Load();

            string url = Env.GetString("SUPABASE_URL") ?? throw new Exception("Supabase URL not found");
            string key = Env.GetString("SUPABASE_KEY") ?? throw new Exception("Supabase key not found");
        
        
            Instance = new Client(url, key, new SupabaseOptions
            {
                AutoConnectRealtime = true
            });

            await Instance.InitializeAsync();
        }
    }
}
