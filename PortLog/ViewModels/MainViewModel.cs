using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortLog.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public LoginViewModel LoginVM { get; }
        public DashboardViewModel DashboardVM { get; }

        public MainViewModel()
        {
            LoginVM = new LoginViewModel(this);
            DashboardVM = new DashboardViewModel();

            CurrentView = LoginVM;
        }

        public void NavigateToDashboard()
        {
            CurrentView = DashboardVM;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
