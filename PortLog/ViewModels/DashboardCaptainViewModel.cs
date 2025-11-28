using PortLog.Commands;
using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using PortLog.Views;
using Supabase.Postgrest;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using static Supabase.Postgrest.Constants;

namespace PortLog.ViewModels
{
    // gunakan nama berbeda agar tidak duplikat dengan kelas lain
    public class CaptainFleetItem
    {
        public long ShipId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string LastVoyage { get; set; }
        public string Status { get; set; }
        public int Capacity { get; set; }
    }

    public class DashboardCaptainViewModel : BaseViewModel
    {
        private readonly PortLog.Services.NavigationService _navigationService;
        private readonly AccountService _accountService;
        private readonly CompanyService _companyService;
        private readonly SupabaseService _supabase;

        public DashboardCaptainViewModel(PortLog.Services.NavigationService navigationService, AccountService accountService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

            if (navigationService.MainViewModel is MainViewModel mainVm)
            {
                _companyService = mainVm.CompanyService;
                _supabase = mainVm.SupabaseService;
            }

            LogoutCommand = new RelayCommand(Logout);
            DetailCommand = new RelayCommand(OpenDetail);

            // start loading
            //_ = LoadAllAsync();
        }

        // --------- Bindable props ----------
        private string _companyInfo = "Loading...";
        public string CompanyInfo
        {
            get => _companyInfo;
            private set => SetProperty(ref _companyInfo, value);
        }

        public string WelcomeMessage => $"Welcome, {_accountService.LoggedInAccount?.Name ?? "User"} (CAPTAIN)";

        private int _captainShipCount;
        public int CaptainShipCount
        {
            get => _captainShipCount;
            private set => SetProperty(ref _captainShipCount, value);
        }

        public ObservableCollection<CaptainFleetItem> CaptainShips { get; } = new();

        public ICommand LogoutCommand { get; }
        public ICommand DetailCommand { get; }

        // --------- Loaders ----------
        public void OnNavigatedTo() => _ = LoadAllAsync();

        private async Task LoadAllAsync()
        {
            await LoadCompanyInfoAsync();
            await LoadCaptainShipsAsync();
        }

        public async Task LoadCompanyInfoAsync()
        {
            try
            {
                if (_accountService.LoggedInAccount?.CompanyId != null)
                {
                    var company = await _companyService.GetCompanyByIdAsync(_accountService.LoggedInAccount.CompanyId.Value);
                    CompanyInfo = company != null ? $"Company: {company.Name}" : "Tidak terafiliasi";
                    Debug.WriteLine("memek:",company);
                }
                else
                {
                    CompanyInfo = "Tidak terafiliasi";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadCompanyInfoAsync error: " + ex.Message);
                CompanyInfo = "Gagal memuat info perusahaan";
            }
        }

        /// <summary>
        /// Query Supabase: SELECT * FROM ship WHERE captain_id = <loggedUser.Id>
        /// lalu set CaptainShipCount dan isi CaptainShips (opsional untuk daftar).
        /// </summary>
        public async Task LoadCaptainShipsAsync()
        {
            try
            {
                CaptainShips.Clear();
                CaptainShipCount = 0;

                if (_supabase == null || _accountService.LoggedInAccount == null)
                    return;

                var loggedId = _accountService.LoggedInAccount.Id;
                var captainIdStr = loggedId.ToString();

                // ambil kapal untuk captain ini
                var shipsRes = await _supabase
                    .Table<Ship>()
                    .Filter("captain_id", Operator.Equals, captainIdStr)
                    .Order("id", Ordering.Descending)
                    .Get();

                var ships = shipsRes.Models;
                if (ships == null || !ships.Any())
                {
                    CaptainShipCount = 0;
                    return;
                }

                // ambil last voyage tiap kapal (opsional)
                foreach (var ship in ships)
                {
                    string lastVoyageText = "No Voyage";
                    try
                    {
                        var voyageRes = await _supabase
                            .Table<VoyageLog>()
                            .Filter("ship_id", Operator.Equals, ship.Id.ToString())
                            .Order("departure_time", Ordering.Descending)
                            .Limit(1)
                            .Get();

                        var last = voyageRes.Models.FirstOrDefault();
                        if (last != null)
                            lastVoyageText = $"{last.DeparturePort} → {last.ArrivalPort} ({last.DepartureTime:dd MMM yyyy})";
                    }
                    catch { /* ignore last voyage errors */ }

                    CaptainShips.Add(new CaptainFleetItem
                    {
                        ShipId = ship.Id,
                        Name = ship.Name,
                        Type = ship.Type,
                        Status = ship.Status,
                        Capacity = ship.PassengerCapacity,
                        LastVoyage = lastVoyageText
                    });
                }

                CaptainShipCount = CaptainShips.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadCaptainShipsAsync error: " + ex.Message);
                CaptainShips.Clear();
                CaptainShipCount = 0;
            }
        }

        // ------- Commands ----------
        private void OpenDetail(object parameter)
        {
            if (parameter is long shipId)
            {
                var detailVm = new DetailShipViewModel(_supabase, shipId, _accountService);
                detailVm.DataChanged += async () => await LoadCaptainShipsAsync();

                var window = new DetailShipView
                {
                    DataContext = detailVm,
                    Owner = Application.Current.MainWindow
                };
                window.ShowDialog();
            }
        }

        private void Logout(object parameter)
        {
            try { _accountService.Logout(); } catch { }

            // pakai navigation service milik projectmu
            _navigationService.NavigateTo(new LoginViewModel(_navigationService, _accountService));
        }
    }
}
