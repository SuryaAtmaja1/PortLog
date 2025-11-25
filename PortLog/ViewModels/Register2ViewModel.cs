
using PortLog.Commands;
using PortLog.Models;
using PortLog.Services;
using System.Windows.Input;
using PortLog.Enumerations;

namespace PortLog.ViewModels
{
    public class Register2ViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;
        private readonly AccountService _accountService;
        private readonly CompanyService _companyService;
        private string _searchTerm;
        private List<Company> _searchResults;
        private Company _selectedCompany;
        private string _message;

        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        public List<Company> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        public Company SelectedCompany
        {
            get => _selectedCompany;
            set => SetProperty(ref _selectedCompany, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public bool CanCreateCompany => _accountService.LoggedInAccount?.RoleEnum == AccountRole.MANAGER;

        public ICommand SearchCommand { get; }
        public ICommand JoinCompanyCommand { get; }
        public ICommand CreateCompanyCommand { get; }
        public ICommand SkipCommand { get; }

        public Register2ViewModel(NavigationService navigationService, AccountService accountService)
        {
            _navigationService = navigationService;
            _accountService = accountService;
            _companyService = ((MainViewModel)navigationService.MainViewModel).CompanyService;

            SearchCommand = new RelayCommand(Search);
            JoinCompanyCommand = new RelayCommand(JoinCompany, _ => SelectedCompany != null);
            CreateCompanyCommand = new RelayCommand(CreateCompany, _ => CanCreateCompany);
            SkipCommand = new RelayCommand(Skip);
        }

        private async void Search(object parameter)
        {
            Message = string.Empty;
            SearchResults = await _companyService.SearchCompaniesAsync(SearchTerm);

            if (!SearchResults.Any())
            {
                Message = "Tidak ditemukan! " + (CanCreateCompany ? "Anda bisa membuat profil perusahaan." : "Coba kode berbeda.");
            }
        }

        private async void JoinCompany(object parameter)
        {
            if (SelectedCompany != null)
            {
                await _companyService.JoinCompanyAsync(SelectedCompany.Id, _accountService.LoggedInAccount);
                await _accountService.UpdateUserCompanyAsync(SelectedCompany.Id);

                // Navigate to dashboard
                if (_accountService.LoggedInAccount.RoleEnum == AccountRole.MANAGER)
                {
                    _navigationService.NavigateTo(new DashboardManagerViewModel(_navigationService, _accountService));
                }
                else
                {
                    _navigationService.NavigateTo(new DashboardCaptainViewModel(_navigationService, _accountService));
                }
            }
        }

        private void CreateCompany(object parameter)
        {
            _navigationService.NavigateTo(new Register3ViewModel(_navigationService, _accountService));
        }

        private void Skip(object parameter)
        {
            // Navigate to dashboard without company
            if (_accountService.LoggedInAccount.RoleEnum == AccountRole.MANAGER)
            {
                _navigationService.NavigateTo(new DashboardManagerViewModel(_navigationService, _accountService));
            }
            else
            {
                _navigationService.NavigateTo(new DashboardCaptainViewModel(_navigationService, _accountService));
            }
        }
    }
}