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
        private readonly VoyageService _voyageService;

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
            _voyageService = new VoyageService(supabase);

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

                // GET SHIPS
                var ships = await _shipService.GetShipsByCompanyIdAsync(companyId);
                var shipIds = ships.Select(s => (object)s.Id).ToArray();

                // CARD: CURRENT SAILING
                CurrentSailingShips = ships.Count(s => s.Status == "SAILING");

                // FLEET PREVIEW
                FleetPreview.Clear();
                var fiveShips = ships.OrderByDescending(s => s.Id).Take(5);

                foreach (var ship in fiveShips)
                {
                    var v = await _voyageService.GetLatestVoyageForShipAsync(ship.Id);

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

                // WEEKLY VOYAGES & REVENUE
                WeeklyVoyages = 0;
                WeeklyRevenueDisplay = "Rp 0";

                if (shipIds.Any())
                {
                    var weeklyVoyages = await _voyageService.GetVoyagesInLast7DaysAsync(shipIds);

                    WeeklyVoyages = weeklyVoyages.Count;

                    decimal revenue = weeklyVoyages.Sum(v => v.RevenueIdr);
                    WeeklyRevenueDisplay = $"Rp {revenue:N0}";
                }

                // LATEST COMPLETED VOYAGE
                LatestVoyages.Clear();

                if (shipIds.Any())
                {
                    var last = await _voyageService.GetLatestCompletedVoyage(shipIds);

                    if (last != null)
                    {
                        if (last.ArrivalPort == null)
                            last.ArrivalPort = "TBA";
                    
                        var ship = await _shipService.GetShipByIdAsync(last.ShipId);

                        LatestVoyages.Add(new DashboardVoyageItem
                        {
                            Route = $"{last.DeparturePort} → {last.ArrivalPort}",
                            Date = last.ArrivalTime.ToString(),
                            Revenue = last.RevenueIdr,
                            Name = ship.Name
                        });
                    }
                }

                // SUMMARY INSIGHT
                var start = DateTime.UtcNow.AddDays(-7);
                var end = DateTime.UtcNow;

                if (!shipIds.Any())
                {
                    SummaryInsight = new Insight(start, end, 0, TimeSpan.Zero, 0, 0, 0);
                    return;
                }

                var insightVoyages = await _voyageService.GetVoyagesInRangeAsync(shipIds, start, end);

                if (!insightVoyages.Any())
                {
                    SummaryInsight = new Insight(start, end, 0, TimeSpan.Zero, 0, 0, 0);
                }
                else
                {
                    int totalTrips = insightVoyages.Count;
                    var totalHours = TimeSpan.FromHours(insightVoyages.Sum(v => (v.ArrivalTime - v.DepartureTime).Value.Hours));
                    float totalDistance = (float)insightVoyages.Sum(v => v.TotalDistanceTraveled);
                    float avgSpeed = totalHours.TotalHours > 0 ? totalDistance / (float)totalHours.TotalHours : 0;
                    float avgFuel = (float)insightVoyages.Average(v => v.AverageFuelConsumption);

                    SummaryInsight = new Insight(start, end, totalTrips, totalHours, totalDistance, avgSpeed, avgFuel);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("🔥 DASHBOARD ERROR:");
                Debug.WriteLine(ex.Message);
                if (ex.InnerException != null)
                    Debug.WriteLine("🔥 INNER: " + ex.InnerException.Message);
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
