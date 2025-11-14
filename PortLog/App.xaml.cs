using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using DotNetEnv;
using Supabase;
using PortLog.Services;

namespace PortLog;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
/// 
public partial class App : Application
{
    SupabaseService Service;
    protected override async void OnStartup(StartupEventArgs e)
    {
        //DotNetEnv.Env.Load();

        base.OnStartup(e);

        await SupabaseClient.InitAsync();
        Service = new SupabaseService(SupabaseClient.Instance);
        GlobalServices.Init();

        // Lanjut buka window pertama
        //var main = new MainWindow();
        //main.Show();
    }
}

