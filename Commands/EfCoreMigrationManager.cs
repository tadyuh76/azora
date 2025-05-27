using AvaloniaAzora.Data;
using AvaloniaAzora.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaAzora.Commands
{
    public static class EfCoreMigrationManager
    {
        /// <summary>
        /// Masks the password in a connection string for secure logging
        /// </summary>
        private static string MaskPassword(string connectionString)
        {
            // Simple approach to mask password without parsing the whole connection string
            if (string.IsNullOrEmpty(connectionString))
                return string.Empty;

            const string passwordKey = "Password=";
            int passwordStartIndex = connectionString.IndexOf(passwordKey, StringComparison.OrdinalIgnoreCase);

            if (passwordStartIndex < 0)
                return connectionString;

            int valueStartIndex = passwordStartIndex + passwordKey.Length;
            int valueEndIndex = connectionString.IndexOf(';', valueStartIndex);

            if (valueEndIndex < 0)
                valueEndIndex = connectionString.Length;

            return connectionString.Substring(0, valueStartIndex) +
                   "********" +
                   connectionString.Substring(valueEndIndex);
        }
        public static async Task InitializeDatabaseAsync()
        {
            try
            {
                Console.WriteLine("Initializing database...");

                // Get the AppDbContext from the service provider
                using var scope = AvaloniaAzora.Services.ServiceProvider.Instance.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Test connection before trying to ensure database exists
                Console.WriteLine("Testing database connection...");
                bool canConnect = await dbContext.Database.CanConnectAsync();

                if (!canConnect)
                {
                    Console.WriteLine("WARNING: Cannot connect to database! Check your connection string.");

                    // Try to provide diagnostic information
                    var connectionString = dbContext.Database.GetConnectionString();
                    if (connectionString != null)
                    {
                        // Mask the password for security
                        var sanitizedConnectionString = MaskPassword(connectionString);
                        Console.WriteLine($"Connection string being used: {sanitizedConnectionString}");
                    }

                    throw new InvalidOperationException("Cannot connect to the database. Please check your configuration.");
                }

                // Create the database if it doesn't exist - since we're using an existing Supabase schema,
                // we don't need to run migrations, just ensure the database is accessible
                Console.WriteLine("Database connection successful! Ensuring database schema is accessible...");
                await dbContext.Database.EnsureCreatedAsync();

                Console.WriteLine("Database connection established successfully!");

                // Check if database is accessible
                var connectionTest = await dbContext.Database.CanConnectAsync();
                Console.WriteLine($"Database connection test: {(connectionTest ? "Success" : "Failed")}");

                if (connectionTest)
                {
                    // List tables in the database using FormattableString to protect against SQL injection
                    FormattableString query = $"SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'";
                    var tables = await dbContext.Database
                        .SqlQuery<string>(query)
                        .ToListAsync();

                    Console.WriteLine("\nDatabase tables:");
                    foreach (var table in tables)
                    {
                        Console.WriteLine($"- {table}");
                    }

                    // Count users
                    var userCount = await dbContext.Users.CountAsync();
                    Console.WriteLine($"\nUsers in database: {userCount}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}