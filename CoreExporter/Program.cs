using Microsoft.Data.Sqlite;
using System;
using System.Linq;
using Dapper;
using CoreExporter.Data;
using System.Text;
using System.IO;
using System.Globalization;

namespace CoreExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                string dbName = args[0];
                string exportPath = args[1];
                if( !Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }
                var cb = new SqliteConnectionStringBuilder();
                cb.DataSource = dbName;

                AmazfitRepository repo = new AmazfitRepository(cb.ToString());

                foreach (var item in repo.GetSportSummaryList())
                {
                    Console.WriteLine($"ID: {item.Id} trackid: {item.trackid} Starttime:{item.StartDateTime} EndTime: {item.EndDateTime} gps:{item.SportDetail.GPSCoordinates.Count}");

                    if(( item.SportDetail.GPSCoordinates.Count!= item.SportDetail.TimeList.Count))
                    {
                        Console.WriteLine("GpsCoordinates and TimeList don't correlate");
                        continue;
                    }
                    // if(( item.SportDetail.GPSCoordinates.Count!= item.SportDetail.DistanceList.Count))
                    // {
                    //     Console.WriteLine("GpsCoordinates and distances/heartrate don't correlate");
                    //     continue;
                    // }

                    StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n");
                    sb.AppendLine("<TrainingCenterDatabase version=\"1.0\" creator=\"CoreExporter by FZelle\" xsi:schemaLocation=\"http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd\" xmlns:ns5=\"http://www.garmin.com/xmlschemas/ActivityGoals/v1\" xmlns:ns3=\"http://www.garmin.com/xmlschemas/ActivityExtension/v2\" xmlns:ns2=\"http://www.garmin.com/xmlschemas/UserProfile/v2\" xmlns=\"http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:ns4=\"http://www.garmin.com/xmlschemas/ProfileExtension/v1\">");
                    sb.AppendLine("  <Activities>");
                    sb.AppendLine($"    <Activity Sport=\"{item.ItemTypeString}\">");
                    sb.AppendLine($"      <Id>{item.StartDateTime:yyyy-MM-ddTHH:mm:ss.fffZ}</Id>");
                    sb.AppendLine($"      <Creator><Name>Huami Amazfit Pace</Name></Creator>");
                    sb.AppendLine($"      <Lap StartTime=\"{item.StartDateTime:yyyy-MM-ddTHH:mm:ss.fffZ}\">");
                    sb.AppendLine($"        <TotalTimeSeconds>{item.run_time}</TotalTimeSeconds>");
                    sb.AppendLine($"        <DistanceMeters>{item.dis}</DistanceMeters>");
                    sb.AppendLine($"        <Calories>{item.cal}</Calories>");
                    sb.AppendLine($"        <AverageHeartRateBpm><Value>{item.avg_heart_rate}</Value></AverageHeartRateBpm>");
                    sb.AppendLine($"        <MaximumHeartRateBpm><Value>{item.MaxHeartRate}</Value></MaximumHeartRateBpm>");
                    sb.AppendLine($"        <Track>");
                    int timePoint = 0;
                    for (int i = 0; i < item.SportDetail.GPSCoordinates.Count; i++)
                    {
                        sb.AppendLine($"          <Trackpoint>");
                        timePoint += item.SportDetail.TimeList[i];
                        var startTime = item.StartDateTime.AddSeconds(timePoint);
                        sb.AppendLine($"            <Time>{startTime:yyyy-MM-ddTHH:mm:ss.fffZ}</Time>");
                        if( item.SportDetail.DistanceList.ContainsKey(timePoint))
                        {
                            sb.AppendLine($"            <DistanceMeters>{item.SportDetail.DistanceList[timePoint].ToString(CultureInfo.InvariantCulture)}</DistanceMeters>");
                        }
                        sb.AppendLine($"            <AltitudeMeters>{item.SportDetail.AltitudeList[i].ToString(CultureInfo.InvariantCulture)}</AltitudeMeters>");
                        sb.AppendLine($"            <Position>");
                        sb.AppendLine($"              <LatitudeDegrees>{item.SportDetail.GPSCoordinates[i].Latitude.ToString(CultureInfo.InvariantCulture)}</LatitudeDegrees>");
                        sb.AppendLine($"              <LongitudeDegrees>{item.SportDetail.GPSCoordinates[i].Longitude.ToString(CultureInfo.InvariantCulture)}</LongitudeDegrees>");
                        sb.AppendLine($"            </Position>");
                        if(item.SportDetail.HeartRateList.ContainsKey(timePoint))
                        {
                            sb.AppendLine($"            <HeartRateBpm>");
                            sb.AppendLine($"              <Value>{item.SportDetail.HeartRateList[timePoint]}</Value>");
                            sb.AppendLine($"            </HeartRateBpm>");
                        }
                        sb.AppendLine($"          </Trackpoint>");
                    }
                    sb.AppendLine("        </Track>");
                    sb.AppendLine("      </Lap>");
                    sb.AppendLine("    </Activity>");
                    sb.AppendLine("  </Activities>");
                    sb.AppendLine("</TrainingCenterDatabase>");
                    var fileName = Path.Combine(exportPath, $"{item.StartDateTime:yyyy-MM-dd-HH-mm-ss}_{item.ItemTypeString}.TCX");
                    File.WriteAllText(fileName, sb.ToString());
                }
                Console.WriteLine("Conversion Done");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine($"usage: CoreExporter DBFileName ExportDirectory");
                Console.ReadLine();
            }
        }
    }
}
