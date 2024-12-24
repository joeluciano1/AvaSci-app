using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using LightBuzz.AvaSci.Measurements;
using Newtonsoft.Json;
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
public static string ConvertCsvStringToJson(string csvString)
    {
        csvString = csvString.Replace(";", ",");
        csvString = csvString.Replace("Timestamp", "TimeOfReading");
        csvString = csvString.Replace("VarusValgusLeftAngleDistance", "VarusValgusLeft");
        csvString = csvString.Replace("VarusValgusRightAngleDistance", "VarusValgusRight");
        csvString = csvString.Replace("HipAnkleHipKneeLeftAbductionDifference", "AnkleHipLeftAbductionDifference");
        csvString = csvString.Replace("HipAnkleHipKneeRightAbductionDifference", "AnkleHipRightAbductionDifference");
        var rows = new List<Dictionary<string, string>>();

        // Use StringReader to process the CSV string line by line
        using (var reader = new StringReader(csvString))
        {
            // Read the header line
            string headerLine = reader.ReadLine();
            if (string.IsNullOrEmpty(headerLine))
                throw new InvalidOperationException("CSV string is empty or invalid.");

            string[] headers = headerLine.Split(',');

            // Process each data row
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = line.Split(',');

                var row = new Dictionary<string, string>();

                for (int i = 0; i < headers.Length; i++)
                {
                    string key = headers[i].Trim();
                    string value = i < values.Length ? values[i].Trim() : "";
                    row[key] = value;
                }

                rows.Add(row);
            }
        }

        // Convert the rows list to JSON
        return JsonConvert.SerializeObject(rows, Formatting.Indented);
    }
}
