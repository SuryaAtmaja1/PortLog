using PortLog.Commands;
using System.Windows.Input;
using PortLog.Services;
using PortLog.Enumerations;
using System.Text.RegularExpressions;

namespace PortLog.ViewModels
{
    internal class Register1ViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly AccountService _accountService;
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _email;
        private string _fullName;
        private AccountRole _selectedRole = AccountRole.MANAGER;
        private string _errorMessage;

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public AccountRole SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        public Register1ViewModel(NavigationService navigationService, AccountService accountService)
        {
            _navigationService = navigationService;
            _accountService = accountService;

            RegisterCommand = new RelayCommand(Register, CanRegister);
            BackToLoginCommand = new RelayCommand(_ => BackToLogin());
        }

        private bool CanRegister(object parameter)
        {
            return !string.IsNullOrWhiteSpace(FullName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword);
        }

        private async void Register(object parameter)
        {
            ErrorMessage = string.Empty;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "Nama lengkap tidak boleh kosong!";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email tidak boleh kosong!";
                return;
            }

            // Validate email format
            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Format email tidak valid!";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password tidak boleh kosong!";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Konfirmasi password tidak sesuai!";
                return;
            }

            if (Password.Length < 8)
            {
                ErrorMessage = "Password minimal 8 karakter!";
                return;
            }

            // Register with all required fields
            var (success, error) = await _accountService.RegisterAsync(
                FullName,
                Email,
                SelectedRole,
                Password
            );

            if (success)
            {
                // Navigate to Register2 (company selection/creation)
                _navigationService.NavigateTo(new Register2ViewModel(_navigationService, _accountService));
            }
            else
            {
                ErrorMessage = error;
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Basic email validation regex
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private void BackToLogin()
        {
            _navigationService.NavigateTo(new LoginViewModel(_navigationService, _accountService));
        }
    }
}