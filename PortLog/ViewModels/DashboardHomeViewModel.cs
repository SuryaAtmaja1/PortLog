using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortLog.ViewModels
{
    public class DashboardHomeViewModel : INotifyPropertyChanged
    {
        private int currentSailingShips = 4;
        public int CurrentSailingShips
        {
            get => currentSailingShips;
            set { currentSailingShips = value; OnPropertyChanged(nameof(CurrentSailingShips)); }
        }

        private decimal weeklyRevenue = 1989000000;
        public string WeeklyRevenueDisplay => $"Rp{weeklyRevenue:N0}";

        private int weeklyVoyages = 10;
        public int WeeklyVoyages
        {
            get => weeklyVoyages;
            set { weeklyVoyages = value; OnPropertyChanged(nameof(WeeklyVoyages)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
