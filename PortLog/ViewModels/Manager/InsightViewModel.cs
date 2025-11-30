using PortLog.Commands;
using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static Supabase.Postgrest.Constants;

namespace PortLog.ViewModels
{
    public class InsightViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;
        private readonly VoyageService _voyageService;

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }
        private DateTime _startDate = DateTime.UtcNow.AddMonths(-1);

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }
        private DateTime _endDate = DateTime.UtcNow;

        public Insight CurrentInsight
        {
            get => _currentInsight;
            set => SetProperty(ref _currentInsight, value);
        }
        private Insight _currentInsight;

        public ICommand SearchCommand { get; }

        public InsightViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;
            _voyageService = new VoyageService(supabase, accountService);

            SearchCommand = new RelayCommand(async _ => await LoadInsights());

            _ = LoadInsights(); // initial load
        }

        public void OnNavigatedTo() => _ = LoadInsights();

        public async Task LoadInsights()
        {
            try
            {
                var start = StartDate.Date;
                var end = EndDate.Date.AddDays(1);

                var voyages = await _voyageService.GetVoyagesByDateRangeAsync(start, end);

                if (!voyages.Any())
                {
                    CurrentInsight = new Insight(start, end, 0, TimeSpan.Zero, 0, 0, 0);
                    return;
                }

                int totalTrips = voyages.Count;

                var totalHours = TimeSpan.FromHours(
                    voyages.Sum(v => (v.ArrivalTime - v.DepartureTime).Value.TotalHours)
                );

                float totalDistance = (float)voyages.Sum(v => v.TotalDistanceTraveled);

                float avgSpeed = totalHours.TotalHours > 0
                    ? totalDistance / (float)totalHours.TotalHours
                    : 0;

                float avgFuel = (float)voyages.Average(v => v.AverageFuelConsumption);

                CurrentInsight = new Insight(
                    start,
                    end,
                    totalTrips,
                    totalHours,
                    totalDistance,
                    avgSpeed,
                    avgFuel
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("INSIGHT ERROR: " + ex.Message);
            }
        }
    }
}
