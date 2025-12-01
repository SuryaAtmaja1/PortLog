using PortLog.Commands;
using PortLog.Services;
using PortLog.Supabase;
using PortLog.ViewModels;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PortLog.ViewModels
{
    public class CaptainDashboardViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly PortLog.Services.NavigationService _navigationService;
        private readonly AccountService _accountService;

        public DashboardCaptainViewModel OverviewVM { get; }

        public FleetCaptainViewModel FleetCaptainVM { get; }

        public VoyageChangeStateViewModel VoyageStateVM { get; }

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

            SelectedMenu = "Overview";
            CurrentPage = OverviewVM;

            InvokeOnNavigatedToIfExists(CurrentPage);
        }

        private void OnNavigate(object parameter)
        {
            SelectedMenu = parameter?.ToString();

            switch (SelectedMenu)
            {
                case "Overview":
                    CurrentPage = OverviewVM;
                    break;
                case "Fleet":
                    CurrentPage = FleetCaptainVM;
                    break;
                case "VoyageState":
                    CurrentPage = VoyageStateVM;
                    break;
                case "Profile":
                    CurrentPage = ProfileCaptainVM;
                    break;
                default:
                    CurrentPage = OverviewVM;
                    break;
            }

            InvokeOnNavigatedToIfExists(CurrentPage);
        }

        private void OnLogout(object parameter)
        {
            _accountService.Logout();
            _navigationService.NavigateTo(new LoginViewModel(_navigationService, _accountService));
        }

        private void InvokeOnNavigatedToIfExists(object page)
        {
            if (page == null) return;

            try
            {
                var mi = page.GetType().GetMethod(
                    "OnNavigatedTo",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);

                if (mi != null)
                {
                    mi.Invoke(page, null);
                }
            }
            catch (TargetInvocationException tie)
            {
                System.Diagnostics.Debug.WriteLine($"InvokeOnNavigatedToIfExists: target invocation error: {tie.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InvokeOnNavigatedToIfExists: reflection error: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
