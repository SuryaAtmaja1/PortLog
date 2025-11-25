using PortLog.ViewModels;

namespace PortLog.Services
{
    public class NavigationService
    {
        public MainViewModel MainViewModel;

        public NavigationService(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
        }

        public void NavigateTo<T>() where T : BaseViewModel
        {
            var viewModel = Activator.CreateInstance(typeof(T), this) as BaseViewModel;
            MainViewModel.CurrentViewModel = viewModel;
        }

        public void NavigateTo(BaseViewModel viewModel)
        {
            MainViewModel.CurrentViewModel = viewModel;
        }
    }
}
