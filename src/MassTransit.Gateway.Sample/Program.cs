using System;
using System.Data.SqlClient;
using System.Linq;
using MassTransit.Gateway.Sql.Database;
using MassTransit.Gateway.Sql.Dynamics;

namespace MassTransit.Gateway.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            const string connectionString = "Data Source=dev-app3.abax.local;Initial Catalog=ETS;User Id=T2F_Web;password=ets;MultipleActiveResultSets=True";

            var schema = DbSchemaReader.ReadTableSchema(() => new SqlConnection(connectionString), "dbo.GpsTrip");

            var properties = schema.Select(PropertyDefinitionFactory.FromDbColumnInfo).ToArray();

            Console.WriteLine(properties.Length);
        }
    }
}