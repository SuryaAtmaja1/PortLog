using PortLog.Commands;
using PortLog.Enumerations;
using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using PortLog.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Supabase.Postgrest.Constants;

namespace PortLog.ViewModels
{
    public class VoyageChangeStateViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;

        public ObservableCollection<FleetItem> SailingShips { get; } = new();
        public ObservableCollection<FleetItem> NotSailingShips { get; } = new();

        public ICommand DetailCommand { get; }
        public ICommand StartVoyageCommand { get; }
        public ICommand EndVoyageCommand { get; }
        public ICommand ChangeStateCommand { get; }

        public void OnNavigatedTo() => _ = LoadData();

        public VoyageChangeStateViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;

            DetailCommand = new RelayCommand(ShowDetail);
            ChangeStateCommand = new RelayCommand(ChangeShipState);

            _ = LoadData();
        }

        private void ShowDetail(object shipIdObj)
        {
            if (shipIdObj is long shipId)
            {
                var vm = new DetailShipViewModel(_supabase, shipId, _accountService);
                var win = new DetailShipView
                {
                    DataContext = vm,
                    Owner = Application.Current.MainWindow
                };
                win.ShowDialog();
                _ = LoadData();
            }
        }

        public async Task LoadData()
        {
            SailingShips.Clear();
            NotSailingShips.Clear();

            var companyId = _accountService.LoggedInAccount.CompanyId;
            var accountId = _accountService.LoggedInAccount.Id;

            var query = _supabase
                .Table<Ship>()
                .Filter("company_id", Operator.Equals, companyId.ToString());

            if (_accountRole() == "captain")
            {
                query = query.Filter("captain_id", Operator.Equals, accountId.ToString());
            }

            var shipsResponse = await query.Get();
            var ships = shipsResponse.Models;

            foreach (var ship in ships)
            {
                // Captain name (best-effort)
                string captainName = "-";
                try
                {
                    var cap = await _supabase.Table<Account>()
                        .Where(a => a.Id == ship.CaptainId)
                        .Single();
                    captainName = cap?.Name ?? "-";
                }
                catch { }

                // Last voyage (most recent)
                string lastVoyage = "No Voyage";
                try
                {
                    var voyage = await _supabase
                        .Table<VoyageLog>()
                        .Where(v => v.ShipId == ship.Id)
                        .Order(v => v.DepartureTime, Ordering.Descending)
                        .Limit(1)
                        .Get();

                    var v = voyage.Models.FirstOrDefault();
                    if (v != null)
                        lastVoyage = $"{v.DeparturePort} → {v.ArrivalPort} ({v.DepartureTime:dd MMM yyyy})";
                }
                catch { }

                bool isSailing =
                    !string.IsNullOrEmpty(ship.Status) &&
                    ship.Status.Equals("SAILING", StringComparison.OrdinalIgnoreCase);
                var item = new FleetItem
                {
                    ShipId = ship.Id,
                    Name = ship.Name,
                    Type = ship.Type,
                    CaptainName = captainName,
                    LastVoyage = lastVoyage,
                    Status = ship.Status,
                    Capacity = ship.PassengerCapacity,

                };


                if (isSailing)
                    SailingShips.Add(item);
                else
                    NotSailingShips.Add(item);
            }
        }


        private async void ChangeShipState(object shipIdObj)
        {
            if (shipIdObj is not long shipId)
                return;

            var resp = await _supabase
                .Table<Ship>()
                .Where(x => x.Id == shipId)
                .Get();

            var ship = resp.Models.FirstOrDefault();

            if (ship == null)
            {
                MessageBox.Show("Ship not found.");
                return;
            }

            string shipName = ship.Name ?? "Unknown";

            // 2. Tentukan apakah buka Start atau End Sailing dialog
            bool isSailing = ship.Status?.Equals("SAILING", StringComparison.OrdinalIgnoreCase) == true;

            if (isSailing)
            {
                // END SAILING
                var win = new EndSailingDialogView(_supabase, ship.Id, shipName)
                {
                    Owner = Application.Current.MainWindow
                };

                if (win.ShowDialog() == true)
                    _ = LoadData();
            }
            else
            {
                // START SAILING
                var win = new StartSailingDialogView(_supabase, ship.Id, shipName)
                {
                    Owner = Application.Current.MainWindow
                };

                if (win.ShowDialog() == true)
                    _ = LoadData();
            }
        }


        private string _accountRole()
        {
            return _accountService.LoggedInAccount?.Role?.ToLowerInvariant() ?? string.Empty;
        }
    }
}
