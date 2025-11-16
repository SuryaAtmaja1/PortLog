using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortLog.ViewModels;

namespace PortLog.Services
{
    public class NavigationService
    {
        private readonly MainViewModel _mainViewModel;

        public NavigationService(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public void NavigateTo<T>() where T : BaseViewModel
        {
            var viewModel = Activator.CreateInstance(typeof(T), this) as BaseViewModel;
            _mainViewModel.CurrentViewModel = viewModel;
        }

        public void NavigateTo(BaseViewModel viewModel)
        {
            _mainViewModel.CurrentViewModel = viewModel;
        }
    }
}
