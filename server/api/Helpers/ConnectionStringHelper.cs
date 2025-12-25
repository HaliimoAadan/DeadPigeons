namespace api.Helpers;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Npgsql;

public static class ConnectionStringHelper
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        var fromUrl = configuration["url"]
                       ?? configuration["URL"]
                       ?? Environment.GetEnvironmentVariable("url")
                       ?? Environment.GetEnvironmentVariable("URL");
        var fromConfiguration = configuration.GetConnectionString("AppDb");
        var fromDefault = configuration.GetConnectionString("Default");
        var fromDatabaseUrl = configuration["DATABASE_URL"] ?? Environment.GetEnvironmentVariable("DATABASE_URL");

        var connectionString = new[] { fromConfiguration, fromDefault, fromDatabaseUrl, BuildFromPgEnvironment(), fromUrl }
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string not configured. Set url/URL, ConnectionStrings__AppDb/ConnectionStrings__Default, or DATABASE_URL (loaded via .env)."
            );
        }

        return NormalizeConnectionString(connectionString);
    }

    private static string NormalizeConnectionString(string connectionString)
    {
        var trimmed = connectionString.Trim();

        if (TryNormalizeUrl(trimmed, out var urlFormatted))
        {
            return urlFormatted;
        }

        if (trimmed.IndexOf("HOST=", StringComparison.OrdinalIgnoreCase) >= 0 &&
            trimmed.IndexOf("DB=", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            var pairs = trimmed.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in pairs)
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2)
                    values[parts[0].Trim()] = parts[1].Trim();
            }

            if (values.Count > 0)
            {
                var normalized = new List<string>();

                if (values.TryGetValue("HOST", out var host)) normalized.Add($"Host={host}");
                if (values.TryGetValue("DB", out var db)) normalized.Add($"Database={db}");
                if (values.TryGetValue("UID", out var user)) normalized.Add($"Username={user}");
                if (values.TryGetValue("PWD", out var pwd)) normalized.Add($"Password={pwd}");
                if (values.TryGetValue("PORT", out var port)) normalized.Add($"Port={port}");

                if (normalized.Count > 0)
                    return string.Join(';', normalized) + ";";
            }
        }

        return trimmed;
    }

    private static string? BuildFromPgEnvironment()
    {
        var host = Environment.GetEnvironmentVariable("PGHOST");
        var database = Environment.GetEnvironmentVariable("PGDATABASE");
        var username = Environment.GetEnvironmentVariable("PGUSER");
        var password = Environment.GetEnvironmentVariable("PGPASSWORD");
        var port = Environment.GetEnvironmentVariable("PGPORT");

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(database))
        {
            return null;
        }

        var parts = new List<string>
        {
            $"Host={host}",
            $"Database={database}"
        };

        if (!string.IsNullOrWhiteSpace(username)) parts.Add($"Username={username}");
        if (!string.IsNullOrWhiteSpace(password)) parts.Add($"Password={password}");
        if (!string.IsNullOrWhiteSpace(port)) parts.Add($"Port={port}");

        return string.Join(';', parts) + ";";
    }

    private static bool TryNormalizeUrl(string value, out string normalized)
    {
        normalized = string.Empty;

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var supportedSchemes = new[] { "postgres", "postgresql", "tcp" };
        if (!supportedSchemes.Any(s => uri.Scheme.Equals(s, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        var userInfo = uri.UserInfo.Split(':', 2);
        var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = username,
            Password = password,
            SslMode = ChooseDefaultSslMode(uri.Host)
        };

        if (!string.IsNullOrWhiteSpace(uri.Query))
        {
            var queryParameters = QueryHelpers.ParseQuery(uri.Query);
            foreach (var kvp in queryParameters)
            {
                var key = kvp.Key;
                var parameterValue = kvp.Value.ToString();

                if (string.Equals(key, "user", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(key, "username", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(key, "uid", StringComparison.OrdinalIgnoreCase))
                {
                    builder.Username = parameterValue;
                    continue;
                }

                if (string.Equals(key, "password", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(key, "pwd", StringComparison.OrdinalIgnoreCase))
                {
                    builder.Password = parameterValue;
                    continue;
                }

                if (string.Equals(key, "sslmode", StringComparison.OrdinalIgnoreCase))
                {
                    if (Enum.TryParse<SslMode>(parameterValue, true, out var sslMode))
                    {
                        builder.SslMode = sslMode;
                    }
                    continue;
                }

                builder[key] = parameterValue;
            }
        }

        normalized = builder.ConnectionString;
        return true;
    }

    private static SslMode ChooseDefaultSslMode(string host)
    {
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(host, "127.0.0.1") ||
            string.Equals(host, "::1"))
        {
            return SslMode.Disable;
        }

        return SslMode.Prefer;
    }

}