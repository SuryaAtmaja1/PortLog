using PortLog.Commands;
using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using PortLog.Views;
using Supabase.Postgrest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using static Supabase.Postgrest.Constants;

namespace PortLog.ViewModels
{
    public class FleetItem
    {
        public long ShipId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string CaptainName { get; set; }
        public string LastVoyage { get; set; }
        public string Status { get; set; }

        public int Capacity { get; set; }
    }


    public class FleetViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;
        private readonly ShipService _shipService;
        private readonly VoyageService _voyageService;

        public ObservableCollection<FleetItem> Fleets { get; } = new();

        public ICommand DetailCommand { get; }

        public ICommand OpenAddShipCommand { get; }

        public void OnNavigatedTo() => _ = LoadData();

        public FleetViewModel(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;
            _shipService = new ShipService(supabase);
            _voyageService = new VoyageService(supabase);

            //_ = LoadData(); 

            DetailCommand = new RelayCommand(ShowDetail);
            OpenAddShipCommand = new RelayCommand(_ => OpenAddShip());
        }
        private void ShowDetail(object shipIdObj)
        {
            if (shipIdObj is long shipId)
            {
                var detailVM = new DetailShipViewModel(_supabase, shipId, _accountService);

                // listen to data changed
                detailVM.DataChanged += async () =>
                {
                    await LoadData();   // REFRESH fleet table
                };

                var window = new DetailShipView
                {
                    DataContext = detailVM,
                    Owner = Application.Current.MainWindow
                };

                window.ShowDialog();
            }
        }
        private void OpenAddShip()
        {
            var win = new AddShipView
            {
                DataContext = new AddShipViewModel(_supabase, _accountService),
                Owner = Application.Current.MainWindow
            };

            win.ShowDialog();

            _ = LoadData(); // refresh
        }

        private async Task LoadData()
        {
            Fleets.Clear();

            var ships = await _shipService.GetShipsByCompanyIdAsync(_accountService.LoggedInAccount.CompanyId.Value);

            foreach (var ship in ships)
            {
                var captain = await _accountService.GetCaptainByIdAsync(ship.CaptainId);

                var lastVoyage = await _voyageService.GetLatestVoyageForShipAsync(ship.Id);

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
