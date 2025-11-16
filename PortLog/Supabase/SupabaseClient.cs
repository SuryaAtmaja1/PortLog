using Supabase;
using DotNetEnv;

public static class SupabaseClient
{
    public static Client? Instance { get; private set; }

    public static async Task InitAsync()
    {
        Env.Load();

        string url = Env.GetString("SUPABASE_URL");
        string key = Env.GetString("SUPABASE_KEY");
        
        
        Instance = new Client(url, key, new SupabaseOptions
        {
            AutoConnectRealtime = true
        });

        await Instance.InitializeAsync();
    }
}
