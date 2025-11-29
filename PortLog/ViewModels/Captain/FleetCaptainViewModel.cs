using PortLog.Commands;
using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using PortLog.Views;
using Supabase.Postgrest;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Supabase.Postgrest.Constants;

namespace PortLog.ViewModels
{
    public class FleetCaptainViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;

        public ObservableCollection<FleetItem> Fleets { get; } = new();

        public ICommand DetailCommand { get; }

        public void OnNavigatedTo() => _ = LoadData();

        public FleetCaptainViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;

            DetailCommand = new RelayCommand(ShowDetail);

            // load when constructed (or you can call OnNavigatedTo externally)
            _ = LoadData();
        }

        private void ShowDetail(object shipIdObj)
        {
            if (shipIdObj is long shipId)
            {
                var detailVM = new DetailShipViewModel(_supabase, shipId, _accountService);

                var window = new DetailShipView
                {
                    DataContext = detailVM,
                    Owner = Application.Current.MainWindow
                };

                window.ShowDialog();
            }
        }

        public async Task LoadData()
        {
            Fleets.Clear();

            var captainId = _accountService.LoggedInAccount.Id;

            // Ambil kapal yang captain_id == captainId
            var shipsResponse = await _supabase
                .Table<Ship>()
                .Filter("captain_id", Operator.Equals, captainId.ToString())
                .Get();

            var ships = shipsResponse.Models;

            foreach (var ship in ships)
            {
                // captain details: bisa pakai accountService (sudah diketahui)
                var captain = _accountService.LoggedInAccount;

                // last voyage
                var voyagesResponse = await _supabase
                    .Table<VoyageLog>()
                    .Where(v => v.ShipId == ship.Id)
                    .Order(v => v.DepartureTime, Ordering.Descending)
                    .Limit(1)
                    .Get();

                var lastVoyage = voyagesResponse.Models.FirstOrDefault();

                Fleets.Add(new FleetItem
                {
                    ShipId = ship.Id,
                    Name = ship.Name,
                    Type = ship.Type,
                    CaptainName = captain?.Name ?? "-",
                    LastVoyage = lastVoyage != null
                        ? $"{lastVoyage.DeparturePort} → {lastVoyage.ArrivalPort} ({lastVoyage.DepartureTime:dd MMM yyyy})"
                        : "No Voyage",
                    Status = ship.Status,
                    Capacity = ship.PassengerCapacity
                });
            }
        }
    }
}
