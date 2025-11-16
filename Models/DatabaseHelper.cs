using Microsoft.Extensions.Configuration;

namespace AdditionApi.Models
{
    public static class DatabaseHelper
    {
        public static string GetConnectionString(IConfiguration configuration)
        {
            return configuration.GetConnectionString("DefaultConnection");
        }
    }
}
