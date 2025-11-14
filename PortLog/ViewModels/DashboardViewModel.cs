using PortLog.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PortLog.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public ICommand NavigateCommand { get; }

        private object currentPage;
        public object CurrentPage
        {
            get => currentPage;
            set { currentPage = value; OnPropertyChanged(nameof(CurrentPage)); }
        }

        // ViewModel Halaman
        public DashboardHomeViewModel HomeVM { get; } = new();
        public CompanyManagementViewModel CompanyVM { get; } = new();
        public FleetViewModel FleetVM { get; } = new();
        public ShipViewModel ShipVM { get; } = new();
        public VoyageListViewModel VoyageVM { get; } = new();
        public InsightViewModel InsightVM { get; } = new();

        private string selectedMenu;
        public string SelectedMenu
        {
            get => selectedMenu;
            set { selectedMenu = value; OnPropertyChanged(nameof(SelectedMenu)); }
        }

        public DashboardViewModel()
        {
            NavigateCommand = new RelayCommand(OnNavigate);

            // Default halaman
            SelectedMenu = "Home";
            CurrentPage = HomeVM;
        }

        private void OnNavigate(object parameter)
        {
            SelectedMenu = parameter?.ToString();

            switch (SelectedMenu)
            {
                case "Home":
                    CurrentPage = HomeVM;
                    break;
                case "Company":
                    CurrentPage = CompanyVM;
                    break;
                case "Fleet":
                    CurrentPage = FleetVM;
                    break;
                case "Ship":
                    CurrentPage = ShipVM;
                    break;
                case "Voyage":
                    CurrentPage = VoyageVM;
                    break;
                case "Insight":
                    CurrentPage = InsightVM;
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
