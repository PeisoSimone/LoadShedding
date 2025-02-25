using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
namespace loadshedding.Services
{
    public static class AnalyticsHelper
    {
        /// <summary>
        /// Track a custom event with App Center Analytics
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="properties">Optional properties to include with the event</param>
        public static void TrackEvent(string eventName, Dictionary<string, string> properties = null)
        {
            try
            {
                Analytics.TrackEvent(eventName, properties);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to track event: {ex.Message}");
            }
        }

        /// <summary>
        /// Track page views in your application
        /// </summary>
        /// <param name="pageName">Name of the page being viewed</param>
        public static void TrackPageView(string pageName)
        {
            var properties = new Dictionary<string, string>
            {
                { "PageName", pageName }
            };
            TrackEvent("PageView", properties);
        }

        /// <summary>
        /// Track load shedding status changes
        /// </summary>
        /// <param name="oldStage">Previous load shedding stage</param>
        /// <param name="newStage">New load shedding stage</param>
        public static void TrackLoadSheddingStageChange(int oldStage, int newStage)
        {
            var properties = new Dictionary<string, string>
            {
                { "OldStage", oldStage.ToString() },
                { "NewStage", newStage.ToString() },
                { "ChangeTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };
            TrackEvent("LoadSheddingStageChanged", properties);
        }

        /// <summary>
        /// Track weather location changes
        /// </summary>
        /// <param name="oldLocation">Previous weather location</param>
        /// <param name="newLocation">New weather location</param>
        public static void TrackWeatherLocationChange(string oldLocation, string newLocation)
        {
            var properties = new Dictionary<string, string>
            {
                { "OldLocation", oldLocation ?? "None" },
                { "NewLocation", newLocation ?? "None" },
                { "ChangeTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };
            TrackEvent("WeatherLocationChanged", properties);
        }

        /// <summary>
        /// Track load shedding area changes
        /// </summary>
        /// <param name="oldArea">Previous load shedding area</param>
        /// <param name="newArea">New load shedding area</param>
        public static void TrackLoadSheddingAreaChange(string oldArea, string newArea)
        {
            var properties = new Dictionary<string, string>
            {
                { "OldArea", oldArea ?? "None" },
                { "NewArea", newArea ?? "None" },
                { "ChangeTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };
            TrackEvent("LoadSheddingAreaChanged", properties);
        }

        /// <summary>
        /// Track weather service API failures
        /// </summary>
        /// <param name="endpoint">The API endpoint that failed</param>
        /// <param name="errorMessage">Error message or details</param>
        /// <param name="statusCode">HTTP status code (if applicable)</param>
        public static void TrackWeatherApiFailure(string endpoint, string errorMessage, string statusCode = null)
        {
            var properties = new Dictionary<string, string>
            {
                { "Endpoint", endpoint },
                { "ErrorMessage", errorMessage },
                { "Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            if (statusCode != null)
            {
                properties.Add("StatusCode", statusCode);
            }

            TrackEvent("WeatherApiFailure", properties);
        }

        /// <summary>
        /// Track load shedding calendar service API failures
        /// </summary>
        /// <param name="endpoint">The API endpoint that failed</param>
        /// <param name="errorMessage">Error message or details</param>
        /// <param name="statusCode">HTTP status code (if applicable)</param>
        public static void TrackCalendarApiFailure(string endpoint, string errorMessage, string statusCode = null)
        {
            var properties = new Dictionary<string, string>
            {
                { "Endpoint", endpoint },
                { "ErrorMessage", errorMessage },
                { "Time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            if (statusCode != null)
            {
                properties.Add("StatusCode", statusCode);
            }

            TrackEvent("CalendarApiFailure", properties);
        }

        /// <summary>
        /// Report a handled exception to App Center Crashes
        /// </summary>
        /// <param name="exception">The exception that was caught</param>
        /// <param name="properties">Optional properties to include with the report</param>
        public static void ReportHandledException(Exception exception, Dictionary<string, string> properties = null)
        {
            try
            {
                Crashes.TrackError(exception, properties);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to report handled exception: {ex.Message}");
            }
        }
    }
}