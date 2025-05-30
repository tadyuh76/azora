using Supabase.Gotrue;
using System;
using System.Threading.Tasks;

namespace AvaloniaAzora.Services
{
    public interface IAuthenticationService
    {
        Task<Session?> SignInAsync(string email, string password, bool rememberMe = false);
        Task<Session?> SignUpAsync(string email, string password, string fullName);
        Task<bool> SendPasswordResetAsync(string email);
        Task<bool> ResendVerificationCodeAsync(string email, string type);
        Task<Session?> VerifyOtpAsync(string email, string token, string type);
        Task<bool> UpdatePasswordAsync(string newPassword);
        Task SignOutAsync();
        Task<Session?> GetCurrentSessionAsync();
        User? GetCurrentUser();
        event EventHandler<Session?>? AuthStateChanged;
    }
}