using Microsoft.Data.Sqlite;
using Dapper;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CoreExporter.Data
{
    public class SportSummary
    {
        public int Id { get; set; }

        public int altitude_ascend { get; set; }
        public int altitude_descend { get; set; }
        public int avg_cadence { get; set; }
        public int avg_frequency { get; set; }
        public int avg_heart_rate { get; set; }
        public double avg_pace { get; set; }
        public int avg_stride_length { get; set; }
        public string bind_device { get; set; }
        public int cal { get; set; }
        public string city { get; set; }
        public int climb_dis_ascend_time { get; set; }
        public int climb_dis_descend { get; set; }
        public int climb_dis_descend_time { get; set; }
        public int distance_ascend { get; set; }
        public string date { get; set; }
        public int dis { get; set; }
        public int end_time { get; set; }
        public int flight_ratio { get; set; }
        public int forefoot_ratio { get; set; }
        public int landing_time { get; set; }
        public int lap_distance { get; set; }
        public string location { get; set; }
        public int max_altitude { get; set; }
        public int max_cadence { get; set; }
        public int max_frequency { get; set; }
        public double max_pace { get; set; }
        public int min_altitude { get; set; }
        public double min_pace { get; set; }
        public int run_time { get; set; }
        public string source { get; set; }
        public int synced { get; set; }
        public int total_step { get; set; }
        public int trackid { get; set; }
        public int type { get; set; }
        public int version { get; set; }

        public DateTime EndDateTime
        {
            get { return new DateTime(1970, 1, 1).AddSeconds(end_time).ToLocalTime(); }
        }
        public DateTime StartDateTime
        {
            get { return new DateTime(1970, 1, 1).AddSeconds(trackid).ToLocalTime(); }
        }
        public string ItemTypeString
        {
            get
            {
                switch (type)
                {
                    case 1: return "running";
                    case 2: return "walking";
                    case 3: return "treadmill";
                    case 4: return "bike";
                    case 5: return "indoor bike";
                    case 6: return "trail run";
                    case 7: return "eliptical";
                    default:return "unknwon";
                }
            }
        }
        public int MaxHeartRate
        {
            get
            {
                return SportDetail.HeartRateList.Max();
            }
        }
        public SportDetail SportDetail { get; set; }
    }
    public class SportDetail
    {
        public int Id { get; set; }
        public string accuracy { get; set; }
        public string altitude { get; set; }
        public string air_pressure_altitude { get; set; }
        public string distance { get; set; }
        public string flag { get; set; }
        public string gait { get; set; }
        public string heart_rate { get; set; }
        public string longitude_latitude { get; set; }
        public string pace { get; set; }
        public string pause { get; set; }
        public string time { get; set; }
        public string kilo_pace { get; set; }
        public string mile_pace { get; set; }
        public int segment { get; set; }
        public string source { get; set; }
        public int synced { get; set; }
        public int trackid { get; set; }
        public int version { get; set; }
        public List<LonLat> GPSCoordinates
        {
            get
            {
                m_GPSCoordinates = m_GPSCoordinates ?? GenerateGPSCoordinatesList();
                return m_GPSCoordinates;
            }
        }

        List<LonLat> m_GPSCoordinates;
        List<int> m_TimeList;
        List<double> m_AltitudeList;
        List<int> m_HeartRateList;
        List<double> m_DistanceList;

        public List<double> AltitudeList
        {
            get
            {
                m_AltitudeList= m_AltitudeList ?? GenerateAltitudeList();
                return m_AltitudeList;
            }
        }
        public List<double> DistanceList
        {
            get
            {
                m_DistanceList = m_DistanceList ?? GenerateDistanceList();
                return m_DistanceList;
            }
        }

        public List<int> HeartRateList
        {
            get
            {
                m_HeartRateList = m_HeartRateList ?? GenereateHeartrateList();
                return m_HeartRateList;
            }
        }

        public double Distance
        {
            get
            {
                return DistanceList.Sum();
            }
        }
        public List<int> TimeList
        {
            get
            {
                m_TimeList = m_TimeList ?? GenerateTimeList();
                return m_TimeList;
            }
        }


        private List<double> GenerateAltitudeList()
        {
            var list = new List<double>();
            if (heart_rate != null)
            {
                double firstValid = 0.0;
                foreach (var item in altitude.Split(';'))
                {
                    double value = 0.0;
                    if (double.TryParse(item, out value))
                    {
                        value /= 100.0;
                        if (value > 0.0)
                            firstValid = value;
                        list.Add(value);
                    }
                    else
                        list.Add(0);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] <= 0.0)
                        list[i] = firstValid;
                }
            }
            return list;
        }

        private List<int> GenereateHeartrateList()
        {
            var list = new List<int>();
            if (distance != null)
            {
                int startValue = 0;

                foreach (var item in heart_rate.Split(';'))
                {
                    int value = 0;
                    if (int.TryParse(item.Split(',')[1], out value))
                    {
                        startValue += value;
                        list.Add(startValue);
                    }
                    else
                        list.Add(0);
                }
            }
            return list;
        }

        private List<int> GenerateTimeList()
        {
            var list = new List<int>();
            if (distance != null)
            {
                foreach (var item in time.Split(';'))
                {
                    int value = 0;
                    if (int.TryParse(item, out value))
                    {
                        list.Add(value);
                    }
                    else
                        list.Add(0);
                }
            }
            return list;
        }
        private List<double> GenerateDistanceList()
        {
            var list = new List<double>();
            if (distance != null)
            {
                foreach (var item in distance.Split(';'))
                {
                    double value = 0.0;
                    if (double.TryParse(item.Split(',')[1], out value))
                    {
                        list.Add(value);
                    }
                    else
                        list.Add(0);
                }
            }
            return list;
        }
        private List<LonLat> GenerateGPSCoordinatesList()
        {
            if (string.IsNullOrWhiteSpace(longitude_latitude))
                return new List<LonLat>();

            string[] longitudelatitudeArray = longitude_latitude.Split(';');

            if (longitudelatitudeArray?.Length > 0)
            {
                var list = new List<LonLat>();
                LonLat lastRecord = new LonLat();
                foreach (var item in longitudelatitudeArray)
                {
                    var tuple = item.Split(',');
                    try
                    {
                        double value = 0.0;
                        if (double.TryParse(tuple[1], out value))
                            lastRecord.Longitude = lastRecord.Longitude + value / 100000000.0;
                        if (double.TryParse(tuple[0], out value))
                            lastRecord.Latitude = lastRecord.Latitude + value / 100000000.0;
                        list.Add(lastRecord);
                    }
                    catch (Exception)
                    {
                    }
                }
                return list;
            }
            return new List<LonLat>();
        }
    }
    public struct LonLat
    {
        public double Longitude;
        public double Latitude;
    }

    public class AmazfitRepository 
    {
        string m_ConnectionString;
        public AmazfitRepository(string connectionString)
        {
            m_ConnectionString = connectionString;
        }

        public List<SportSummary> GetSportSummaryList()
        {
            using (var con = new SqliteConnection(m_ConnectionString))
            {
                con.Open();
                return con.Query<SportSummary, SportDetail, SportSummary>("SELECT Sport_Summary.*,Sport_Detail.* FROM [Sport_Summary] join [Sport_Detail] on Sport_Summary.trackid = Sport_Detail.trackid", (summary, detail) =>
                {
                    summary.SportDetail = detail;
                    return summary;
                }).ToList();
            }
        }
    }

}
