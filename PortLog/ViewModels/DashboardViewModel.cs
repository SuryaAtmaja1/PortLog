using PortLog.Commands;
using PortLog.Services;
using PortLog.Supabase;
using System.ComponentModel;
using System.Windows.Input;
//using System.Windows.Navigation;

namespace PortLog.ViewModels
{
    public class DashboardViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        private readonly AccountService _accountService;

        // Constructor baru yang sesuai pola NavigationService
        public DashboardViewModel(NavigationService navigationService, AccountService accountService)
        {
            _navigationService = navigationService;
            _accountService = accountService;

            NavigateCommand = new RelayCommand(OnNavigate);

            CompanyVM = new CompanyManagementViewModel(
                navigationService.MainViewModel.SupabaseService,
                navigationService.MainViewModel.AccountService
            );

            HomeVM = new DashboardHomeViewModel(
                navigationService.MainViewModel.SupabaseService,
                navigationService.MainViewModel.AccountService
            );

            FleetVM = new FleetViewModel(
                navigationService.MainViewModel.SupabaseService,
                navigationService.MainViewModel.AccountService
            );

            VoyageVM = new VoyageListViewModel(
                navigationService.MainViewModel.SupabaseService,
                navigationService.MainViewModel.AccountService
            );

            InsightVM = new InsightViewModel(
                navigationService.MainViewModel.SupabaseService,
                navigationService.MainViewModel.AccountService
            );

            // Default Page
            SelectedMenu = "Home";
            CurrentPage = HomeVM;

            LogoutCommand = new RelayCommand(Logout);
        }

        public ICommand NavigateCommand { get; }

        private object currentPage;
        public object CurrentPage
        {
            get => currentPage;
            set { currentPage = value; OnPropertyChanged(nameof(CurrentPage)); }
        }

        // ViewModel Halaman
        public DashboardHomeViewModel HomeVM { get; }
        public CompanyManagementViewModel CompanyVM { get; }
        public FleetViewModel FleetVM { get; }
        public ShipViewModel ShipVM { get; } = new();
        public VoyageListViewModel VoyageVM { get; }
        public InsightViewModel InsightVM { get; }

        public ICommand LogoutCommand { get; }

        private string selectedMenu;
        public string SelectedMenu
        {
            get => selectedMenu;
            set { selectedMenu = value; OnPropertyChanged(nameof(SelectedMenu)); }
        }

        private void OnNavigate(object parameter)
        {
            SelectedMenu = parameter?.ToString();

            switch (SelectedMenu)
            {
                case "Home":
                    CurrentPage = HomeVM;
                    HomeVM.OnNavigatedTo();
                    break;
                case "Company":
                    CurrentPage = CompanyVM;
                    CompanyVM.OnNavigatedTo();
                    break;
                case "Fleet":
                    CurrentPage = FleetVM;
                    FleetVM.OnNavigatedTo();
                    break;
                case "Voyage":
                    CurrentPage = VoyageVM;
                    VoyageVM.OnNavigatedTo();
                    break;
                case "Insight":
                    CurrentPage = InsightVM;
                    InsightVM.OnNavigatedTo();
                    break;
            }
        }
        private void Logout(object parameter)
        {
            _accountService.Logout();
            _navigationService.NavigateTo(new LoginViewModel(_navigationService, _accountService));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
