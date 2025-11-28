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

        public DetailShipViewModel(SupabaseService supabase, long shipId, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;
            ShipId = shipId;

            EditCommand = new RelayCommand(_ => EnterEditMode());
            SaveCommand = new RelayCommand(async _ => await Save());
            CancelCommand = new RelayCommand(_ => Cancel());
                
            IsEditing = false;

            _ = LoadData();
            _ = LoadCaptains();
        }

        private async Task LoadData()
        {
            var shipResp = await _supabase
                .Table<Ship>()
                .Where(s => s.Id == ShipId)
                .Single();

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
            var captainResp = await _supabase
                .Table<Account>()
                .Where(a => a.Id == _ship.CaptainId)
                .Single();

            CaptainName = captainResp?.Name ?? "-";
            OnPropertyChanged(nameof(CaptainName));

            // Last voyage
            var voyageResp = await _supabase
                .Table<VoyageLog>()
                .Filter("ship_id", Operator.Equals, ShipId.ToString())
                .Order("departure_time", Ordering.Descending)
                .Limit(1)
                .Get();

            if (voyageResp.Models.Any())
            {
                var v = voyageResp.Models[0];
                LastVoyage = $"{v.DeparturePort} → {v.ArrivalPort}";
            }
            else LastVoyage = "No voyage";

            OnPropertyChanged(nameof(LastVoyage));
        }

        private void EnterEditMode()
        {
            IsEditing = true;
        }

        private void Cancel()
        {
            IsEditing = false;

            EditName = Name;
            EditType = Type;
            EditCapacity = Capacity;
        }

        private async Task Save()
        {
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
            Captains.Clear();

            var res = await _supabase.Table<Account>()
                .Filter("company_id", Operator.Equals, _accountService.LoggedInAccount.CompanyId.ToString())
                .Filter("account_role", Operator.Equals, "CAPTAIN")
                .Get();

            foreach (var c in res.Models)
                Captains.Add(c);

            EditCaptain = Captains.FirstOrDefault(c => c.Id == CaptainId);
        }
    }

}
