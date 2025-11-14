using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using PortLog.Commands;      // RelayCommand
using PortLog.Services;      // GlobalServices / AccountService (optional)
 
namespace PortLog.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _main;
        public LoginViewModel(MainViewModel main)
        {
            _main = main;
            LoginCommand = new RelayCommand(async _ => await Login());
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        private string _error;
        public string Error
        {
            get => _error;
            set { _error = value; OnPropertyChanged(nameof(Error)); }
        }
        public ICommand LoginCommand { get; }

        private async Task Login()
        {
            var ok = await GlobalServices.Account.LoginAsync(Email, Password);

            if (ok)
                _main.NavigateToDashboard();
            else
                Error = "Incorrect email or password.";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
