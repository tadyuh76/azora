using System;
using System.Collections.Generic;

namespace AvaloniaAzora.Services
{
    /// <summary>
    /// Event arguments for class enrollment changes
    /// </summary>
    public class ClassEnrollmentChangedEventArgs : EventArgs
    {
        public Guid ClassId { get; set; }
        public int NewEnrollmentCount { get; set; }
        public DateTimeOffset ChangeTime { get; set; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Service to manage notifications when class enrollments change
    /// This enables automatic refresh of test rankings when new students are enrolled
    /// </summary>
    public class EnrollmentNotificationService
    {
        private static readonly Lazy<EnrollmentNotificationService> _instance = new(() => new EnrollmentNotificationService());
        public static EnrollmentNotificationService Instance => _instance.Value;

        /// <summary>
        /// Event fired when a student is enrolled or removed from a class
        /// </summary>
        public event EventHandler<ClassEnrollmentChangedEventArgs>? ClassEnrollmentChanged;

        private EnrollmentNotificationService() { }

        /// <summary>
        /// Notify all subscribers that enrollment has changed for a class
        /// </summary>
        /// <param name="classId">The class where enrollment changed</param>
        /// <param name="newEnrollmentCount">The new total enrollment count</param>
        public void NotifyEnrollmentChanged(Guid classId, int newEnrollmentCount)
        {
            ClassEnrollmentChanged?.Invoke(this, new ClassEnrollmentChangedEventArgs
            {
                ClassId = classId,
                NewEnrollmentCount = newEnrollmentCount
            });
        }

        /// <summary>
        /// Subscribe to enrollment changes for a specific class
        /// </summary>
        /// <param name="classId">The class to monitor</param>
        /// <param name="callback">The callback to invoke when enrollment changes</param>
        /// <returns>A subscription that can be disposed to unsubscribe</returns>
        public IDisposable SubscribeToClassChanges(Guid classId, Action<ClassEnrollmentChangedEventArgs> callback)
        {
            void Handler(object? sender, ClassEnrollmentChangedEventArgs e)
            {
                if (e.ClassId == classId)
                {
                    callback(e);
                }
            }

            ClassEnrollmentChanged += Handler;

            return new Subscription(() => ClassEnrollmentChanged -= Handler);
        }

        /// <summary>
        /// Helper class for managing event subscriptions
        /// </summary>
        private class Subscription : IDisposable
        {
            private readonly Action _unsubscribe;
            private bool _disposed;

            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _unsubscribe();
                    _disposed = true;
                }
            }
        }
    }
}
