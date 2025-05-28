using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AvaloniaAzora.Data;
using AvaloniaAzora.Services;
using Supabase;
using System;
using System.IO;

namespace AvaloniaAzora.Services
{
    public static class ServiceProvider
    {
        private static IServiceProvider? _instance;
        
        public static IServiceProvider Instance => _instance ?? throw new InvalidOperationException("ServiceProvider not initialized");

        // Keep this method for backwards compatibility with existing ViewModels
        public static T GetService<T>() where T : notnull
        {
            return Instance.GetRequiredService<T>();
        }

        public static void Initialize()
        {
            try
            {
                var services = new ServiceCollection();
                
                // Configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                
                services.AddSingleton<IConfiguration>(configuration);
                
                // Supabase Configuration
                var supabaseUrl = configuration["Supabase:Url"];
                var supabaseKey = configuration["Supabase:AnonKey"];
                
                if (!string.IsNullOrEmpty(supabaseUrl) && !string.IsNullOrEmpty(supabaseKey))
                {
                    var supabaseOptions = new Supabase.SupabaseOptions
                    {
                        AutoConnectRealtime = true
                    };
                    
                    services.AddSingleton<Supabase.Client>(provider => 
                        new Supabase.Client(supabaseUrl, supabaseKey, supabaseOptions));
                }
                
                // Database Configuration
                var connectionString = configuration["ConnectionStrings:DefaultConnection"];
                if (!string.IsNullOrEmpty(connectionString))
                {
                    services.AddDbContextFactory<AppDbContext>(options =>
                    {
                        options.UseNpgsql(connectionString);
                        options.EnableSensitiveDataLogging(false);
                        options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Error);
                    });
                }
                
                // Services
                services.AddScoped<IDataService, EfCoreDataService>();
                services.AddScoped<IAuthenticationService, SupabaseAuthenticationService>();
                services.AddScoped<GroqApiService>();
                
                _instance = services.BuildServiceProvider();
                
                Console.WriteLine("✅ ServiceProvider initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to initialize ServiceProvider: {ex.Message}");
                throw;
            }
        }
    }
}
