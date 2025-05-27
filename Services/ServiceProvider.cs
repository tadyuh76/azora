using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AvaloniaAzora.Models;
using Supabase;
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using AvaloniaAzora.Data;

namespace AvaloniaAzora.Services
{
    public static class ServiceProvider
    {
        private static IServiceProvider? _serviceProvider;

        public static IServiceProvider Instance
        {
            get
            {
                if (_serviceProvider == null)
                {
                    throw new InvalidOperationException("ServiceProvider has not been initialized. Call Initialize() first.");
                }
                return _serviceProvider;
            }
        }

        public static void Initialize()
        {
            try
            {
                Console.WriteLine("Initializing ServiceProvider...");
                var services = new ServiceCollection();

                // Configuration
                Console.WriteLine("Loading configuration...");
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                services.AddSingleton<IConfiguration>(configuration);

                // Supabase Configuration
                Console.WriteLine("Reading Supabase configuration...");
                var supabaseSection = configuration.GetSection("Supabase");

                // Get the URI-style connection string
                var uriConnectionString = supabaseSection["ConnectionString"] ??
                    throw new InvalidOperationException("Supabase ConnectionString not found in appsettings.json");

                // Parse the URI connection string to standard Npgsql format
                var parsedConnectionString = ParseConnectionString(uriConnectionString);
                Console.WriteLine($"Parsed connection string: {parsedConnectionString}");

                var supabaseConfig = new SupabaseConfig
                {
                    Url = supabaseSection["Url"] ?? throw new InvalidOperationException("Supabase Url not found in appsettings.json"),
                    AnonKey = supabaseSection["AnonKey"] ?? throw new InvalidOperationException("Supabase AnonKey not found in appsettings.json"),
                    ConnectionString = parsedConnectionString
                };

                Console.WriteLine($"Supabase URL: {supabaseConfig.Url}");

                // Supabase Client
                Console.WriteLine("Creating Supabase client...");
                services.AddSingleton<Supabase.Client>(provider =>
                {
                    var options = new SupabaseOptions
                    {
                        AutoConnectRealtime = false, // Disable auto-connect to prevent blocking
                        AutoRefreshToken = true
                    };

                    var client = new Supabase.Client(supabaseConfig.Url, supabaseConfig.AnonKey, options);

                    // Don't initialize synchronously - this was causing the hang
                    // client.InitializeAsync().GetAwaiter().GetResult();

                    return client;
                });

                // Authentication Service
                Console.WriteLine("Registering authentication service...");
                services.AddSingleton<IAuthenticationService, SupabaseAuthenticationService>();

                // Database Context
                Console.WriteLine("Registering database context...");

                // Register the DbContext using the connection string
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(supabaseConfig.ConnectionString, npgsqlOptions =>
                    {
                        // Configure Npgsql options
                        npgsqlOptions.EnableRetryOnFailure(3);
                    });

                    // Enable debugging features (disabled for cleaner console output)
                    // options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Register DbContext factory for use in ViewModels
                services.AddSingleton<IDbContextFactory<AppDbContext>>(provider =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                    optionsBuilder.UseNpgsql(supabaseConfig.ConnectionString);
                    return new CustomDbContextFactory(optionsBuilder.Options);
                });

                // Register Data Service
                Console.WriteLine("Registering data service...");
                services.AddSingleton<IDataService, EfCoreDataService>();

                _serviceProvider = services.BuildServiceProvider();
                Console.WriteLine("ServiceProvider initialized successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing ServiceProvider: {ex.Message}");
                throw;
            }
        }

        public static T GetService<T>() where T : notnull
        {
            return Instance.GetRequiredService<T>();
        }

        /// <summary>
        /// Parses a URI-style PostgreSQL connection string into the format Npgsql expects
        /// </summary>
        /// <param name="uriString">URI-style connection string (postgresql://user:pass@host:port/dbname)</param>
        /// <returns>Standard Npgsql connection string</returns>
        private static string ParseConnectionString(string uriString)
        {
            try
            {
                // Remove the postgresql:// prefix if present
                string connectionString = uriString;
                if (connectionString.StartsWith("postgresql://"))
                {
                    connectionString = connectionString.Substring("postgresql://".Length);
                }

                // Extract user and password
                string userPass = connectionString.Split('@')[0];
                string[] userPassParts = userPass.Split(':');
                string username = userPassParts[0];
                string password = userPassParts.Length > 1 ? userPassParts[1] : string.Empty;

                // Extract host, port and database
                string hostPortDb = connectionString.Split('@')[1];
                string[] hostPortParts = hostPortDb.Split('/');
                string hostPort = hostPortParts[0];
                string database = hostPortParts.Length > 1 ? hostPortParts[1] : "postgres";

                string[] hostPortSplit = hostPort.Split(':');
                string host = hostPortSplit[0];
                string port = hostPortSplit.Length > 1 ? hostPortSplit[1] : "5432";

                // Build the standard connection string
                return $"Host={host};Port={port};Username={username};Password={password};Database={database}";
            }
            catch (Exception ex)
            {
                throw new FormatException($"Failed to parse connection string: {ex.Message}", ex);
            }
        }
    }
}