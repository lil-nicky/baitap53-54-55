
using System.Data.SqlClient;

namespace HotelManagement.DAL
{
    public static class Db
    {
        private static readonly string connectionString =
            @"Data Source=.\SQLEXPRESS;Initial Catalog=HotelDB;Integrated Security=True;TrustServerCertificate=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
