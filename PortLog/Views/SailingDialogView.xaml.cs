using PortLog.Services;
using PortLog.Supabase;
using PortLog.ViewModels;
using System.Windows;

namespace PortLog.Views
{
    public partial class StartSailingDialogView : Window
    {
        public StartSailingDialogView(SupabaseService supabase, long shipId, string shipName)
        {
            InitializeComponent();
            var vm = new StartSailingDialogViewModel(supabase, shipId, shipName);
            vm.RequestClose += (s, e) => { this.DialogResult = e; this.Close(); };
            this.DataContext = vm;
        }
    }
}
