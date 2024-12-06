using System.Diagnostics;
using System.IO;
using UnityEngine;
public class RScriptExecutor : MonoBehaviour
{
    public void RunRScript(string csvPath)
    {
        // Paths
        string rScriptPath = Path.Combine(Application.dataPath, "Scripts/R/generate_report.R");
        string csvFilePath = csvPath;;

        // Make sure the file exists
        if (!File.Exists(rScriptPath))
        {
            UnityEngine.Debug.LogError("R script not found at " + rScriptPath);
            return;
        }

        // Call R script
        Process process = new Process();
        process.StartInfo.FileName = "Rscript"; // Ensure Rscript is in your system's PATH
        process.StartInfo.Arguments = $"\"{rScriptPath}\" \"{csvFilePath}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        // Optional: Capture output
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        UnityEngine.Debug.Log("R script output: " + output);
    }
}
