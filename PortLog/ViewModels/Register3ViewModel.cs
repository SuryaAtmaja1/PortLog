using PortLog.Commands;
using PortLog.Services;
using System.Windows.Input;
using System.Diagnostics;

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
        private bool _isCreating;

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

        public bool IsCreating
        {
            get => _isCreating;
            set => SetProperty(ref _isCreating, value);
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
            return !string.IsNullOrWhiteSpace(CompanyName) && !IsCreating;
        }

        private async void CreateAndJoin(object parameter)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                ErrorMessage = "Nama perusahaan tidak boleh kosong!";
                return;
            }

            if (_accountService.LoggedInAccount == null)
            {
                ErrorMessage = "Tidak ada akun yang login. Silakan login kembali.";
                return;
            }

            IsCreating = true;

            try
            {
                Debug.WriteLine($"[CreateAndJoin] Creating company: {CompanyName}");

                // Create company
                var (company, error) = await _companyService.CreateCompanyAsync(
                    CompanyName,
                    Address ?? string.Empty,
                    Provinsi ?? string.Empty
                );

                if (company == null || !string.IsNullOrEmpty(error))
                {
                    ErrorMessage = error ?? "Gagal membuat perusahaan.";
                    Debug.WriteLine($"[CreateAndJoin] Failed to create company: {ErrorMessage}");
                    IsCreating = false;
                    return;
                }

                Debug.WriteLine($"[CreateAndJoin] Company created with ID: {company.Id}");

                // Join company
                var joinSuccess = await _companyService.JoinCompanyAsync(
                    company.Id,
                    _accountService.LoggedInAccount
                );

                if (!joinSuccess)
                {
                    ErrorMessage = "Perusahaan dibuat, tetapi gagal bergabung.";
                    Debug.WriteLine("[CreateAndJoin] Failed to join company");
                    IsCreating = false;
                    return;
                }

                Debug.WriteLine("[CreateAndJoin] Successfully joined company");

                // Update user's company ID
                var updateSuccess = await _accountService.UpdateUserCompanyAsync(company.Id);

                if (!updateSuccess)
                {
                    ErrorMessage = "Gagal memperbarui profil akun.";
                    Debug.WriteLine("[CreateAndJoin] Failed to update user company");
                    IsCreating = false;
                    return;
                }

                Debug.WriteLine("[CreateAndJoin] Successfully updated user company");

                // Navigate to dashboard
                _navigationService.NavigateTo(new DashboardViewModel(_navigationService, _accountService));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Terjadi kesalahan: {ex.Message}";
                Debug.WriteLine($"[CreateAndJoin] Exception: {ex.Message}");
                Debug.WriteLine($"[CreateAndJoin] Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsCreating = false;
            }
        }

        private void Back()
        {
            _navigationService.NavigateTo(new Register2ViewModel(_navigationService, _accountService));
        }
    }
}