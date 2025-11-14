using Supabase;
using DotNetEnv;

public static class SupabaseClient
{
    public static Supabase.Client Instance { get; private set; }

    public static async Task InitAsync()
    {
        DotNetEnv.Env.Load();

        var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
        var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");

        Instance = new Supabase.Client(url, key, new SupabaseOptions
        {
            AutoConnectRealtime = true
        });

        await Instance.InitializeAsync();
    }
}
