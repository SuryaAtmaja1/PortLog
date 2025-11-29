using PortLog.Commands;
using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using PortLog.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static Supabase.Postgrest.Constants;

namespace PortLog.ViewModels
{
    public class CompanyManagementViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;
        private readonly CompanyService _companyService;

        // ========================
        // COMPANY MAIN INFO
        // ========================
        private Company _company;
        public Company Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }

        public ObservableCollection<CompanyContact> Contacts { get; } = new();

        // ========================
        // EDITING
        // ========================
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        private string _editName;
        public string EditName
        {
            get => _editName;
            set => SetProperty(ref _editName, value);
        }

        private string _editAddress;
        public string EditAddress
        {
            get => _editAddress;
            set => SetProperty(ref _editAddress, value);
        }

        private string _editProvinsi;
        public string EditProvinsi
        {
            get => _editProvinsi;
            set => SetProperty(ref _editProvinsi, value);
        }

        private int _managerCount;
        public int ManagerCount
        {
            get => _managerCount;
            set => SetProperty(ref _managerCount, value);
        }

        private int _captainCount;
        public int CaptainCount
        {
            get => _captainCount;
            set => SetProperty(ref _captainCount, value);
        }

        public ICommand EditInfoCommand { get; }
        public ICommand SaveInfoCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ManageCrewCommand { get; }

        public CompanyManagementViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;
            _companyService = new CompanyService(supabase);

            EditInfoCommand = new RelayCommand(_ => StartEditing());
            SaveInfoCommand = new RelayCommand(async _ => await SaveEditing());
            CancelEditCommand = new RelayCommand(_ => CancelEditing());
            ManageCrewCommand = new RelayCommand(_ => ShowCrewManagement());

            _ = LoadData();
        }

        public void OnNavigatedTo() => _ = LoadData();

        private async Task LoadData()
        {
            try
            {
                var companyId = _accountService.LoggedInAccount.CompanyId;

                // --- LOAD COMPANY DATA ---
                var companyResponse = await _companyService.GetCompanyByIdAsync(companyId.Value);

                Company = companyResponse;

                // --- LOAD CONTACTS ---
                Contacts.Clear();
                var contactsResponse = await _supabase
                    .Table<CompanyContact>()
                    .Filter("company_id", Operator.Equals, companyId.ToString())
                    .Get();

                foreach (var c in contactsResponse.Models)
                    Contacts.Add(c);

                // --- LOAD CREW COUNTS ---
                var crew = await _accountService.GetAccountsByCompanyIdAsync(companyId.Value);

                ManagerCount = crew.Count(a => a.Role == "MANAGER");
                CaptainCount = crew.Count(a => a.Role == "CAPTAIN");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("🔥 COMPANY MANAGEMENT ERROR");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
        private void StartEditing()
        {
            IsEditing = true;

            EditName = Company.Name;
            EditAddress = Company.Address;
            EditProvinsi = Company.Provinsi;
        }

        private void CancelEditing()
        {
            IsEditing = false;
        }

        private async Task SaveEditing()
        {
            try
            {
                Company.Name = EditName;
                Company.Address = EditAddress;
                Company.Provinsi = EditProvinsi;
                Company.LastUpdated = DateTime.UtcNow;

                await _supabase.Table<Company>().Upsert(Company);

                IsEditing = false;

                // Reload to reflect fresh data
                await LoadData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("🔥 ERROR SAVING COMPANY INFO");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void ShowCrewManagement()
        {
            var win = new ManageCrewView
            {
                DataContext = new ManageCrewViewModel(_supabase, _accountService),
                Owner = Application.Current.MainWindow
            };

            win.ShowDialog();
        }
    }
}
