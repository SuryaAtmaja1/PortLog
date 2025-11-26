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

            SearchCommand = new RelayCommand(async _ => await LoadVoyages());
        }

        private async Task LoadVoyages()
        {
            Voyages.Clear();

            // Ambil semua voyage milik kapal company user
            var companyId = _accountService.LoggedInAccount.CompanyId.ToString();

            var shipsResponse = await _supabase
                .Table<Ship>()
                .Filter("company_id", Operator.Equals, companyId)
                .Get();

            var ships = shipsResponse.Models;

            foreach (var ship in ships)
            {
                // Query voyage berdasarkan rentang tanggal
                var query = _supabase
                    .Table<VoyageLog>()
                    .Filter("ship_id", Operator.Equals, ship.Id.ToString());

                if (StartDate.HasValue)
                    query = query.Filter("departure_time", Operator.GreaterThanOrEqual, StartDate.Value.ToString("yyyy-MM-dd"));

                if (EndDate.HasValue)
                    query = query.Filter("arrival_time", Operator.LessThanOrEqual, EndDate.Value.ToString("yyyy-MM-dd"));

                var voyagesResponse = await query.Get();

                foreach (var v in voyagesResponse.Models)
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
