using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LightBuzz.AvaSci.Csv;
using LightBuzz.AvaSci.Measurements;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;


public static class GeneralStaticManager
{
    public static Dictionary<string, string> GlobalVar = new Dictionary<string, string>();
    public static Dictionary<string, Sprite> GlobalImage = new Dictionary<string, Sprite>();
    public static Dictionary<string, List<float>> GraphsReadings = new Dictionary<string, List<float>>();

    public static string AddSpacesToSentence(string text, bool preserveAcronyms)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
                if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                     i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
    }
    public static double DegreesToRadians(double degrees)
    {
        return (Math.PI / 180) * degrees;
    }
    [DllImport("__Internal")]
    private static extern void _OpenFile(string path);

    public static void OpenFile(string path)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _OpenFile(path);
#else
        Debug.Log("Only IOS");
#endif
    }

    public static string GetMeasurementTypeName(this object value)
    {
        return Enum.GetName(typeof(MeasurementType), value);
    }
    public static float ClosestTo(this IEnumerable<float> collection, float target)
    {
    return collection.OrderBy(x => Math.Abs(target - x)).First();
    }
public static async Task<string> ConvertCsvStringToJson(string csvString)
{
    
    var rows = new List<Dictionary<string, string>>();

    using (var reader = new StringReader(csvString))
    {
        string headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrEmpty(headerLine))
            throw new InvalidOperationException("CSV string is empty or invalid.");

        // Replace field names safely
        headerLine = headerLine.Replace(";", ",")
                               .Replace("Timestamp", "TimeOfReading")
                               .Replace("VarusValgusLeftAngleDistance", "VarusValgusLeft")
                               .Replace("VarusValgusRightAngleDistance", "VarusValgusRight")
                               .Replace("HipAnkleHipKneeLeftAbductionDifference", "AnkleHipLeftAbductionDifference")
                               .Replace("HipAnkleHipKneeRightAbductionDifference", "AnkleHipRightAbductionDifference")
                               .Replace("AnkleRight 3D Z", "AnkleRight3DZ")
                               .Replace("AnkleLeft 3D Z", "AnkleLeft3DZ")
                               .Replace("HeelLeft 3D Z", "HeelLeft3DZ")
                               .Replace("HeelRight 3D Z", "HeelRight3DZ")
                               .Replace("AnkleRight Confidence","AnkleRightConfidence")
                               .Replace("AnkleLeft Confidence","AnkleLeftConfidence");
            

        string[] headers = headerLine.Split(',');

        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Now split actual data by semicolon (original delimiter)
            string[] values = line.Split(';');

            var row = new Dictionary<string, string>();
            for (int i = 0; i < headers.Length; i++)
            {
                string key = headers[i].Trim();
                string value = i < values.Length ? values[i].Trim() : "";

                if (value.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                    value = null;

                row[key] = value;
            }

            rows.Add(row);
        }
    }

    return JsonConvert.SerializeObject(rows, Formatting.Indented);
}

    public static void CreateZipFile(string outputZipPath, List<string> csvFilePaths)
    {
        if (csvFilePaths == null || csvFilePaths.Count == 0)
        {
            Debug.LogError("No CSV files provided for zipping.");
            return;
        }

        // Ensure the output directory exists
        string outputDirectory = Path.GetDirectoryName(outputZipPath);
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // Create the ZIP file
        using (FileStream zipToCreate = new FileStream(outputZipPath, FileMode.Create))
        {
            using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
            {
                foreach (string filePath in csvFilePaths)
                {
                    if (File.Exists(filePath))
                    {
                        // Add the CSV file to the ZIP archive
                        ZipArchiveEntry entry = archive.CreateEntry(Path.GetFileName(filePath));
                        using (Stream entryStream = entry.Open())
                        {
                            byte[] csvBytes = File.ReadAllBytes(filePath);
                            entryStream.Write(csvBytes, 0, csvBytes.Length);
                        }
                        Debug.Log($"Added {filePath} to ZIP.");
                    }
                    else
                    {
                        Debug.LogError($"File not found: {filePath}");
                    }
                }
            }
        }

        Debug.Log("ZIP file created at: " + outputZipPath);

        // Optionally, open the ZIP file in file explorer
        CSVManager.Export(outputZipPath);
    }
    public static long GetFileSizeFromBase64(string base64String)
    {
        // Remove Base64 metadata if present (e.g., "data:image/png;base64,")
        int indexOfComma = base64String.IndexOf(",");
        if (indexOfComma >= 0)
        {
            base64String = base64String.Substring(indexOfComma + 1);
        }

        int padding = base64String.EndsWith("==") ? 2 : base64String.EndsWith("=") ? 1 : 0;
        long fileSizeInBytes = (long)Math.Floor((base64String.Length * 3) / 4.0) - padding;
    
        return fileSizeInBytes;
    }
    public static long GetStringSizeInBytes(string data, Encoding encoding)
    {
        return encoding.GetByteCount(data);
    }
    public static T MiddleOrDefault<T>(this IEnumerable<T> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        var list = source as IList<T> ?? source.ToList();
        if (list.Count == 0) return default;

        return list[list.Count / 2];
    }
    public static T MiddleOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var filteredList = source.Where(predicate).ToList();
        if (filteredList.Count == 0) return default;

        return filteredList[filteredList.Count / 2];
    }

    public static T MiddleOfFirstQuarterOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var filteredList = source.Where(predicate).ToList();
        if (filteredList.Count == 0) return default;

        int quarterCount = filteredList.Count / 4;
        if (quarterCount == 0) return filteredList[0]; // If less than 4 items, return first

        int middleIndex = quarterCount / 2;
        return filteredList[middleIndex];
    }
    public static T LastOfFirstQuarterOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var filteredList = source.Where(predicate).ToList();
        if (filteredList.Count == 0) return default;

        int quarterCount = filteredList.Count / 4;
        if (quarterCount == 0) return filteredList[0]; // Fallback if less than 4 items

        return filteredList[quarterCount - 1]; // Last of the first quarter
    }
    public static T MiddleOfSecondQuarterOrDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var filteredList = source.Where(predicate).ToList();
        if (filteredList.Count == 0) return default;

        int quarterLength = filteredList.Count / 4;
        if (quarterLength == 0) return filteredList[0]; // fallback if too small

        int start = quarterLength; // second quarter starts here
        int middleIndex = start + quarterLength / 2;

        if (middleIndex >= filteredList.Count)
            middleIndex = filteredList.Count - 1; // prevent overflow

        return filteredList[middleIndex];
    }
    public static float GetMiddleValue(List<float> values)
    {
        if (values == null || values.Count == 0)
            throw new ArgumentException("List is empty or null");

        values.Sort(); // Ensure the list is sorted

        int mid = values.Count / 2;

        if (values.Count % 2 == 0)
            return (values[mid - 1] + values[mid]) / 2f; // average of two middle elements
        else
            return values[mid]; // exact middle
    }
    public static string GenerateRandomName(int length = 5)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";

        System.Text.StringBuilder result = new System.Text.StringBuilder();

        // Start with 2 random digits
        for (int i = 0; i < 2; i++)
            result.Append(digits[UnityEngine.Random.Range(0, digits.Length)]);

        // Add middle random lowercase letters
        for (int i = 0; i < length - 4; i++)
            result.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);

        // End with 2 random digits
        for (int i = 0; i < 2; i++)
            result.Append(digits[UnityEngine.Random.Range(0, digits.Length)]);

        return result.ToString();
    }
    public static string FormatCsvAsTable(string csvRaw)
    {
        csvRaw = csvRaw.Replace("\\n", "\n"); // in case \n is string literal
        var lines = csvRaw.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return "";

        var rows = lines.Select(l => l.Split(',')).ToList();
        int columnCount = rows.Max(r => r.Length);

        // Calculate max width of each column
        int[] maxWidths = new int[columnCount];
        foreach (var row in rows)
        {
            for (int i = 0; i < row.Length; i++)
                maxWidths[i] = Mathf.Max(maxWidths[i], row[i].Length);
        }

        var sb = new System.Text.StringBuilder();

        for (int r = 0; r < rows.Count; r++)
        {
            for (int c = 0; c < columnCount; c++)
            {
                string cell = (c < rows[r].Length) ? rows[r][c] : "";
                string padded = cell.PadRight(maxWidths[c] + 2);
                sb.Append(r == 0 ? $"<b>{padded}</b>" : padded);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
    public static bool AreAllNullablePropertiesNull(object obj)
    {
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties)
        {
            // Check if property is a nullable type (e.g., int? or float?)
            if (Nullable.GetUnderlyingType(property.PropertyType) != null)
            {
                // If any nullable property is not null, return false
                if (property.GetValue(obj) != null)
                    return false;
            }
        }
        
        return true; // All nullable properties are null
    }
    public static string ToJsonExcludingNulls(object obj)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                // Ignore null values during serialization
                DefaultMembersSearchFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                NamingStrategy = new CamelCaseNamingStrategy(),
                IgnoreSerializableAttribute = true
            },
            NullValueHandling = NullValueHandling.Ignore
        };

        return JsonConvert.SerializeObject(obj, settings);
    }
    
    public static string ExportDenormalized(List<UserReportData> users)
    {
        var sb = new StringBuilder();

        // ===== HEADER =====
        sb.AppendLine(string.Join(",", new[]
        {
            // UserReportData (parent)
            "User_Id","User_UserID","User_UserName","User_VideoURL","User_ReportURL",
            "User_CreatedOn","User_ReportDescription","User_GroupName","User_SubGroupName",
            "User_SubjectId","User_HasHtmlReports","User_JointReadingsJson",

            // Key used to align (nullable)
            "ReportsRecordId",

            // Gait (child)
            "G_CreatedBy","G_Subject","G_SubjectStandingAtTime","G_FootStrikeAtTime","G_HeelPassingAtTime",
            "G_AngleDifferenceAtTime","G_MMDistaceAtTime","G_MaxAngleDifference","G_MaxmmDistance",
            "G_HipAbductionAtTime","G_PelvisAngleAtTime","G_AnkleAbductionAtTime","G_VarusValgusAtTime",
            "G_SelectedLeg","G_VideoName",

            // Time-based reading (child)
            "T_UserName","T_TimeOfReading","T_KneeLeftAbduction","T_KneeRightAbduction","T_PelvisAngle",
            "T_AnkleHipLeftAbductionDifference","T_AnkleHipRightAbductionDifference",
            "T_HipKneeRightDistance","T_HipKneeLeftDistance","T_NeckLateralFlexion","T_NeckRotation",
            "T_ElbowLeftFlexion","T_ElbowRightFlexion","T_ShoulderLeftAbduction","T_ShoulderLeftRotation",
            "T_ShoulderRightAbduction","T_ShoulderRightRotation","T_ShoulderLeftFlexion","T_ShoulderRightFlexion",
            "T_HipLeftAbduction","T_HipLeftFlexion","T_HipRightAbduction","T_HipRightFlexion",
            "T_KneeLeftFlexion","T_KneeRightFlexion","T_AnkleLeftAbduction","T_AnkleRightAbduction",
            "T_VarusValgusRight","T_VarusValgusLeft","T_StepLength","T_StepLeftAngle","T_StepRightAngle",
            "T_VideoName","T_CreatedOnISO","T_AnkleRight3DZ","T_AnkleLeft3DZ","T_HeelLeft3DZ","T_HeelRight3DZ",
            "T_AnkleRightConfidence","T_AnkleLeftConfidence"
        }));

        foreach (var u in users ?? Enumerable.Empty<UserReportData>())
        {
            // Group children by ReportsRecordId (nullable long => key can be null)
            var gaitByKey = (u.GaitReports ?? new List<GetGaitReportResponse>())
                .GroupBy(x => x.ReportsRecordId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var timeByKey = (u.TimeBasedReadings ?? new List<TimeBasedReadingRequest>())
                .GroupBy(x => x.ReportsRecordId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // All keys present in either side
            var allKeys = new HashSet<long?>(gaitByKey.Keys);
            foreach (var k in timeByKey.Keys) allKeys.Add(k);

            // If both sides empty, still emit one parent-only row
            if (allKeys.Count == 0)
            {
                WriteRow(sb, u, reportsRecordId: null, g: null, t: null);
                continue;
            }

            foreach (var key in allKeys)
            {
                var gList = gaitByKey.TryGetValue(key, out var gv) ? gv : new List<GetGaitReportResponse>();
                var tList = timeByKey.TryGetValue(key, out var tv) ? tv : new List<TimeBasedReadingRequest>();

                // Ensure at least one iteration even if one side is empty
                if (gList.Count == 0 && tList.Count == 0)
                {
                    WriteRow(sb, u, key, null, null);
                }
                else if (gList.Count == 0)
                {
                    foreach (var t in tList) WriteRow(sb, u, key, null, t);
                }
                else if (tList.Count == 0)
                {
                    foreach (var g in gList) WriteRow(sb, u, key, g, null);
                }
                else
                {
                    // If multiple on both sides for the same key, emit all combinations
                    foreach (var g in gList)
                        foreach (var t in tList)
                            WriteRow(sb, u, key, g, t);
                }
            }
        }

        return sb.ToString();
    }

    public static void SaveToFile(string csv, string filePath) =>
        File.WriteAllText(filePath, csv ?? "", Encoding.UTF8);

    // ----- helpers -----

    private static void WriteRow(
        StringBuilder sb,
        UserReportData u,
        long? reportsRecordId,
        GetGaitReportResponse g,
        TimeBasedReadingRequest t)
    {
        // Serialize JointReadings to JSON (safe for CSV cell)
        string jointJson = (u.JointReadings != null && u.JointReadings.Count > 0)
            ? JsonConvert.SerializeObject(u.JointReadings)
            : "";

        var cells = new List<string>
        {
            // User
            Csv(u.Id), Csv(u.UserID), Csv(u.UserName), Csv(u.VideoURL), Csv(u.ReportURL),
            Csv(u.CreatedOn), Csv(u.ReportDescription), Csv(u.GroupName), Csv(u.SubGroupName),
            Csv(u.SubjectId), Csv(u.HasHtmlReports), Csv(jointJson),

            // Key
            Csv(reportsRecordId),

            // Gait
            Csv(g?.CreatedBy), Csv(g?.Subject), Csv(g?.SubjectStandingAtTime),
            Csv(g?.FootStrikeAtTime), Csv(g?.HeelPassingAtTime),
            Csv(g?.AngleDifferenceAtTime), Csv(g?.MMDistaceAtTime),
            Csv(g?.MaxAngleDifference), Csv(g?.MaxmmDistance),
            Csv(g?.HipAbductionAtTime), Csv(g?.PelvisAngleAtTime),
            Csv(g?.AnkleAbductionAtTime), Csv(g?.VarusValgusAtTime),
            Csv(g?.SelectedLeg), Csv(g?.VideoName),

            // Time-based
            Csv(t?.UserName), Csv(t?.TimeOfReading), Csv(t?.KneeLeftAbduction), Csv(t?.KneeRightAbduction),
            Csv(t?.PelvisAngle), Csv(t?.AnkleHipLeftAbductionDifference), Csv(t?.AnkleHipRightAbductionDifference),
            Csv(t?.HipKneeRightDistance), Csv(t?.HipKneeLeftDistance), Csv(t?.NeckLateralFlexion), Csv(t?.NeckRotation),
            Csv(t?.ElbowLeftFlexion), Csv(t?.ElbowRightFlexion), Csv(t?.ShoulderLeftAbduction), Csv(t?.ShoulderLeftRotation),
            Csv(t?.ShoulderRightAbduction), Csv(t?.ShoulderRightRotation), Csv(t?.ShoulderLeftFlexion), Csv(t?.ShoulderRightFlexion),
            Csv(t?.HipLeftAbduction), Csv(t?.HipLeftFlexion), Csv(t?.HipRightAbduction), Csv(t?.HipRightFlexion),
            Csv(t?.KneeLeftFlexion), Csv(t?.KneeRightFlexion), Csv(t?.AnkleLeftAbduction), Csv(t?.AnkleRightAbduction),
            Csv(t?.VarusValgusRight), Csv(t?.VarusValgusLeft), Csv(t?.StepLength), Csv(t?.StepLeftAngle), Csv(t?.StepRightAngle),
            Csv(t?.VideoName),
            Csv(t?.CreatedOn.ToString("o")), // ISO 8601
            Csv(t?.AnkleRight3DZ), Csv(t?.AnkleLeft3DZ), Csv(t?.HeelLeft3DZ), Csv(t?.HeelRight3DZ),
            Csv(t?.AnkleRightConfidence), Csv(t?.AnkleLeftConfidence)
        };

        sb.AppendLine(string.Join(",", cells));
    }

    private static string Csv(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        // Escape quotes/commas/newlines
        return "\"" + s.Replace("\"", "\"\"") + "\"";
    }

    private static string Csv(bool b) => b ? "true" : "false";

    private static string Csv(long? n) => n.HasValue ? n.Value.ToString() : "";

    private static string Csv(float? f) => f.HasValue ? f.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";
}
