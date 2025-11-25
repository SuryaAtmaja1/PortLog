using System.Windows;
using PortLog.Supabase;
using PortLog.Services;
using PortLog.Views;
using PortLog.ViewModels;

namespace PortLog;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
/// 
public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        await SupabaseClient.InitAsync();

        var supabaseService = new SupabaseService(SupabaseClient.Instance);
        GlobalServices.Register(supabaseService);
        GlobalServices.Init();

        var mainWindow = new MainWindow();
        mainWindow.DataContext = new MainViewModel();
        mainWindow.Show();
    }
}