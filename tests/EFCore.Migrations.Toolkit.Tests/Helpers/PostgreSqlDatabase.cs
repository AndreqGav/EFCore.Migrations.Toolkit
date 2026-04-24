using System;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EFCore.Migrations.Toolkit.Tests.Helpers
{
    static internal class PostgreSqlDatabase
    {
        private static readonly Lazy<string> ConnectionStringLazy = new(LoadConnectionString);

        private static readonly Lazy<bool> IsAvailableLazy = new(TryConnect);

        public static string ConnectionString => ConnectionStringLazy.Value;

        public static bool IsAvailable => IsAvailableLazy.Value;

        private static string LoadConnectionString()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddEnvironmentVariables()
                .Build()
                .GetConnectionString("PostgreSqlTestDatabase");
        }

        private static bool TryConnect()
        {
            var connectionString = ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                return false;
            }

            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}