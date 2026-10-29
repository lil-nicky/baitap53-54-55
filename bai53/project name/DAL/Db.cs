
using System.Data.SqlClient;

namespace CafePOSSystem.DAL
{
    internal static class Db
    {

        private static readonly string connectionString =
     @"Data Source=BEN\SQLEXPRESS;Initial Catalog=CafePOSDB;Integrated Security=True;TrustServerCertificate=True";

        internal static SqlConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
