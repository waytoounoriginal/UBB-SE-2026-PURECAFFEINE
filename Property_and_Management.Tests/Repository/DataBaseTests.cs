using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Property_and_Management.Src.Repository;

namespace Property_and_Management.Tests.Repository
{
    public abstract class DataBaseTests
    {
        private const string ConnectionStringName = "BoardRent";
        protected string ConnectionString { get; private set; } = null!;
        
        [SetUp]
        public void TruncateBusinessTables()
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                    "DELETE FROM Notifications;"
                    + "DELETE FROM Rentals;"
                    + "DELETE FROM Requests;"
                    + "DBCC CHECKIDENT ('Notifications', RESEED, 0);"
                    + "DBCC CHECKIDENT ('Rentals', RESEED, 0);"
                    + "DBCC CHECKIDENT ('Requests', RESEED, 0);";
                command.ExecuteNonQuery();
            }
            catch (SqlException sqlException)
            {
                Assert.Ignore(
                    "Skipping integration tests: SQL Server test database is not reachable. "
                    + $"Error: {sqlException.Message}");
            }
        }

        private static string? ReadConnectionStringFromCopiedConfig(string configFileName)
        {
            var configPath = Path.Combine(TestContext.CurrentContext.TestDirectory, configFileName);
            if (!File.Exists(configPath))
            {
                return null;
            }

            var configuration = XDocument.Load(configPath);
            foreach (var addElement in configuration.Descendants("add"))
            {
                var name = addElement.Attribute("name")?.Value;
                if (string.Equals(name, ConnectionStringName, StringComparison.OrdinalIgnoreCase))
                {
                    return addElement.Attribute("connectionString")?.Value;
                }
            }

            return null;
        }
        private static void ConfigureAppConfigForTestHost()
        {
            var copiedConfigPath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "Property_and_Management.Tests.dll.config");

            if (!File.Exists(copiedConfigPath))
            {
                return;
            }

            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", copiedConfigPath);
            ConfigurationManager.RefreshSection("connectionStrings");
        }
        private static string ResolveConnectionString()
        {
            ConfigureAppConfigForTestHost();

            var configuredConnectionString = ConfigurationManager
                .ConnectionStrings[ConnectionStringName]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(configuredConnectionString))
            {
                return configuredConnectionString;
            }

            return ReadConnectionStringFromCopiedConfig("Property_and_Management.Tests.dll.config")
                   ?? ReadConnectionStringFromCopiedConfig("App.config")
                   ?? string.Empty;
        }


        [OneTimeSetUp]
        public void InitializeDatabase()
        {
            ConnectionString = ResolveConnectionString();
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                Assert.Ignore("connection string is missing");
            }
            try
            {
                DatabaseInitializer.EnsureDatabaseInitialized();
            }
            catch (SqlException sqlException)
            {
                Assert.Ignore("SQL Server is not reachable");
            }
        }
        }
}
