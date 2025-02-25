using Plugin.LocalNotification;
using System;
using loadshedding.Interfaces;

namespace loadshedding.Services
{
    public class NotificationServices : INotificationServices
    {
        public void ShowNotification(string title, string message, DateTime? scheduleTime = null)
        {
            try
            {
                var notification = new NotificationRequest
                {
                    Title = title,
                    Description = message,
                    NotificationId = new Random().Next(100, 1000)
                };

                if (scheduleTime.HasValue && scheduleTime.Value > DateTime.Now)
                {
                    // Schedule the notification for future delivery
                    notification.Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = scheduleTime.Value
                    };

                    // Log for debugging
                    Console.WriteLine($"Scheduling notification: {title} for {scheduleTime.Value.ToString("yyyy-MM-dd HH:mm:ss")}");
                }

                // Show/schedule the notification
                LocalNotificationCenter.Current.Show(notification);
                Console.WriteLine($"Notification created: {title}, ID: {notification.NotificationId}");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error showing notification: {ex.Message}");
            }
        }
    }
}