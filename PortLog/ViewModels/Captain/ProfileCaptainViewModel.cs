using PortLog.Commands;
using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PortLog.ViewModels
{
    public class ProfileCaptainViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;

        public Guid CaptainId { get; }

        // ============================
        // CAPTAIN INFO
        // ============================

        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _contact;
        public string Contact { get => _contact; set => SetProperty(ref _contact, value); }

        // ============================
        // COMPANY INFO (NOW LIST)
        // ============================

        private string _companyName;
        public string CompanyName { get => _companyName; set => SetProperty(ref _companyName, value); }

        private string _companyAddress;
        public string CompanyAddress { get => _companyAddress; set => SetProperty(ref _companyAddress, value); }

        public ObservableCollection<string> CompanyEmails { get; set; } = new();
        public ObservableCollection<string> CompanyNumbers { get; set; } = new();

        // ============================

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

        private string _successMessage;
        public string SuccessMessage { get => _successMessage; set => SetProperty(ref _successMessage, value); }

        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }

        private string _originalName;
        private string _originalEmail;
        private string _originalContact;

        // ============================
        // COMMANDS
        // ============================

        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ProfileCaptainViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;
            CaptainId = _accountService.LoggedInAccount.Id;

            EditCommand = new RelayCommand(_ => EnterEditMode());
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => CancelEdit());
        }

        public void OnNavigatedTo() => _ = LoadProfile();

        // ============================
        // LOAD DATA
        // ============================

        private async Task LoadProfile()
        {
            try
            {
                var acc = await _supabase.Table<Account>()
                    .Where(a => a.Id == CaptainId)
                    .Single();

                if (acc == null) return;

                Name = acc.Name;
                Email = acc.Email;
                Contact = acc.Contact;

                _originalName = Name;
                _originalEmail = Email;
                _originalContact = Contact;

                // Load company data
                if (acc.CompanyId != null)
                {
                    var company = await _supabase.Table<Company>()
                        .Where(c => c.Id == acc.CompanyId)
                        .Single();

                    CompanyName = company?.Name ?? "-";
                    CompanyAddress = company?.Address ?? "-";

                    // ===== Load ALL company contacts =====
                    CompanyEmails.Clear();
                    CompanyNumbers.Clear();

                    var contacts = await _supabase.Table<CompanyContact>()
                        .Where(x => x.CompanyId == acc.CompanyId)
                        .Get();

                    foreach (var ct in contacts.Models)
                    {
                        if (!string.IsNullOrWhiteSpace(ct.Email))
                            CompanyEmails.Add(ct.Email);

                        if (!string.IsNullOrWhiteSpace(ct.Phone))
                            CompanyNumbers.Add(ct.Phone);
                    }

                    if (!CompanyEmails.Any()) CompanyEmails.Add("-");
                    if (!CompanyNumbers.Any()) CompanyNumbers.Add("-");
                }
                else
                {
                    CompanyName = "Unassigned";
                    CompanyAddress = "-";
                    CompanyEmails = new ObservableCollection<string> { "-" };
                    CompanyNumbers = new ObservableCollection<string> { "-" };
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Gagal memuat profil: " + ex.Message;
            }
        }

        // ============================
        // EDIT MODE
        // ============================

        private void EnterEditMode()
        {
            _originalName = Name;
            _originalEmail = Email;
            _originalContact = Contact;

            ErrorMessage = "";
            SuccessMessage = "";
            IsEditing = true;
        }

        private void CancelEdit()
        {
            Name = _originalName;
            Email = _originalEmail;
            Contact = _originalContact;

            ErrorMessage = "";
            SuccessMessage = "";
            IsEditing = false;
        }

        // ============================
        // SAVE
        // ============================

        private async Task SaveAsync()
        {
            ErrorMessage = "";
            SuccessMessage = "";

            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Nama harus diisi.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email harus diisi.";
                return;
            }

            try
            {
                await _supabase.Table<Account>()
                    .Where(x => x.Id == CaptainId)
                    .Set(x => x.Name, Name.Trim())
                    .Set(x => x.Email, Email.Trim())
                    .Set(x => x.Contact, Contact?.Trim())
                    .Set(x => x.LastUpdated, DateTime.UtcNow)
                    .Update();

                SuccessMessage = "Profil berhasil diperbarui.";
                IsEditing = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Gagal menyimpan perubahan: " + ex.Message;
            }
        }
    }
}
