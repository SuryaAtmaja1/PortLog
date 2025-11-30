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
        private readonly ContactService _contactService;

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
        // EDITING COMPANY INFO
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

        // ========================
        // CONTACT MANAGEMENT
        // ========================
        private CompanyContact _selectedContact;
        public CompanyContact SelectedContact
        {
            get => _selectedContact;
            set => SetProperty(ref _selectedContact, value);
        }

        private bool _isAddingContact;
        public bool IsAddingContact
        {
            get => _isAddingContact;
            set => SetProperty(ref _isAddingContact, value);
        }

        private bool _isEditingContact;
        public bool IsEditingContact
        {
            get => _isEditingContact;
            set => SetProperty(ref _isEditingContact, value);
        }

        private string _contactName;
        public string ContactName
        {
            get => _contactName;
            set => SetProperty(ref _contactName, value);
        }

        private string _contactEmail;
        public string ContactEmail
        {
            get => _contactEmail;
            set => SetProperty(ref _contactEmail, value);
        }

        private string _contactPhone;
        public string ContactPhone
        {
            get => _contactPhone;
            set => SetProperty(ref _contactPhone, value);
        }

        // ========================
        // COMMANDS
        // ========================
        public ICommand EditInfoCommand { get; }
        public ICommand SaveInfoCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ManageCrewCommand { get; }

        public ICommand AddContactCommand { get; }
        public ICommand StartEditContactCommand { get; }
        public ICommand SaveContactCommand { get; }
        public ICommand DeleteContactCommand { get; }
        public ICommand CancelContactEditCommand { get; }
        public ICommand CopyJoinCodeCommand { get; }

        public CompanyManagementViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;
            _companyService = new CompanyService(supabase);
            _contactService = new ContactService(supabase);

            EditInfoCommand = new RelayCommand(_ => StartEditing());
            SaveInfoCommand = new RelayCommand(async _ => await SaveEditing());
            CancelEditCommand = new RelayCommand(_ => CancelEditing());
            ManageCrewCommand = new RelayCommand(_ => ShowCrewManagement());

            // contact commands - FIXED: accept parameter
            AddContactCommand = new RelayCommand(_ => StartAddContact());
            StartEditContactCommand = new RelayCommand(param => StartEditContact(param as CompanyContact));
            SaveContactCommand = new RelayCommand(async _ => await SaveContact());
            DeleteContactCommand = new RelayCommand(async param => await DeleteContact(param as CompanyContact));
            CancelContactEditCommand = new RelayCommand(_ => CancelContactEditing());
            CopyJoinCodeCommand = new RelayCommand(_ => CopyJoinCodeToClipboard());

            _ = LoadData();
        }

        public void OnNavigatedTo() => _ = LoadData();

        private async Task LoadData()
        {
            try
            {
                var companyId = _accountService.LoggedInAccount.CompanyId.Value;

                // --- LOAD COMPANY DATA ---
                var companyResponse = await _companyService.GetCompanyByIdAsync(companyId);
                Company = companyResponse;

                // --- LOAD CONTACTS via ContactService ---
                await LoadContacts(companyId);

                // --- LOAD CREW COUNTS ---
                var crew = await _accountService.GetAccountsByCompanyIdAsync(companyId);

                ManagerCount = crew.Count(a => a.Role == "MANAGER");
                CaptainCount = crew.Count(a => a.Role == "CAPTAIN");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("🔥 COMPANY MANAGEMENT ERROR");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private async Task LoadContacts(Guid companyId)
        {
            Contacts.Clear();

            var contacts = await _contactService.GetContactsByCompanyAsync(companyId);

            foreach (var c in contacts)
                Contacts.Add(c);
        }

        // ========================
        // COMPANY EDITING METHODS
        // ========================
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

        // ========================
        // CONTACT METHODS - FIXED
        // ========================
        private void StartAddContact()
        {
            IsAddingContact = true;
            IsEditingContact = false;

            ContactName = string.Empty;
            ContactEmail = string.Empty;
            ContactPhone = string.Empty;

            OnPropertyChanged(nameof(ContactName));
            OnPropertyChanged(nameof(ContactEmail));
            OnPropertyChanged(nameof(ContactPhone));
        }

        private void StartEditContact(CompanyContact contact)
        {
            if (contact == null)
                return;

            // Set the selected contact
            SelectedContact = contact;

            IsEditingContact = true;
            IsAddingContact = false;

            ContactName = contact.Name;
            ContactEmail = contact.Email;
            ContactPhone = contact.Phone;

            OnPropertyChanged(nameof(ContactName));
            OnPropertyChanged(nameof(ContactEmail));
            OnPropertyChanged(nameof(ContactPhone));
        }

        private void CancelContactEditing()
        {
            IsAddingContact = false;
            IsEditingContact = false;
            SelectedContact = null;
        }

        private async Task SaveContact()
        {
            try
            {
                var companyId = _accountService.LoggedInAccount.CompanyId.Value;

                if (IsAddingContact)
                {
                    var contact = new CompanyContact
                    {
                        CompanyId = companyId,
                        Name = ContactName,
                        Email = ContactEmail,
                        Phone = ContactPhone
                    };

                    await _contactService.AddContactAsync(contact);
                }
                else if (IsEditingContact && SelectedContact != null)
                {
                    SelectedContact.Name = ContactName;
                    SelectedContact.Email = ContactEmail;
                    SelectedContact.Phone = ContactPhone;

                    await _contactService.UpdateContactAsync(SelectedContact);
                }

                IsAddingContact = false;
                IsEditingContact = false;
                SelectedContact = null;

                await LoadContacts(companyId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("🔥 ERROR SAVING CONTACT");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private async Task DeleteContact(CompanyContact contact)
        {
            if (contact == null)
                return;

            var confirm = MessageBox.Show(
                $"Delete contact {contact.Name}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                await _contactService.DeleteContactAsync(contact);

                var companyId = _accountService.LoggedInAccount.CompanyId.Value;
                await LoadContacts(companyId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("🔥 ERROR DELETING CONTACT");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void CopyJoinCodeToClipboard()
        {
            try
            {
                if (Company?.JoinCode != null)
                {
                    Clipboard.SetText(Company.JoinCode);
                    MessageBox.Show(
                        "Join code copied to clipboard!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("🔥 ERROR COPYING TO CLIPBOARD");
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show(
                    "Failed to copy join code",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}