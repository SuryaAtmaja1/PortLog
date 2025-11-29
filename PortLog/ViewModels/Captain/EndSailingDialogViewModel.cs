using PortLog.Commands;
using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using Supabase.Postgrest;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PortLog.ViewModels
{
    public class EndSailingDialogViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase;
        public event EventHandler<bool> RequestClose;

        public long ShipId { get; }
        public string ShipName { get; }

        // Bindings
        public string ArrivalPort { get; set; }
        public DateTime? ArrivalDate { get; set; } = DateTime.Now.Date;
        public string ArrivalTimeText { get; set; } = DateTime.Now.ToString("HH:mm");

        public string RevenueText { get; set; } // decimal, IDR
        public string TotalDistanceText { get; set; } // double
        public string AvgFuelText { get; set; } // double
        public string Notes { get; set; }

        public string ErrorMessage { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EndSailingDialogViewModel(SupabaseService supabase, long shipId, string shipName)
        {
            _supabase = supabase;
            ShipId = shipId;
            ShipName = shipName;

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => Cancel());

            // Optionally load latest voyage to prefill notes etc.
            _ = LoadCurrentVoyage();
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }

        private async Task LoadCurrentVoyage()
        {
            try
            {
                var resp = await _supabase
                    .Table<VoyageLog>()
                    .Where(v => v.ShipId == ShipId)
                    .Order(v => v.DepartureTime, Constants.Ordering.Descending)
                    .Limit(1)
                    .Get();

                var v = resp.Models.FirstOrDefault() as VoyageLog;
                if (v != null && v.ArrivalTime == null)
                {
                    ArrivalPort = v.ArrivalPort;
                    Notes = v.Notes;
                    OnPropertyChanged(nameof(ArrivalPort));
                    OnPropertyChanged(nameof(Notes));
                }
            }
            catch { /* ignore */ }
        }

        private async Task SaveAsync()
        {
            ErrorMessage = string.Empty;

            // ===== VALIDASI =====
            if (string.IsNullOrWhiteSpace(ArrivalPort))
            {
                ErrorMessage = "Arrival port harus diisi.";
                OnPropertyChanged(nameof(ErrorMessage));
                return;
            }

            if (!ArrivalDate.HasValue)
            {
                ErrorMessage = "Arrival date invalid.";
                OnPropertyChanged(nameof(ErrorMessage));
                return;
            }

            if (!TimeSpan.TryParse(ArrivalTimeText, out var ts))
            {
                ErrorMessage = "Format waktu harus HH:mm.";
                OnPropertyChanged(nameof(ErrorMessage));
                return;
            }

            var arrivalDateTime = ArrivalDate.Value.Date + ts;

            // ===== PARSE NUMBER INPUTS =====
            long? revenueCents = null;
            if (!string.IsNullOrWhiteSpace(RevenueText))
            {
                if (decimal.TryParse(RevenueText, NumberStyles.Number, CultureInfo.InvariantCulture, out var dec))
                    revenueCents = (long)(dec * 100);
                else
                {
                    ErrorMessage = "Revenue format invalid.";
                    OnPropertyChanged(nameof(ErrorMessage));
                    return;
                }
            }

            double? totalDistance = null;
            if (!string.IsNullOrWhiteSpace(TotalDistanceText))
            {
                if (double.TryParse(TotalDistanceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    totalDistance = d;
                else
                {
                    ErrorMessage = "Distance invalid.";
                    OnPropertyChanged(nameof(ErrorMessage));
                    return;
                }
            }

            double? avgFuel = null;
            if (!string.IsNullOrWhiteSpace(AvgFuelText))
            {
                if (double.TryParse(AvgFuelText, NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
                    avgFuel = f;
                else
                {
                    ErrorMessage = "Average fuel invalid.";
                    OnPropertyChanged(nameof(ErrorMessage));
                    return;
                }
            }

            try
            {
                // =====================================================================
                // GET LATEST ACTIVE VOYAGE (ArrivalTime = NULL)
                // =====================================================================
                var resp = await _supabase
                    .Table<VoyageLog>()
                    .Where(v => v.ShipId == ShipId)
                    .Order(v => v.DepartureTime, Constants.Ordering.Descending)
                    .Limit(1)
                    .Get();

                var voyage = resp.Models.FirstOrDefault() as VoyageLog;

                if (voyage == null)
                {
                    ErrorMessage = "Tidak ditemukan voyage aktif untuk kapal ini.";
                    OnPropertyChanged(nameof(ErrorMessage));
                    return;
                }

                // =====================================================================
                // UPDATE VOYAGE LOG
                // =====================================================================
                await _supabase
                    .Table<VoyageLog>()
                    .Where(x => x.Id == voyage.Id)
                    .Set(x => x.ArrivalPort, ArrivalPort.Trim())
                    .Set(x => x.ArrivalTime, arrivalDateTime) // Add ToUniversalTime()
                    .Set(x => x.TotalDistanceTraveled, totalDistance)
                    .Set(x => x.AverageFuelConsumption, avgFuel)
                    .Set(x => x.Revenue, revenueCents)
                    .Set(x => x.Notes, Notes ?? string.Empty)
                    .Update();

                Debug.WriteLine(voyage);
                // =====================================================================
                // UPDATE SHIP STATUS → "STANDBY"
                // =====================================================================

                await _supabase
                    .Table<Ship>()
                    .Where(x => x.Id == ShipId)
                    .Set(x => x.Status, "STANDBY")
                    .Update();

                // Close dialog
                RequestClose?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Gagal menyimpan: " + ex.Message;
                Debug.WriteLine(ErrorMessage);
                Debug.WriteLine("Mawemek: "+ arrivalDateTime);
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

    }
}
