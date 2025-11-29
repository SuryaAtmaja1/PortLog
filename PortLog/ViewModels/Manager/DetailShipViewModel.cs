using PortLog.Commands;
using PortLog.Enumerations;
using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static Supabase.Postgrest.Constants;

namespace PortLog.ViewModels
{
    public class DetailShipViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase;
        private readonly ShipService _shipService;
        private readonly VoyageService _voyageService;
        private readonly AccountService _accountService;
        public long ShipId { get; }

        // READ-ONLY Properties
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string CaptainName { get; private set; }
        public string Status { get; private set; }
        public int Capacity { get; private set; }
        public string LastVoyage { get; private set; }

        // EDITABLE FIELDS
        public string EditName { get => _editName; set => SetProperty(ref _editName, value); }
        private string _editName;

        public string EditType { get => _editType; set => SetProperty(ref _editType, value); }
        private string _editType;

        public int EditCapacity { get => _editCapacity; set => SetProperty(ref _editCapacity, value); }
        private int _editCapacity;

        public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }
        private bool _isEditing;

        public ObservableCollection<Account> Captains { get; } = new();

        private Account _editCaptain;
        public Account EditCaptain
        {
            get => _editCaptain;
            set => SetProperty(ref _editCaptain, value);
        }

        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public IEnumerable<string> ShipTypes { get; } =
             Enum.GetNames(typeof(ShipType));

        public event Action DataChanged;

        private Ship _ship;
        public Guid CaptainId => _ship?.CaptainId ?? Guid.Empty;

        // NEW: expose whether current user can edit (manager)
        public bool CanEdit { get; }

        public DetailShipViewModel(SupabaseService supabase, long shipId, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;
            _shipService = new ShipService(supabase);
            _voyageService = new VoyageService(supabase);
            ShipId = shipId;

            // determine permission: manager can edit, captain cannot
            var role = _accountService.LoggedInAccount?.Role?.ToLowerInvariant() ?? string.Empty;
            CanEdit = role == "manager" || role == "admin"; // tambahkan "admin" jika ada role lain yang boleh edit

            // Commands: if RelayCommand supports canExecute predicate, we pass it.
            // If not, internal checks in methods will still prevent edit when CanEdit == false.
            try
            {
                EditCommand = new RelayCommand(_ => EnterEditMode(), _ => CanEdit);
                SaveCommand = new RelayCommand(async _ => await Save(), _ => CanEdit);
                CancelCommand = new RelayCommand(_ => Cancel(), _ => CanEdit);
            }
            catch
            {
                // Fallback: RelayCommand mungkin tidak mendukung predicate
                EditCommand = new RelayCommand(_ => EnterEditMode());
                SaveCommand = new RelayCommand(async _ => await Save());
                CancelCommand = new RelayCommand(_ => Cancel());
            }

            IsEditing = false;

            _ = LoadData();
            // only load captains if user can edit (manager)
            if (CanEdit)
                _ = LoadCaptains();
        }

        private async Task LoadData()
        {
            var shipResp = await _shipService.GetShipByIdAsync(ShipId);

            _ship = shipResp;

            Name = _ship.Name;
            Type = _ship.Type;
            Capacity = _ship.PassengerCapacity;
            Status = _ship.Status;

            // Preload editable values
            EditName = Name;
            EditType = Type;
            EditCapacity = Capacity;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(Capacity));
            OnPropertyChanged(nameof(Status));

            // Captain
            var captainResp = await _accountService.GetCaptainByIdAsync(_ship.CaptainId);

            CaptainName = captainResp?.Name ?? "-";
            OnPropertyChanged(nameof(CaptainName));

            // Last voyage
            var voyage = await _voyageService.GetLatestVoyageForShipAsync(ShipId);

            LastVoyage = voyage != null
                ? $"{voyage.DeparturePort} → {voyage.ArrivalPort}"
                : "No voyage";

            OnPropertyChanged(nameof(LastVoyage));
        }

        private void EnterEditMode()
        {
            // Safety: guard, jika tidak punya hak edit jangan masuk mode edit
            if (!CanEdit) return;

            IsEditing = true;
        }

        private void Cancel()
        {
            // Only cancel if editing
            IsEditing = false;

            EditName = Name;
            EditType = Type;
            EditCapacity = Capacity;
        }

        private async Task Save()
        {
            // guard: only managers can save
            if (!CanEdit) return;

            if (EditCaptain == null)
                return;

            _ship.Name = EditName;
            _ship.Type = EditType;
            _ship.PassengerCapacity = EditCapacity;

            _ship.CaptainId = EditCaptain.Id;

            await _supabase.Table<Ship>().Update(_ship);

            // sync UI
            Name = _ship.Name;
            Type = _ship.Type;
            Capacity = _ship.PassengerCapacity;
            CaptainName = EditCaptain.Name;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(Capacity));
            OnPropertyChanged(nameof(CaptainName));

            IsEditing = false;

            DataChanged?.Invoke();
        }


        private async Task LoadCaptains()
        {
            // Only load captains list if can edit (manager)
            if (!CanEdit) return;

            Captains.Clear();

            var res = await _accountService.GetCaptainsByCompanyIdAsync(
                _accountService.LoggedInAccount.CompanyId.Value);

            foreach (var c in res)
                Captains.Add(c);

            EditCaptain = Captains.FirstOrDefault(c => c.Id == CaptainId);
        }
    }
}
