using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EFCore.Migrations.Toolkit.Tests.Helpers;

static internal class SqlServerTestDatabase
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
            .GetConnectionString("SqlServerTestDatabase");
    }

    private static bool TryConnect()
    {
        var connectionString = ConnectionString;

        if (string.IsNullOrEmpty(connectionString))
            return false;

        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            return true;
        }
        catch
        {
            return false;
        }
    }
}