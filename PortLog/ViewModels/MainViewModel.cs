using PortLog.Services;
using PortLog.Supabase;

namespace PortLog.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;

        public NavigationService NavigationService { get; }
        public SupabaseService SupabaseService { get; }
        public AccountService AccountService { get; }
        public CompanyService CompanyService { get; }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public MainViewModel()
        {
            // 1. Create a single shared SupabaseService instance
            SupabaseService = GlobalServices.Get<SupabaseService>();

            // 2. Inject it into your business services
            AccountService = new AccountService(SupabaseService);
            CompanyService = new CompanyService(SupabaseService);

            // 3. Navigation needs this MainViewModel
            NavigationService = new NavigationService(this);

            // 4. Start app at Login
            CurrentViewModel = new LoginViewModel(NavigationService, AccountService);
        }
    }
}
