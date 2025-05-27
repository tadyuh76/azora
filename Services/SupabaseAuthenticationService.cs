using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using Supabase.Gotrue.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaAzora.Services
{
    public class SupabaseAuthenticationService : IAuthenticationService
    {
        private readonly Supabase.Client _supabaseClient;
        private bool _isInitialized = false;

        public event EventHandler<Session?>? AuthStateChanged;

        public SupabaseAuthenticationService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_isInitialized)
            {
                await _supabaseClient.InitializeAsync();

                // Subscribe to auth state changes after initialization
                _supabaseClient.Auth.AddStateChangedListener(OnAuthStateChanged);

                _isInitialized = true;
            }
        }

        private void OnAuthStateChanged(IGotrueClient<User, Session> sender, Constants.AuthState state)
        {
            AuthStateChanged?.Invoke(this, sender.CurrentSession);
        }

        public async Task<Session?> SignInAsync(string email, string password)
        {
            try
            {
                await EnsureInitializedAsync();
                var result = await _supabaseClient.Auth.SignIn(email, password);
                return result;
            }
            catch (GotrueException ex)
            {
                // Log the error or handle it appropriately
                throw new Exception($"Sign in failed: {ex.Message}", ex);
            }
        }

        public async Task<Session?> SignUpAsync(string email, string password, string fullName)
        {
            try
            {
                await EnsureInitializedAsync();
                var options = new SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        ["full_name"] = fullName
                    }
                };

                var result = await _supabaseClient.Auth.SignUp(email, password, options);
                return result;
            }
            catch (GotrueException ex)
            {
                throw new Exception($"Sign up failed: {ex.Message}", ex);
            }
        }

        public async Task<bool> SendPasswordResetAsync(string email)
        {
            try
            {
                await EnsureInitializedAsync();
                await _supabaseClient.Auth.ResetPasswordForEmail(email);
                return true;
            }
            catch (GotrueException ex)
            {
                throw new Exception($"Password reset failed: {ex.Message}", ex);
            }
        }

        public async Task<bool> ResendVerificationCodeAsync(string email, string type)
        {
            try
            {
                await EnsureInitializedAsync();

                if (type.ToLower() == "recovery")
                {
                    // For password reset, we can just call the reset method again
                    await _supabaseClient.Auth.ResetPasswordForEmail(email);
                }
                else
                {
                    // For signup verification, we use the workaround mentioned in Supabase discussions:
                    // Since there's no direct resend method, we can try signing up again with the same email
                    // This will resend the verification email if the user hasn't been confirmed yet
                    // Note: This is a temporary workaround until the resend method is available

                    // We'll use SignInWithOtp which can be called multiple times
                    var options = new SignInWithPasswordlessEmailOptions(email);
                    await _supabaseClient.Auth.SignInWithOtp(options);
                }

                return true;
            }
            catch (GotrueException ex)
            {
                throw new Exception($"Resend verification failed: {ex.Message}", ex);
            }
        }

        public async Task<Session?> VerifyOtpAsync(string email, string token, string type)
        {
            try
            {
                await EnsureInitializedAsync();
                // Convert string type to the appropriate enum value
                var otpType = type.ToLower() switch
                {
                    "signup" => Constants.EmailOtpType.Email,
                    "recovery" => Constants.EmailOtpType.Recovery,
                    _ => Constants.EmailOtpType.Email
                };

                var result = await _supabaseClient.Auth.VerifyOTP(email, token, otpType);
                return result;
            }
            catch (GotrueException ex)
            {
                throw new Exception($"OTP verification failed: {ex.Message}", ex);
            }
        }

        public async Task SignOutAsync()
        {
            try
            {
                await EnsureInitializedAsync();
                await _supabaseClient.Auth.SignOut();
            }
            catch (GotrueException ex)
            {
                throw new Exception($"Sign out failed: {ex.Message}", ex);
            }
        }

        public async Task<Session?> GetCurrentSessionAsync()
        {
            try
            {
                await EnsureInitializedAsync();
                return _supabaseClient.Auth.CurrentSession;
            }
            catch
            {
                return null;
            }
        }

        public User? GetCurrentUser()
        {
            // Note: This is a synchronous method, so we can't call EnsureInitializedAsync here
            // The client should be initialized by the time this is called
            return _supabaseClient.Auth.CurrentUser;
        }
    }
}