using PortLog.Commands;
using PortLog.Services;
using System.Windows.Input;
using PortLog.Models;
using System.ComponentModel;

namespace PortLog.ViewModels
{
    public class DashboardManagerViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly NavigationService _navigationService;
        private readonly AccountService _accountService;
        private readonly CompanyService _companyService;
        private string _companyInfo = "Loading...";
        public string CompanyInfo => _companyInfo;

        public string WelcomeMessage => $"Welcome, {_accountService.LoggedInAccount?.Name ?? "User"} (MANAGER)";
        public async Task LoadCompanyInfoAsync()
        {
            if (_accountService.LoggedInAccount?.CompanyId != null)
            {
                Company? company = await _companyService.GetCompanyByIdAsync(
                    _accountService.LoggedInAccount.CompanyId.Value
                );

                _companyInfo = company != null
                    ? $"Company: {company.Name}"
                    : "Tidak terafiliasi";
            }
            else
            {
                _companyInfo = "Tidak terafiliasi";
            }
        }

        public ICommand LogoutCommand { get; }

        public DashboardManagerViewModel(NavigationService navigationService, AccountService accountService)
        {
            _navigationService = navigationService;
            _accountService = accountService;
            _companyService = ((MainViewModel)navigationService.MainViewModel).CompanyService;

            LogoutCommand = new RelayCommand(Logout);
        }

        private void Logout(object parameter)
        {
            _accountService.Logout();
            _navigationService.NavigateTo(new LoginViewModel(_navigationService, _accountService));
        }
    }
}
