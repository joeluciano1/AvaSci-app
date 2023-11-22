using LightBuzz.AvaSci.Measurements;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace LightBuzz.AvaSci.Csv
{
    /// <summary>
    /// Exports a CSV file from a LightBuzz video folder.
    /// </summary>
    public class CSVManager
    {
        /// <summary>
        /// Creates a CSV file from a LightBuzz video folder, saves the file on the app local storage, and uses the device's native share functionality to export the file.
        /// </summary>
        /// <param name="source">The absolute path to the LightBuzz video folder.</param>
        /// <param name="measurements">A list of measurements to export.</param>
        public static async void CreateSaveExport(string source, List<MeasurementType> measurements)
        {
            string destination = GetDestination(source);

            await Task.Run(() =>
            {
                string csv = Create(source, measurements);
                Save(csv, destination);
            });

            Export(destination);
        }

        /// <summary>
        /// Creates a CSV file from a LightBuzz video folder.
        /// </summary>
        /// <param name="source">The absolute path to the LightBuzz video folder.</param>
        /// <param name="measurements">A list of measurements to export.</param>
        /// <returns>The CSV file contents.</returns>
        public static string Create(string source, List<MeasurementType> measurements)
        {
            return CSV.Export(source, measurements);
        }

        /// <summary>
        /// Saves a CSV file on the app local storage.
        /// </summary>
        /// <param name="csv">The CSV file contents.</param>
        /// <param name="destination">The absolute path to a .csv file within the app local storage.</param>
        public static void Save(string csv, string destination)
        {
            File.WriteAllText(destination, csv);
        }

        /// <summary>
        /// Uses the device's native share functionality to export a CSV file.
        /// </summary>
        /// <param name="file">The absolute path to a .csv file within the app local storage.</param>
        public static void Export(string file)
        {
            if (Application.isMobilePlatform)
            {
                ExportMobile(file);
            }
            else
            {
                ExportDesktop(file);
            }
        }

        private static string GetDestination(string source)
        {
            string name = new DirectoryInfo(source).Name;
            string extension = ".csv";
            string temp = Path.Combine(Application.persistentDataPath, "Temp");

            if (!Directory.Exists(temp))
            {
                Directory.CreateDirectory(temp);
            }

            string destination = Path.Combine(temp, name + extension);

            return destination;
        }

        private static void ExportMobile(string destination)
        {
            new NativeShare().
                AddFile(destination).
                Share();
        }

        private static void ExportDesktop(string destination)
        {
            string folder = Path.GetDirectoryName(destination);
            string url = folder.Replace(" ", "%20");
            //string url = UnityEngine.Networking.UnityWebRequest.EscapeURL(folder);

            Application.OpenURL($"file://{url}");
        }
    }
}