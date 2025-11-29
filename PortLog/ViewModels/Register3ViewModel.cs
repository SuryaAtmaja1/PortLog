using PortLog.Commands;
using PortLog.Services;
using System.Windows.Input;

namespace PortLog.ViewModels
{
    public class Register3ViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly AccountService _accountService;
        private readonly CompanyService _companyService;
        private string _companyName;
        private string _address;
        private string _provinsi;
        private string _errorMessage;

        public string CompanyName
        {
            get => _companyName;
            set => SetProperty(ref _companyName, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }
        public string Provinsi
        {
            get => _provinsi;
            set => SetProperty(ref _provinsi, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand CreateAndJoinCommand { get; }
        public ICommand BackCommand { get; }

        public Register3ViewModel(NavigationService navigationService, AccountService accountService)
        {
            _navigationService = navigationService;
            _accountService = accountService;
            _companyService = ((MainViewModel)navigationService.MainViewModel).CompanyService;

            CreateAndJoinCommand = new RelayCommand(CreateAndJoin, CanCreate);
            BackCommand = new RelayCommand(_ => Back());
        }

        private bool CanCreate(object parameter)
        {
            return !string.IsNullOrWhiteSpace(CompanyName);
        }

        private async void CreateAndJoin(object parameter)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                ErrorMessage = "Nama perusahaan kosong!.";
                return;
            }

            var (company, error) = await _companyService.CreateCompanyAsync(CompanyName, Address, Provinsi);
            await _companyService.JoinCompanyAsync(company.Id, _accountService.LoggedInAccount);
            await _accountService.UpdateUserCompanyAsync(company.Id);

            _navigationService.NavigateTo(new DashboardViewModel(_navigationService, _accountService));
        }

        private void Back()
        {
            _navigationService.NavigateTo(new Register2ViewModel(_navigationService, _accountService));
        }
    }
}
