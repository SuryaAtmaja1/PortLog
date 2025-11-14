namespace PortLog.Services
{
    public static class GlobalServices
    {
        public static AccountService Account { get; private set; }

        public static void Init()
        {
            Account = new AccountService();
        }
    }
}
