using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace loadshedding.Models
{
    public class ScheduleRoot
    {
        public int Id { get; set; }
        public string Area { get; set; }

        [JsonPropertyName("date_of_month")]
        public int DateOfMonth { get; set; }

        [JsonIgnore]
        public DateTime StartTime { get; set; }

        [JsonIgnore]
        public DateTime FinishTime { get; set; }


        //Coverting time from Timespan to Datetime
        [JsonPropertyName("start_time")]
        public string StartTimeString
        {
            get => StartTime.ToString("HH:mm:ss");
            set
            {
                if (TimeSpan.TryParse(value, out TimeSpan time))
                {
                    try
                    {
                        var now = DateTime.Now;
                        var date = new DateTime(now.Year, now.Month, DateOfMonth);
                        StartTime = date.Add(time);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw new ArgumentException($"Invalid date_of_month value: {DateOfMonth}");
                    }
                }
            }
        }

        //Coverting time from Timespan to Datetime
        [JsonPropertyName("finish_time")]
        public string FinishTimeString
        {
            get => FinishTime.ToString("HH:mm:ss");
            set
            {
                if (TimeSpan.TryParse(value, out TimeSpan time))
                {
                    try
                    {
                        var now = DateTime.Now;
                        var date = new DateTime(now.Year, now.Month, DateOfMonth);
                        FinishTime = date.Add(time);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw new ArgumentException($"Invalid date_of_month value: {DateOfMonth}");
                    }
                }
            }
        }

        public int Stage { get; set; }
    }
}
