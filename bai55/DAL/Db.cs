
using System.Data.SqlClient;

namespace InventoryManagement.DAL
{
    public static class Db
    {
        private static readonly string connectionString =
            @"Data Source=.\SQLEXPRESS;Initial Catalog=InventoryDB;Integrated Security=True;TrustServerCertificate=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
