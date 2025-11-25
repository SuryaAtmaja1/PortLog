using System.Windows.Input;
using PortLog.Commands;      // RelayCommand
using PortLog.Enumerations;
using PortLog.Supabase;
using PortLog.Services;
 
namespace PortLog.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly AccountService _accountService;
        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel(NavigationService navigationService, AccountService accountService)
        {
            _navigationService = navigationService;
            _accountService = accountService;

            LoginCommand = new RelayCommand(Login, CanLogin);
            NavigateToRegisterCommand = new RelayCommand(_ => NavigateToRegister());
        }

        private bool CanLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private async void Login(object parameter)
        {
            var (success, error) = await _accountService.LoginAsync(Username, Password);

            if (success)
            {
                // Navigate to appropriate dashboard based on role
                if (_accountService.LoggedInAccount?.RoleEnum == AccountRole.MANAGER)
                {
                    _navigationService.NavigateTo(new DashboardManagerViewModel(_navigationService, _accountService));
                }
                else
                {
                    _navigationService.NavigateTo(new DashboardCaptainViewModel(_navigationService, _accountService));
                }
            }
            else
            {
                ErrorMessage = error;
            }
        }

        private void NavigateToRegister()
        {
            _navigationService.NavigateTo(new Register1ViewModel(_navigationService, _accountService));
        }
    }
}
