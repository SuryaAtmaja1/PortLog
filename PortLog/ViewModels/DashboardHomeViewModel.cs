using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace PortLog.ViewModels
{
    public class DashboardHomeViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;
        private readonly ShipService _shipService;

        // ==================== TOP CARDS ====================

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

        // ==================== LISTS ====================

        public ObservableCollection<DashboardFleetItem> FleetPreview { get; } = new();
        public ObservableCollection<DashboardVoyageItem> LatestVoyages { get; } = new();

        // ==================== SUMMARY INSIGHT ====================

        public Insight SummaryInsight
        {
            get => _summaryInsight;
            set => SetProperty(ref _summaryInsight, value);
        }
        private Insight _summaryInsight;

        // ==================== INIT ====================

        public DashboardHomeViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;
            _shipService = new ShipService(supabase);

            _ = LoadDashboard();
        }

        public void OnNavigatedTo() => _ = LoadDashboard();



        // ==========================================================
        // MAIN DASHBOARD LOADER
        // ==========================================================

        private async Task LoadDashboard()
        {
            try
            {
                var companyId = _accountService.LoggedInAccount.CompanyId.Value;

                // ===================== GET SHIPS =====================
                var ships = await _shipService.GetShipsByCompanyIdAsync(companyId);

                var shipIds = ships.Select(s => (object)s.Id).ToArray();

                // ===================== CARD 1: CURRENT SAILING =====================

                CurrentSailingShips = ships.Count(s => s.Status == "SAILING");


                // ===================== FLEET PREVIEW: 5 TERBARU =====================

                FleetPreview.Clear();

                var fiveShips = ships
                    .OrderByDescending(s => s.Id)
                    .Take(5)
                    .ToList();

                foreach (var ship in fiveShips)
                {
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


                // ===================== WEEKLY VOYAGES & REVENUE =====================

                WeeklyVoyages = 0;
                WeeklyRevenueDisplay = "Rp 0";

                if (shipIds.Any())
                {
                    DateTime sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

                    var weeklyVoyagesRes = await _supabase
                        .Table<VoyageLog>()
                        .Filter("ship_id", Operator.In, shipIds)
                        .Filter("arrival_time", Operator.GreaterThanOrEqual, sevenDaysAgo.ToString("o"))
                        .Order("arrival_time", Ordering.Descending)
                        .Get();

                    var weeklyVoyagesList = weeklyVoyagesRes.Models;

                    WeeklyVoyages = weeklyVoyagesList.Count;

                    decimal revenue = weeklyVoyagesList.Sum(v => v.RevenueIdr);
                    WeeklyRevenueDisplay = $"Rp {revenue:N0}";
                }


                // ===================== LATEST COMPLETED VOYAGES =====================

                LatestVoyages.Clear();

                if (shipIds.Any())
                {
                    var latestRes = await _supabase
                        .Table<VoyageLog>()
                        .Filter("ship_id", Operator.In, shipIds)
                        .Order("arrival_time", Ordering.Descending)
                        .Limit(1)
                        .Get();

                    foreach (var v in latestRes.Models)
                    {
                        var ship = await _shipService.GetShipByIdAsync(v.ShipId);
                        LatestVoyages.Add(new DashboardVoyageItem
                        {
                            Route = $"{v.DeparturePort} → {v.ArrivalPort}",
                            Date = v.ArrivalTime.ToString(),
                            Revenue = v.RevenueIdr,
                            Name = ship.Name,
                        });
                    }
                }


                // ===================== SUMMARY INSIGHT: LAST 7 DAYS =====================

                if (!shipIds.Any())
                {
                    SummaryInsight = new Insight(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, 0, TimeSpan.Zero, 0, 0, 0);
                    return;
                }

                var start = DateTime.UtcNow.AddDays(-7);
                var end = DateTime.UtcNow;

                var insightRes = await _supabase
                    .Table<VoyageLog>()
                    .Filter("ship_id", Operator.In, shipIds)
                    .Filter("departure_time", Operator.GreaterThanOrEqual, start.ToString("o"))
                    .Filter("arrival_time", Operator.LessThanOrEqual, end.ToString("o"))
                    .Get();

                var voyages = insightRes.Models;

                if (!voyages.Any())
                {
                    SummaryInsight = new Insight(start, end, 0, TimeSpan.Zero, 0, 0, 0);
                }
                else
                {
                    int totalTrips = voyages.Count;

                    var totalHours = TimeSpan.FromHours(
                        voyages.Sum(v => (v.ArrivalTime - v.DepartureTime).Value.Hours)
                    );

                    float totalDistance = (float)voyages.Sum(v => v.TotalDistanceTraveled);

                    float avgSpeed = totalHours.TotalHours > 0
                        ? totalDistance / (float)totalHours.TotalHours
                        : 0;

                    float avgFuel = (float)voyages.Average(v => v.AverageFuelConsumption);

                    SummaryInsight = new Insight(start, end, totalTrips, totalHours, totalDistance, avgSpeed, avgFuel);
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



    // =============== MODELS ===============

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
        public string Name { get; set; }
        public string RevenueFormatted => $"Rp {Revenue:N0}";
    }
}
