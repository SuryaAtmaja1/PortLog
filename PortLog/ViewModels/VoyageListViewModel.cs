using PortLog.Commands;
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
    public class VoyageListItem
    {
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string DeparturePort { get; set; }
        public string ArrivalPort { get; set; }
        public string ShipName { get; set; }
        public decimal Revenue { get; set; }
    }

    public class VoyageListViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;
        private readonly ShipService _shipService;
        private readonly VoyageService _voyageService;

        public ObservableCollection<VoyageListItem> Voyages { get; set; } = new();

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public ICommand SearchCommand { get; }

        public VoyageListViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;
            _shipService = new ShipService(supabase);
            _voyageService = new VoyageService(supabase);

            SearchCommand = new RelayCommand(async _ => await LoadVoyages());
        }

        public void OnNavigatedTo() => _ = LoadVoyages();

        private async Task LoadVoyages()
        {
            Voyages.Clear();

            // Ambil semua voyage milik kapal company user
            var companyId = _accountService.LoggedInAccount.CompanyId.Value;

            var ships = await _shipService.GetShipsByCompanyIdAsync(companyId);

            foreach (var ship in ships)
            {
                var voyages = await _voyageService.GetVoyagesByShipAsync(
                    ship.Id,
                    StartDate,
                    EndDate
                );

                foreach (var v in voyages)
                {
                    Voyages.Add(new VoyageListItem
                    {
                        DepartureTime = v.DepartureTime,
                        ArrivalTime = v.ArrivalTime,
                        DeparturePort = v.DeparturePort,
                        ArrivalPort = v.ArrivalPort,
                        ShipName = ship.Name,
                        Revenue = (v.Revenue ?? 0) / 100m
                    });
                }
            }
        }
    }
}
