using PortLog.Commands;
using PortLog.Services;
using PortLog.Supabase;
using PortLog.ViewModels;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PortLog.ViewModels
{
    public class CaptainDashboardViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly PortLog.Services.NavigationService _navigationService;
        private readonly AccountService _accountService;

        // Only Overview page for now
        public DashboardCaptainViewModel OverviewVM { get; }

        public FleetCaptainViewModel FleetCaptainVM { get; }

        public VoyageChangeStateViewModel VoyageStateVM {  get; }

        public ProfileCaptainViewModel ProfileCaptainVM { get; }
        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }

        private object currentPage;
        public object CurrentPage
        {
            get => currentPage;
            set { currentPage = value; OnPropertyChanged(nameof(CurrentPage)); }
        }

        private string selectedMenu;
        public string SelectedMenu
        {
            get => selectedMenu;
            set { selectedMenu = value; OnPropertyChanged(nameof(SelectedMenu)); }
        }

        public CaptainDashboardViewModel(PortLog.Services.NavigationService navigationService, AccountService accountService)
        {
            _navigationService = navigationService ?? throw new System.ArgumentNullException(nameof(navigationService));
            _accountService = accountService ?? throw new System.ArgumentNullException(nameof(accountService));

            // obtain supabase from main vm the same pattern as your project
            var mainVm = navigationService.MainViewModel as MainViewModel;
            var supabase = mainVm?.SupabaseService;
            var accountSvc = mainVm?.AccountService;

            OverviewVM = new DashboardCaptainViewModel(navigationService, accountService);
            FleetCaptainVM = new FleetCaptainViewModel(supabase, accountService);
            VoyageStateVM = new VoyageChangeStateViewModel(supabase, accountService);
            ProfileCaptainVM = new ProfileCaptainViewModel(supabase, accountService);

            NavigateCommand = new RelayCommand(OnNavigate);
            LogoutCommand = new RelayCommand(OnLogout);

            OverviewVM.ParentVM = this;

            // default page
            SelectedMenu = "Overview";
            CurrentPage = OverviewVM;
            OverviewVM.OnNavigatedTo();
        }

        private void OnNavigate(object parameter)
        {
            SelectedMenu = parameter?.ToString();

            switch (SelectedMenu)
            {
                case "Overview":
                    CurrentPage = OverviewVM;
                    OverviewVM.OnNavigatedTo();
                    break;
                case "Fleet":
                    CurrentPage = FleetCaptainVM;
                    FleetCaptainVM.OnNavigatedTo();
                    break;
                case "VoyageState":
                    CurrentPage = VoyageStateVM;
                    VoyageStateVM.OnNavigatedTo();
                    break;
                case "Profile":
                    CurrentPage = ProfileCaptainVM;
                    ProfileCaptainVM.OnNavigatedTo();
                    break;
            }
        }


        private void OnLogout(object parameter)
        {
            _accountService.Logout();
            _navigationService.NavigateTo(new LoginViewModel(_navigationService, _accountService));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
