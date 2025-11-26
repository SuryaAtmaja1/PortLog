using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace PortLog.ViewModels
{
    public class DashboardHomeViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;

        // TOP CARDS
        public int CurrentSailingShips
        {
            get => _currentSailingShips;
            set => SetProperty(ref _currentSailingShips, value);
        }
        private int _currentSailingShips;

        public string WeeklyRevenueDisplay
        {
            get => _weeklyRevenueDisplay;
            set => SetProperty(ref _weeklyRevenueDisplay, value);
        }
        private string _weeklyRevenueDisplay;

        public int WeeklyVoyages
        {
            get => _weeklyVoyages;
            set => SetProperty(ref _weeklyVoyages, value);
        }
        private int _weeklyVoyages;

        // LISTS
        public ObservableCollection<DashboardFleetItem> FleetPreview { get; } = new();
        public ObservableCollection<DashboardVoyageItem> LatestVoyages { get; } = new();

        public DashboardHomeViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;

            _ = LoadDashboard();
        }

        public void OnNavigatedTo() => _ = LoadDashboard();


        private async Task LoadDashboard()
        {
            try
            {
                var companyId = _accountService.LoggedInAccount.CompanyId.ToString();

                // ====================================================
                // 1. AMBIL SEMUA SHIP MILIK COMPANY
                // ====================================================
                var shipsRes = await _supabase
                    .Table<Ship>()
                    .Filter("company_id", Operator.Equals, companyId)
                    .Get();

                var ships = shipsRes.Models;
                var shipIds = ships.Select(s => s.Id).ToList();

                // ====================================================
                // 2. CURRENT SAILING SHIPS
                // ====================================================
                CurrentSailingShips = ships.Count(s => s.Status == "SAILING");

                // ====================================================
                // 3. FLEET PREVIEW (5 terbaru)
                // ====================================================
                FleetPreview.Clear();

                var fiveShips = ships
                    .OrderByDescending(s => s.Id)
                    .Take(5)
                    .ToList();

                foreach (var ship in fiveShips)
                {
                    // last voyage per ship
                    var lastVoyage = await _supabase
                        .Table<VoyageLog>()
                        .Filter("ship_id", Operator.Equals, ship.Id.ToString())
                        .Order("departure_time", Ordering.Descending)
                        .Limit(1)
                        .Get();

                    var v = lastVoyage.Models.FirstOrDefault();

                    FleetPreview.Add(new DashboardFleetItem
                    {
                        Name = ship.Name,
                        Status = ship.Status,
                        Type = ship.Type,
                        LastVoyage = v != null
                            ? $"{v.DeparturePort} → {v.ArrivalPort} ({v.DepartureTime:dd MMM})"
                            : "No voyage"
                    });
                }

                // ====================================================
                // 4. WEEKLY VOYAGE COUNT + REVENUE
                // ====================================================
                WeeklyVoyages = 0;
                WeeklyRevenueDisplay = "Rp 0";

                if (shipIds.Any())
                {
                    string shipFilter = "(" + string.Join(",", shipIds) + ")";
                    DateTime sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

                    var weeklyVoyagesRes = await _supabase
                        .Table<VoyageLog>()
                        .Filter("ship_id", Operator.In, shipIds.Cast<object>().ToArray())
                        .Filter("arrival_time", Operator.GreaterThanOrEqual, sevenDaysAgo.ToString("o"))
                        .Order("arrival_time", Ordering.Descending)
                        .Get();

                    var weeklyVoyagesList = weeklyVoyagesRes.Models;

                    WeeklyVoyages = weeklyVoyagesList.Count;

                    decimal revenue = weeklyVoyagesList.Sum(v => v.RevenueIdr);
                    WeeklyRevenueDisplay = $"Rp {revenue:N0}";
                }

                // ====================================================
                // 5. LATEST COMPLETED VOYAGES (5 terakhir)
                // ====================================================
                LatestVoyages.Clear();

                if (shipIds.Any())
                {
                    string shipFilter = "(" + string.Join(",", shipIds) + ")";

                    var latestRes = await _supabase
                        .Table<VoyageLog>()
                        .Filter("ship_id", Operator.In, shipIds.Cast<object>().ToArray())
                        .Order("arrival_time", Ordering.Descending)
                        .Limit(5)
                        .Get();

                    foreach (var v in latestRes.Models)
                    {
                        LatestVoyages.Add(new DashboardVoyageItem
                        {
                            Route = $"{v.DeparturePort} → {v.ArrivalPort}",
                            Date = v.ArrivalTime.ToString("dd MMM yyyy"),
                            Revenue = v.RevenueIdr
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("🔥 DASHBOARD ERROR:");
                System.Diagnostics.Debug.WriteLine(ex.Message);

                if (ex.InnerException != null)
                    System.Diagnostics.Debug.WriteLine("🔥 INNER: " + ex.InnerException.Message);
            }
        }
    }

    // ================= MODELS ==================

    public class DashboardFleetItem
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string LastVoyage { get; set; }
    }

    public class DashboardVoyageItem
    {
        public string Route { get; set; }
        public string Date { get; set; }
        public decimal Revenue { get; set; }

        public string RevenueFormatted => $"Rp {Revenue:N0}";
    }
}
