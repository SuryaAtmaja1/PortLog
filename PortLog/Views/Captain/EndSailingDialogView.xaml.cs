using PortLog.Services;
using PortLog.Supabase;
using PortLog.ViewModels;
using System.Windows;

namespace PortLog.Views
{
    public partial class EndSailingDialogView : Window
    {
        public EndSailingDialogView(SupabaseService supabase, long shipId, string shipName)
        {
            InitializeComponent();
            var vm = new EndSailingDialogViewModel(supabase, shipId, shipName);
            vm.RequestClose += (s, e) => { this.DialogResult = e; this.Close(); };
            this.DataContext = vm;
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
