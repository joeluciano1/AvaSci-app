using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using TMPro;  // remove if using legacy Text

public class MemoryHUD : MonoBehaviour
{
    // public TextMeshProUGUI text;   // assign in inspector
    // public float updateInterval = 0.5f; // seconds

    float timer;
    void OnEnable()
    {
        Application.lowMemory += OnLowMemoryReceived;
    }

    void OnDisable()
    {
        Application.lowMemory -= OnLowMemoryReceived;
    }

    private void OnLowMemoryReceived()
    {
        Debug.LogWarning("[LowMemoryHandler] Low memory warning received – freeing caches...");
        ReferenceManager.instance.PopupManager.Show("Low Memory Warning", "Application Running Low on Memory, Removing all saved capture references. You will have to download the captures again. Free Memory?","No",okPressed:FreeMemory,"Yes");
        // FreeMemory();
    }

    private void FreeMemory()
    {
        // 1. Clear any custom caches
        ReferenceManager.instance.userReportController.userReportFromDBs.ForEach(x=>
        {
            x.WatchBtn.onClick.RemoveAllListeners();
            File.Delete(Path.Combine(Application.persistentDataPath,
                $"{x.videoId}.txt"));
        });
        ReferenceManager.instance.userReportController.userReportFromDBs.ForEach(x=>{x.WatchBtn.onClick.AddListener(
                () =>
                {
                    StartCoroutine(
                        ReferenceManager.instance.userReportController.GetText(
                            x.VideoURL,
                            x.WatchBtn,
                            x
                        )
                    );
                    ReferenceManager.instance.azureStorageManager.selectedVideo = x;
                }
            );
            x.ButtonText.text = "Download";
            x.Download.SetActive(true);
            x.Error.SetActive(false);
            x.Watch.SetActive(false);});
        // 2. Unload unused assets
        Resources.UnloadUnusedAssets();

        // 3. Force GC for managed memory
        System.GC.Collect();
    }
    // void Update()
    // {
    //     timer -= Time.unscaledDeltaTime;
    //     if (timer <= 0f)
    //     {
    //         timer = updateInterval;
    //         UpdateMemoryText();
    //     }
    // }
    //
    // void UpdateMemoryText()
    // {
    //     // Unity memory breakdown (in bytes)
    //     long used    = Profiler.GetTotalAllocatedMemoryLong();
    //     long reserved = Profiler.GetTotalReservedMemoryLong();
    //     long unused  = Profiler.GetTotalUnusedReservedMemoryLong();
    //     long mono    = Profiler.GetMonoUsedSizeLong();
    //
    //     // Convert to MB
    //     float usedMB    = used    / (1024f * 1024f);
    //     float reservedMB = reserved / (1024f * 1024f);
    //     float unusedMB  = unused  / (1024f * 1024f);
    //     float monoMB    = mono    / (1024f * 1024f);
    //
    //     // Device total system memory (approx)
    //     int systemMB = SystemInfo.systemMemorySize;
    //
    //     text.text =
    //         $"Used: {usedMB:F1} MB\n" +
    //         $"Reserved: {reservedMB:F1} MB\n" +
    //         $"Unused Reserved: {unusedMB:F1} MB\n" +
    //         $"Mono (C#): {monoMB:F1} MB\n" +
    //         $"System RAM: {systemMB} MB";
    // }
}