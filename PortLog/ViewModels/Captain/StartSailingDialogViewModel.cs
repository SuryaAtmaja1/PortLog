using PortLog.Commands;
using PortLog.Models;
using PortLog.Services;
using PortLog.Supabase;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Supabase.Postgrest.Constants;

namespace PortLog.ViewModels
{
    public class StartSailingDialogViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase;
        public event EventHandler<bool> RequestClose;

        public long ShipId { get; }
        public string ShipName { get; }

        // Bindings
        public string DeparturePort { get; set; }
        public DateTime? DepartureDate { get; set; } = DateTime.Now.Date;
        public string DepartureTimeText { get; set; } = DateTime.Now.ToString("HH:mm");
        public string ErrorMessage { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public StartSailingDialogViewModel(SupabaseService supabase, long shipId, string shipName)
        {
            _supabase = supabase;
            ShipId = shipId;
            ShipName = shipName;

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }

        private async Task SaveAsync()
        {
            ErrorMessage = string.Empty;
            // Validation
            if (string.IsNullOrWhiteSpace(DeparturePort))
            {
                ErrorMessage = "Departure port harus diisi.";
                OnPropertyChanged(nameof(ErrorMessage));
                return;
            }

            if (!DepartureDate.HasValue)
            {
                ErrorMessage = "Departure date invalid.";
                OnPropertyChanged(nameof(ErrorMessage));
                return;
            }

            if (!TimeSpan.TryParse(DepartureTimeText, out var ts))
            {
                ErrorMessage = "Format waktu harus HH:mm.";
                OnPropertyChanged(nameof(ErrorMessage));
                return;
            }

            var departureDateTime = DepartureDate.Value.Date + ts;

            try
            {
                // 1) create new VoyageLog
                var voyage = new VoyageLog
                {
                    ShipId = ShipId,
                    DeparturePort = DeparturePort.Trim(),
                    DepartureTime = departureDateTime,
                    ArrivalPort = null,
                    ArrivalTime = null,
                    Notes = string.Empty,
                    TotalDistanceTraveled = null,
                    AverageFuelConsumption = null,
                    Revenue = null
                };

                // insert voyage
                await _supabase.Table<VoyageLog>().Insert(voyage);

                // 2) update Ship status -> SAILING
                // assuming Ship model exists
                await _supabase.Table<Ship>().Where(x => x.Id == ShipId).Set(x => x.Status, "SAILING").Update();

                await _supabase.Table<VoyageLog>().Update(voyage);

                RequestClose?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Gagal menyimpan: " + ex.Message;
                Debug.WriteLine(ErrorMessage);
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
    }
}
