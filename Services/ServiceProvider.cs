using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AvaloniaAzora.Models;
using Supabase;
using System;
using System.IO;

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
                var supabaseConfig = new SupabaseConfig
                {
                    Url = supabaseSection["Url"] ?? throw new InvalidOperationException("Supabase Url not found in appsettings.json"),
                    AnonKey = supabaseSection["AnonKey"] ?? throw new InvalidOperationException("Supabase AnonKey not found in appsettings.json"),
                    ConnectionString = supabaseSection["ConnectionString"] ?? throw new InvalidOperationException("Supabase ConnectionString not found in appsettings.json")
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
    }
}