using PortLog.Models;
using PortLog.Supabase;
using Supabase.Postgrest;
using System.Diagnostics;

namespace PortLog.Services
{
    public class ShipService
    {
        private readonly SupabaseService _supabase;

        public ShipService(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public async Task<Ship?> GetShipByIdAsync(long id)
        {
            try
            {
                var response = await _supabase
                    .Table<Ship>()
                    .Filter("id", Constants.Operator.Equals, id.ToString())
                    .Get();
                if (response == null)
                {
                    System.Diagnostics.Debug.WriteLine("GetShipByIdAsync: response is null");
                    return null;
                }
                var ship = response.Models?.FirstOrDefault();
                return ship;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetShipByIdAsync: exception: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public async Task<List<Ship>> GetShipsByCompanyIdAsync(Guid companyId)
        {
            try
            {
                var response = await _supabase
                    .Table<Ship>()
                    .Filter("company_id", Constants.Operator.Equals, companyId.ToString())
                    .Get();
                if (response == null)
                {
                    System.Diagnostics.Debug.WriteLine("GetShipsByCompanyIdAsync: response is null");
                    return new List<Ship>();
                }
                var ships = response.Models ?? new List<Ship>();
                return ships;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetShipsByCompanyIdAsync: exception: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return new List<Ship>();
            }
        }

        public async Task AddShipAsync(Ship ship)
        {
            try
            {
                var response = await _supabase
                    .Table<Ship>()
                    .Insert(ship);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InsertShipAsync: exception: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }

        }
    }
}
