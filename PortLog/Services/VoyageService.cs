using PortLog.Models;
using PortLog.Supabase;
using Supabase.Postgrest;
using System.Diagnostics;
using static Supabase.Postgrest.Constants;

namespace PortLog.Services
{
    public class VoyageService
    {
        private readonly SupabaseService _supabase;
        private readonly AccountService _accountService;

        public VoyageService(SupabaseService supabase, AccountService accountService)
        {
            _supabase = supabase;
            _accountService = accountService;
        }

        public async Task<VoyageLog?> GetLatestVoyageForShipAsync(long shipId)
        {
            var response = await _supabase
                .Table<VoyageLog>()
                .Where(v => v.ShipId == shipId)
                .Order(v => v.DepartureTime, Ordering.Descending)
                .Limit(1)
                .Get();
            return response.Models.FirstOrDefault();
        }

        public async Task<List<VoyageLog>> GetVoyagesInLast7DaysAsync(object[] shipIds)
        {
            try
            {
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

                var res = await _supabase
                    .Table<VoyageLog>()
                    .Filter("ship_id", Operator.In, shipIds)
                    .Filter("arrival_time", Operator.GreaterThanOrEqual, sevenDaysAgo.ToString("o"))
                    .Order("arrival_time", Ordering.Descending)
                    .Get();

                return res.Models ?? new List<VoyageLog>();
            }
            catch
            {
                return new List<VoyageLog>();
            }
        }

        public async Task<VoyageLog?> GetLatestCompletedVoyage(object[] shipIds)
        {
            var res = await _supabase
                .Table<VoyageLog>()
                .Filter("ship_id", Operator.In, shipIds)
                .Order("arrival_time", Ordering.Descending)
                .Limit(1)
                .Get();

            return res.Models.FirstOrDefault();
        }

        public async Task<List<VoyageLog>> GetVoyagesInRangeAsync(object[] shipIds, DateTime start, DateTime end)
        {
            try
            {
                var res = await _supabase
                    .Table<VoyageLog>()
                    .Filter("ship_id", Operator.In, shipIds)
                    .Filter("departure_time", Operator.GreaterThanOrEqual, start.ToString("o"))
                    .Filter("arrival_time", Operator.LessThanOrEqual, end.ToString("o"))
                    .Get();

                return res.Models ?? new List<VoyageLog>();
            }
            catch
            {
                return new List<VoyageLog>();
            }
        }

        public async Task<List<VoyageLog>> GetVoyagesByShipAsync(
            long shipId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var query = _supabase
                    .Table<VoyageLog>()
                    .Filter("ship_id", Operator.Equals, shipId.ToString());

                if (startDate != null)
                {
                    query = query.Filter(
                        "departure_time",
                        Operator.GreaterThanOrEqual,
                        startDate.Value.ToString("yyyy-MM-dd")
                    );
                }

                if (endDate != null)
                {
                    query = query.Filter(
                        "arrival_time",
                        Operator.LessThanOrEqual,
                        endDate.Value.ToString("yyyy-MM-dd")
                    );
                }

                var response = await query.Get();

                return response.Models ?? new List<VoyageLog>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetVoyagesByShipAsync] {ex.Message}");
                return new List<VoyageLog>();
            }
        }

        public async Task<List<VoyageLog>> GetVoyagesByDateRangeAsync(DateTime start, DateTime end)
        {
            try
            {
                var companyId = _accountService.LoggedInAccount.CompanyId.Value;
                // Step 1: Get ships owned by this company
                var shipRes = await _supabase
                    .Table<Ship>()
                    .Filter("company_id", Operator.Equals, companyId.ToString())
                    .Get();

                var shipIds = shipRes.Models.Select(s => s.Id).ToList();

                if (shipIds.Count == 0)
                    return new List<VoyageLog>();

                var res = await _supabase
                    .Table<VoyageLog>()
                    .Filter("ship_id", Operator.In, shipIds) // <-- pass the List<Guid>
                    .Filter("departure_time", Operator.GreaterThanOrEqual, start.ToString("o"))
                    .Filter("arrival_time", Operator.LessThanOrEqual, end.ToString("o"))
                    .Get();

                return res.Models ?? new List<VoyageLog>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetVoyagesByDateRangeAsync] {ex.Message}");
                return new List<VoyageLog>();
            }
        }

    }
}
