using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using LightBuzz.BodyTracking.Video;

namespace LightBuzz.AvaSci.Csv
{
    /// <summary>
    /// Creates a CSV file from a LightBuzz video folder.
    /// The produced CSV file includes:
    /// - Frame timestamps
    /// - Skeleton joint data (confidence, coordinates).
    /// - Measurement data (angles).
    /// </summary>
    public static class CSV
    {
        /// <summary>
        /// The CSV delimiter.
        /// </summary>
        public static readonly string Delimiter = ";";

        private static readonly JointType[] JointTypes = (JointType[])Enum.GetValues(typeof(JointType));

        /// <summary>
        /// Exports a LightBuzz video to CSV.
        /// </summary>
        /// <param name="folder">The absolute video folder path.</param>
        /// <returns>The CSV file contents.</returns>
        public static string Export(string folder, List<MeasurementType> measurementTypes)
        {
            string timestampsFile = Path.Combine(folder, FileNames.Timestamps + FileExtensions.Timestamps);
            string timestampsData = File.ReadAllText(timestampsFile);
            string versionFile = Path.Combine(folder, FileNames.Version + FileExtensions.Version);
            string versionData = File.ReadAllText(versionFile);

            List<DateTime> timestamps = (List<DateTime>)VideoHelper.ImportTimestamps(timestampsData);

            if (timestamps == null || timestamps.Count == 0) return string.Empty;

            timestamps.Sort();

            List<Measurement> measurements = CreateMeasurements(measurementTypes);

            StringBuilder sb = new StringBuilder();

            sb.Append($"Timestamp{Delimiter}");

            for (int j = 0; j < measurementTypes.Count; j++)
            {
                MeasurementType type = measurementTypes[j];

                sb.Append($"{type}{Delimiter}");
            }

            for (int j = 0; j < JointTypes.Length; j++)
            {
                JointType type = JointTypes[j];

                sb.Append(
                    $"{type} Confidence{Delimiter}" +
                    $"{type} 2D X{Delimiter}" +
                    $"{type} 2D Y{Delimiter}" +
                    $"{type} 3D X{Delimiter}" +
                    $"{type} 3D Y{Delimiter}" +
                    $"{type} 3D Z");

                bool isLast = j == JointTypes.Length - 1;

                if (!isLast)
                {
                    sb.Append($"{Delimiter}");
                }
            }

            sb.AppendLine();

            DateTime first = timestamps[0];

            for (int t = 0; t < timestamps.Count; t++)
            {
                DateTime date = timestamps[t];

                long timestamp = date.Ticks;

                string bodyFile = Path.Combine(folder, timestamp + FileExtensions.Body);
                string bodyData = File.ReadAllText(bodyFile);

                List<Body> bodies = VideoHelper.ImportBodyData(bodyData, versionData) as List<Body>;

                Body body = bodies?.Default();

                //double ms = 0.0;

                // if (t > 0)
                // {
                //     DateTime previous = timestamps[t - 1];
                //     ms = (date - previous).TotalMilliseconds;
                // }

                double seconds = (date - first).TotalSeconds;

                if (body != null)
                {
                    //sb.Append($"{ms:N0}{Delimiter}");
                    sb.Append($"{seconds:N5}{Delimiter}");

                    for (int j = 0; j < measurements.Count; j++)
                    {
                        Measurement measurement = measurements[j];
                        measurement.Update(body);

                        string value = measurement.Value.ToString("N2", CultureInfo.InvariantCulture);

                        sb.Append($"{value}{Delimiter}");
                    }

                    for (int j = 0; j < JointTypes.Length; j++)
                    {
                        JointType type = JointTypes[j];

                        Joint joint = body.Joints[type];
                        float confidence = joint.Confidence;
                        Vector2D position2D = joint.Position2D;
                        Vector3D position3D = joint.Position3D;

                        string c = confidence.ToString("N2", CultureInfo.InvariantCulture);
                        string x2 = position2D.X.ToString("N0", CultureInfo.InvariantCulture);
                        string y2 = position2D.Y.ToString("N0", CultureInfo.InvariantCulture);
                        string x3 = position3D.X.ToString("N2", CultureInfo.InvariantCulture);
                        string y3 = position3D.Y.ToString("N2", CultureInfo.InvariantCulture);
                        string z3 = position3D.Z.ToString("N2", CultureInfo.InvariantCulture);

                        sb.Append(
                            $"{c}{Delimiter}" +
                            $"{x2}{Delimiter}" +
                            $"{y2}{Delimiter}" +
                            $"{x3}{Delimiter}" +
                            $"{y3}{Delimiter}" +
                            $"{z3}");

                        bool isLast = j == JointTypes.Length - 1;

                        if (!isLast)
                        {
                            sb.Append($"{Delimiter}");
                        }
                    }
                }
                else
                {
                    //sb.Append($"{ms:N0}{Delimiter}");
                    sb.Append($"{seconds:N5}{Delimiter}");

                    for (int j = 0; j < measurementTypes.Count; j++)
                    {
                        sb.Append($"N/A{Delimiter}");
                    }

                    for (int j = 0; j < JointTypes.Length; j++)
                    {
                        sb.Append(
                            $"N/A{Delimiter}" +
                            $"N/A{Delimiter}" +
                            $"N/A{Delimiter}" +
                            $"N/A{Delimiter}" +
                            $"N/A{Delimiter}" +
                            $"N/A");

                        bool isLast = j == JointTypes.Length - 1;

                        if (!isLast)
                        {
                            sb.Append($"{Delimiter}");
                        }
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static List<Measurement> CreateMeasurements(List<MeasurementType> types)
        {
            List<Measurement> list = new List<Measurement>();

            foreach (MeasurementType type in types)
            {
                list.Add(Measurement.Create(type));
            }

            return list;
        }
    }
}