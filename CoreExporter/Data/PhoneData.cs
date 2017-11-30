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
        public string[] longitudelatitudeArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(longitude_latitude))
                    return new string[0];
                return longitude_latitude.Split(';');
            }
        }
        public List<LonLat> GPSCoordinates
        {
            get
            {
                if( m_GPSCoordinates!=null)
                    return m_GPSCoordinates;

                if (longitudelatitudeArray?.Length > 0)
                {
                    var retval = new List<LonLat>();
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
                            retval.Add(lastRecord);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    m_GPSCoordinates = retval;
                    return retval;
                }
                return null;
            }
        }
        List<LonLat> m_GPSCoordinates;

        public List<double> AltitudeList
        {
            get
            {
                if( m_AltitudeList!=null)
                    return m_AltitudeList;
                var list = new List<double>();
                if(heart_rate!= null)
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
                m_AltitudeList = list;
                return list;
            }
        }
        List<double> m_AltitudeList;
        public List<double> DistanceList
        {
            get
            {
                if( m_DistanceList!=null)
                    return m_DistanceList;

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
                m_DistanceList = list;
                return list;
            }
        }
        List<double> m_DistanceList;
        public List<int> HeartRateList
        {
            get
            {
                if( m_HeartRateList!=null)
                    return m_HeartRateList;
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
                m_HeartRateList  = list;
                return list;
            }
        }
        List<int> m_HeartRateList;
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
                if( m_TimeList!=null)
                    return m_TimeList;
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
                m_TimeList = list;
                return list;
            }
        }
        List<int> m_TimeList;

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
