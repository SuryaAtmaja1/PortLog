using PortLog.Commands;
using PortLog.Enumerations;
using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static Supabase.Postgrest.Constants;
using System.Diagnostics;

namespace PortLog.ViewModels
{
    public class AddShipViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;
        private readonly ShipService _shipService;

        // SHIP TYPES
        public ObservableCollection<string> ShipTypes { get; }
            = new(Enum.GetNames(typeof(ShipType)));

        // CAPTAINS LIST
        public ObservableCollection<Account> Captains { get; }
            = new();

        public Account SelectedCaptain
        {
            get => _selectedCaptain;
            set => SetProperty(ref _selectedCaptain, value);
        }
        private Account _selectedCaptain;

        // INPUT FIELDS
        public string NewName { get => _newName; set => SetProperty(ref _newName, value); }
        private string _newName;

        public string NewType { get => _newType; set => SetProperty(ref _newType, value); }
        private string _newType = "MULTI_PURPOSE_VESSEL";

        public string NewCapacity { get => _newCapacity; set => SetProperty(ref _newCapacity, value); }
        private string _newCapacity;

        private bool _isSaving = false;

        public ICommand SaveCommand { get; }
        public Action CloseAction { get; set; }

        public AddShipViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;
            _shipService = new ShipService(supabase);

            SaveCommand = new RelayCommand(async _ => await Save());
            _ = LoadCaptains();
        }

        private async Task LoadCaptains()
        {
            Captains.Clear();

            var res = await _accountService.GetCaptainsByCompanyIdAsync(
                _accountService.LoggedInAccount.CompanyId.Value);

            foreach (var c in res)
                Captains.Add(c);

            if (Captains.Count > 0)
                SelectedCaptain = Captains[0];
        }

        private async Task Save()
        {
            if (_isSaving) return;
            _isSaving = true;

            try
            {
                if (string.IsNullOrWhiteSpace(NewName) ||
                    string.IsNullOrWhiteSpace(NewType) ||
                    string.IsNullOrWhiteSpace(NewCapacity))
                    return;

                var ship = new Ship
                {
                    CompanyId = _accountService.LoggedInAccount.CompanyId.Value,
                    Name = NewName,
                    Type = NewType,
                    PassengerCapacity = int.Parse(NewCapacity),
                    Status = "STANDBY",
                    CaptainId = SelectedCaptain?.Id ?? Guid.Empty
                };

                await _shipService.AddShipAsync(ship);

                CloseAction?.Invoke();
            }
            finally
            {
                    _isSaving = false;
            }
        }
    }

}
