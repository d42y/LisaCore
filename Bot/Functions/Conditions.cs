using Microsoft.Recognizers.Definitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Functions
{
    internal static class Conditions
    {
        public static string TimeOfDay()
        {
            TimeSpan time = DateTime.Now.TimeOfDay;

            if (time >= TimeSpan.FromHours(0) && time < TimeSpan.FromHours(6))
            {
                return "Night";
            }
            else if (time >= TimeSpan.FromHours(6) && time < TimeSpan.FromHours(12))
            {
                return "Morning";
            }
            else if (time >= TimeSpan.FromHours(12) && time < TimeSpan.FromHours(16))
            {
                return "Afternoon";
            }
            else if (time >= TimeSpan.FromHours(16) && time < TimeSpan.FromHours(20))
            {
                return "Evening";
            }
            else //if (time >= TimeSpan.FromHours(20) && time < TimeSpan.FromHours(24))
            {
                return "Night";
            }
        }

        public static string Weather()
        {
            if (!CheckInternetConnection()) return "I cannot get to the Internet to get weather information.";
            string baseUrl = "https://api.weather.gov";

            try
            {
                var location = GetUserLocation();
                string url = $"{baseUrl}/points/{location.Latitude},{location.Longitude}/forecast";

                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0"); // Set a user agent header to avoid HTTP 403 error

                    string response = client.DownloadString(url);

                    // Parse the JSON response to extract the weather information
                    dynamic weatherData = JsonConvert.DeserializeObject(response);
                    string forecast = weatherData.properties.periods[0].detailedForecast;

                    // Return the weather forecast
                    return forecast;
                }
            }
            catch
            {
                return "Failed to retrieve weather information.";
            }

        }
        private static Location GetUserLocation()
        {
            try
            {
                string ipAddress = new WebClient().DownloadString("https://api.ipify.org"); // Get the public IP address
                string url = $"https://ipapi.co/{ipAddress}/json/"; // Use an IP geolocation service like ipapi.co

                using (var client = new WebClient())
                {
                    string response = client.DownloadString(url);

                    // Parse the JSON response to extract the location information
                    dynamic locationData = JsonConvert.DeserializeObject(response);
                    double latitude = locationData.latitude;
                    double longitude = locationData.longitude;

                    // Return the user's location information as a Location object
                    return new Location
                    {
                        Latitude = latitude,
                        Longitude = longitude
                    };
                }
            }
            catch
            {
                return null; // Return null if failed to determine the location
            }
        }

        private class Location
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        public static bool CheckInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
