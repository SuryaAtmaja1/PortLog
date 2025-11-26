using PortLog.Commands;      // RelayCommand
using PortLog.Enumerations;
using PortLog.Services;
using PortLog.Supabase;
using System.ComponentModel;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PortLog.ViewModels
{
    public class LoginViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        private readonly AccountService _accountService;
        private string _username;
        private string _password;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
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
                    _navigationService.NavigateTo(new DashboardViewModel(_navigationService, _accountService));
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void NavigateToRegister()
        {
            _navigationService.NavigateTo(new Register1ViewModel(_navigationService, _accountService));
        }
    }
}
