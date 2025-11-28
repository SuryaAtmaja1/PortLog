using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using PortLog.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static Supabase.Postgrest.Constants;

namespace PortLog.ViewModels
{
    public class ManageCrewViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;

        // CURRENT CREW
        public ObservableCollection<Account> Managers { get; } = new();
        public ObservableCollection<Account> Captains { get; } = new();

        // USERS WITHOUT COMPANY (for dropdown)
        public ObservableCollection<Account> AvailableUsers { get; } = new();

        private Account _selectedUser;
        public Account SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        // ROLE SELECTION
        public string[] Roles { get; } = { "MANAGER", "CAPTAIN" };
        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }
        private string _selectedRole = "CAPTAIN";

        // ERROR MESSAGE
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        private string _errorMessage;

        // COMMANDS
        public ICommand AddCrewCommand { get; }
        public ICommand RemoveCrewCommand { get; }

        public ManageCrewViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;

            AddCrewCommand = new RelayCommand(async _ => await AddCrew());
            RemoveCrewCommand = new RelayCommand(async id => await RemoveCrew((Guid)id));

            _ = LoadCrew();
        }

        private async Task LoadCrew()
        {
            try
            {
                Managers.Clear();
                Captains.Clear();
                AvailableUsers.Clear();

                var companyId = _accountService.LoggedInAccount.CompanyId.ToString();

                // Fetch all crew in this company
                var crewRes = await _supabase
                    .Table<Account>()
                    .Filter("company_id", Operator.Equals, companyId)
                    .Get();

                foreach (var acc in crewRes.Models)
                {
                    if (acc.Role == "MANAGER")
                        Managers.Add(acc);
                    else if (acc.Role == "CAPTAIN")
                        Captains.Add(acc);
                }

                // Fetch all users WITHOUT a company
                var availableRes = await _supabase
                    .Table<Account>()
                    .Filter("company_id", Operator.Is, (Guid?)null) // NULL company
                    .Get();

                foreach (var user in availableRes.Models)
                    AvailableUsers.Add(user);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
            }
        }

        private async Task AddCrew()
        {
            ErrorMessage = "";

            try
            {
                if (SelectedUser == null)
                {
                    ErrorMessage = "Please select a user.";
                    return;
                }

                SelectedUser.CompanyId = _accountService.LoggedInAccount.CompanyId;
                SelectedUser.Role = SelectedRole;
                SelectedUser.LastUpdated = DateTime.UtcNow;

                await _supabase.Table<Account>().Upsert(SelectedUser);

                await LoadCrew();

                SelectedUser = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to add crew: {ex.Message}";
            }
        }

        private async Task RemoveCrew(Guid id)
        {
            try
            {
                var user = await _supabase
                    .Table<Account>()
                    .Where(a => a.Id == id)
                    .Single();

                if (user == null)
                {
                    ErrorMessage = "User not found.";
                    return;
                }

                user.CompanyId = null;
                user.LastUpdated = DateTime.UtcNow;

                await _supabase.Table<Account>().Upsert(user);

                await LoadCrew();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to remove crew: {ex.Message}";
            }
        }
    }
}
