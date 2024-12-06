using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using LightBuzz.AvaSci;
using LightBuzz.AvaSci.Csv;
using LightBuzz.AvaSci.UI;
using LightBuzz.BodyTracking;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserReportController : MonoBehaviour
{
    public UserReportFromDB userReportFromDBPrefab;
    List<UserReportFromDB> userReportFromDBs = new List<UserReportFromDB>();

    public GameObject ReportPanel;
    public GameObject GraphPanel;
    public Image ImageOfFrame;
    public GameObject LightBuzzViewer;
    public GameObject RecorderView;
    public Main LighbuzzMain;

    public VideoPlayerView videoPlayerView;
    public VideoRecordingView videoRecorderView;

    UserReportFromDB RecentlyPlayedButton;
    public Button StopRecButton;
    public Button ResetButton;
    public VerticalLayoutGroup ReportsLayoutGroup;
    public JointReadingFromDB jointReadingFromDBPrefab;
    public GameObject ReadingViewer;
    public Button CreateCSVButton;
    public List<UserReportFromDB> selectedReadings = new List<UserReportFromDB>();
    // Start is called before the first frame update
    public void Start()
    {
        GetReportsBody getReportsBody = new GetReportsBody()
        {
            UserID = GeneralStaticManager.GlobalVar["UserID"]
        };
        string json = JsonConvert.SerializeObject(getReportsBody);
        Transform itemToSnapTo = null;
        APIHandler.instance.Post(
            "UserReport/GetReports",
            json,
            onSuccess: (response) =>
            {
                UserReportResponse userReportResponse =
                    JsonConvert.DeserializeObject<UserReportResponse>(response);
                     
                if (userReportResponse.isSuccess)
                {
                    foreach (var item in userReportResponse.result)
                    {
                        
                        var user = userReportFromDBs.FirstOrDefault(x =>x.VideoURL == item.VideoURL);
                        if (user != null)
                        {
                            if (!PlayerPrefs.GetString("LastVidURL").Equals(user.VideoURL))
                            {
                                user.WatchBtn.onClick.RemoveAllListeners();
                                user.WatchBtn.onClick.AddListener(
                                    () =>
                                        StartCoroutine(GetText(user.VideoURL, user.WatchBtn, user))
                                );
                                user.ButtonText.text = "Download";
                            }
                            if(item.JointReadings!=null && item.JointReadings.Count > 0)
                            {
                                user.CompareViewButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Select To Compare";
                                user.CompareViewButton.interactable = true;
                                user.CompareViewButton.gameObject.SetActive(true);
                                user.jointReadings = item.JointReadings;
                                user.CompareViewButton.onValueChanged.RemoveAllListeners();
                                user.CompareViewButton.onValueChanged.AddListener((value) => 
                                {
                                    if(value)
                                    {
                                        selectedReadings.Add(user);
                                    }
                                    else{
                                        selectedReadings.Remove(user);
                                    }
                                });
                                // user.CompareViewButton.onClick.AddListener(() => { ShowJointReadingsFromDB(item.JointReadings); ReferenceManager.instance.CompareReadingSelected = user; });
                            }
                            else
                            {
                                user.CompareViewButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "No Reading Exists";
                                user.CompareViewButton.interactable = false;
                            }
                            continue;
                        }
                        UserReportFromDB userReportFromDB = Instantiate(
                            userReportFromDBPrefab,
                            userReportFromDBPrefab.transform.parent
                        );
                        userReportFromDB.videoId = item.Id;
                        userReportFromDB.UserId = item.UserID;
                        userReportFromDB.UserNameOfSubject = item.UserName;
                        userReportFromDB.VideoURL = item.VideoURL;
                        userReportFromDB.gameObject.SetActive(true);
                        if(item.JointReadings!=null && item.JointReadings.Count > 0)
                        {
                            userReportFromDB.jointReadings = item.JointReadings;
                            userReportFromDB.CompareViewButton.interactable = true;
                            userReportFromDB.CompareViewButton.gameObject.SetActive(true);
                             userReportFromDB.CompareViewButton.onValueChanged.RemoveAllListeners();
                                userReportFromDB.CompareViewButton.onValueChanged.AddListener((value) => 
                                {
                                    if(value)
                                    {
                                        selectedReadings.Add(userReportFromDB);
                                    }
                                    else{
                                        selectedReadings.Remove(userReportFromDB);
                                    }
                                });
                            // userReportFromDB.CompareViewButton.onClick.RemoveAllListeners();
                            // userReportFromDB.CompareViewButton.onClick.AddListener(() => { ShowJointReadingsFromDB(item.JointReadings); ReferenceManager.instance.CompareReadingSelected = userReportFromDB; });
                        }
                        else
                        {
                            userReportFromDB.CompareViewButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "No Reading Exists";
                            userReportFromDB.CompareViewButton.interactable = false;
                        }
                        if(string.IsNullOrEmpty(item.SubjectId))
                        userReportFromDB.UserName.text = item.UserName;
                        else
                        userReportFromDB.UserName.text = item.SubjectId;
                        if (!string.IsNullOrEmpty(item.ReportDescription))
                            userReportFromDB.ReportDescription.text = item.ReportDescription;
                        DateTime serverTime;

                        if (DateTime.TryParseExact(item.CreatedOn,"M/dd/yyyy h:mm:ss tt",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out serverTime))
                        {
                            DateTime localTime = ConvertToLocalTime(serverTime);
                            userReportFromDB.CreatedOn.text = localTime.ToString(
                                "MM/dd/yyyy h:mm:ss tt"
                            );
                            // Debug.Log($"Yes: {item.CreatedOn}");
                        }
                        else if (DateTime.TryParseExact(item.CreatedOn,"M/d/yyyy hh:mm:ss tt",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out serverTime))
                        {
                            DateTime localTime = ConvertToLocalTime(serverTime);
                            userReportFromDB.CreatedOn.text = localTime.ToString(
                                "MM/dd/yyyy h:mm:ss tt"
                            );
                            
                        }
                        else
                        {
                            
                            userReportFromDB.CreatedOn.text = item.CreatedOn;
                        }

                        userReportFromDB.WatchBtn.interactable = true;
                        if (!PlayerPrefs.GetString("LastVidURL").Equals(item.VideoURL))
                        {
                            userReportFromDB.WatchBtn.onClick.AddListener(
                                () =>
                                    {
                                        StartCoroutine(
                                        GetText(
                                            item.VideoURL,
                                            userReportFromDB.WatchBtn,
                                            userReportFromDB
                                        )
                                    );
                                        ReferenceManager.instance.azureStorageManager.selectedVideo = userReportFromDB;
                                    }
                            );
                            userReportFromDB.ButtonText.text = "Download";
                        }
                        else
                        {
                            userReportFromDB.WatchBtn.onClick.RemoveAllListeners();
                            userReportFromDB.ButtonText.text = "Watch";
                            itemToSnapTo = userReportFromDB.transform;
                            RecentlyPlayedButton = userReportFromDB;
                            userReportFromDB.WatchBtn.onClick.AddListener(
                                () => { CreateFileAndView(null, "", userReportFromDB.UserNameOfSubject); 
                                ReferenceManager.instance.SelectedVideoID = userReportFromDB.videoId;ReferenceManager.instance.azureStorageManager.selectedVideo = userReportFromDB; }
                            );
                            if (!string.IsNullOrEmpty(item.ReportURL))
                            {
                                userReportFromDB.PreviewButton.interactable = true;
                                userReportFromDB.PreviewButton.gameObject.SetActive(true);
                                userReportFromDB
                                    .PreviewButton.transform.GetChild(0)
                                    .GetComponent<TMP_Text>()
                                    .text = "View Report";
                                userReportFromDB.PreviewButton.onClick.AddListener(
                                    () => CreateReportAndView()
                                );
                            }
                        }

                        userReportFromDBs.Add(userReportFromDB);
                    }
                    if (itemToSnapTo != null)
                        SnapToChild(itemToSnapTo);
                }
                if (userReportResponse.isError)
                {
                    string reasons = "";
                    foreach (var item in userReportResponse.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    ReferenceManager.instance.PopupManager.Show(
                        "Fetching Users Failed!",
                        $"Reasons are: {reasons}"
                    );
                    
                }
            },
            onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show(
                    "Fetching Users Failed!",
                    $"Reasons are: {error}"
                );
                
            }
        );
    }
    List<GameObject> addedJointReadings = new();
    public void ShowJointReadingsFromDB(List<JointReading> jointReading)
    {
        ReadingViewer.SetActive(true);
        addedJointReadings.ForEach(x => Destroy(x.gameObject));
        addedJointReadings.Clear();
        var jointsOnDifferentDates = jointReading.GroupBy(o =>
            {
                // Group by day and rounded timestamp ignoring seconds
                DateTime rounded = new DateTime(o.CreatedOn.Year, o.CreatedOn.Month, o.CreatedOn.Day, o.CreatedOn.Hour, o.CreatedOn.Minute, 0);
                return rounded.ToString("yyyy-MM-dd HH:mm"); // Grouping key
            })
            .ToList();
        
       foreach (var group in jointsOnDifferentDates)
        {
            // Create a group header
            
            JointReadingFromDB groupHeader = Instantiate(jointReadingFromDBPrefab, jointReadingFromDBPrefab.transform.parent);
            addedJointReadings.Add(groupHeader.gameObject);
            groupHeader.readingDate.text = $"{group.ElementAt(0).VideoNameLink} \nRecorded at: {group.Key}";
            // groupHeader.readingValues.text = "<u>Name Of Reading</u>\t\t| <u>Mini Value</u>\t| <u>Max Value</u>\t| <u>Range</u>";
            // Create items for each object in the group
            foreach (var obj in group)
            {
                JointReadingInfoSection jointReadingInfoSection = Instantiate(groupHeader.jointReadingInfoSectionPrefab, groupHeader.jointReadingInfoSectionPrefab.transform.parent);
                jointReadingInfoSection.NameOfReading.text = obj.NameOfReading;
                jointReadingInfoSection.MinValue.text = obj.MinimumValue.ToString();
                jointReadingInfoSection.MaxValue.text = obj.MaximumValue.ToString();
                jointReadingInfoSection.RangeValue.text = obj.RangeValue.ToString();
            }
            groupHeader.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(groupHeader.GetComponent<RectTransform>());
        }
        CreateCSVButton.onClick.RemoveAllListeners();
        CreateCSVButton.onClick.AddListener(() => CreateCSV($"{selectedReadings[0].UserName.text}_{DateTime.Now.ToShortDateString().Replace("/","-")}_JointReading.csv", jointReading));
    }
     void CreateCSV(string fileName, List<JointReading> readings)
    {
        // Path to save the file
        string filePath = Path.Combine(Application.dataPath,"ExportedData", fileName);

        // Use StringBuilder for efficient CSV generation
        StringBuilder csvContent = new StringBuilder();

        // Add header row
        csvContent.AppendLine("Name of Video,Name Of Reading,Mini Value,Max Value,Range,Readings Taken Date");

        // Add data rows
        foreach (var reading in readings)
        {
            csvContent.AppendLine($"{reading.VideoNameLink},{reading.NameOfReading},{reading.MinimumValue},{reading.MaximumValue},{reading.RangeValue},{ConvertToLocalTime(reading.CreatedOn)}");
        }

        // Write the CSV content to the file
        File.WriteAllText(filePath, csvContent.ToString());

        // Log the file path
        
        CSVManager.Export(filePath);
        RunRScript(filePath);
    }
     public void RunRScript(string csvPath)
    {
         // Paths
        string rScriptExecutable = "/usr/local/bin/Rscript"; // Full path to Rscript
        string rScriptPath = Path.Combine(Application.dataPath, "Scripts/R/generate_report.R");
        string outputPath = Path.Combine(Application.dataPath, "ExportedData/RReports");

        // Ensure output directory exists
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // Log paths
        UnityEngine.Debug.Log($"R Script Path: {rScriptPath}");
        UnityEngine.Debug.Log($"CSV Path: {csvPath}");
        UnityEngine.Debug.Log($"Output Path: {outputPath}");

        // Check if files exist
        if (!File.Exists(rScriptExecutable))
        {
            UnityEngine.Debug.LogError("Rscript not found at " + rScriptExecutable);
            return;
        }

        if (!File.Exists(rScriptPath))
        {
            UnityEngine.Debug.LogError("R script not found at " + rScriptPath);
            return;
        }

        // Run R script
        Process process = new Process();
        process.StartInfo.FileName = rScriptExecutable; // Full path to Rscript
        process.StartInfo.Arguments = $"\"{rScriptPath}\" \"{csvPath}\" \"{outputPath}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();

        // Capture output and errors
        string output = process.StandardOutput.ReadToEnd();
        string errors = process.StandardError.ReadToEnd();
        process.WaitForExit();

        // Log output and errors
        UnityEngine.Debug.Log("R script output: " + output);
        if (!string.IsNullOrEmpty(errors))
        {
            UnityEngine.Debug.LogError("R script errors: " + errors);
        }
    }
    public void CompareSelectedReadings()
    {
        if(selectedReadings.Count == 0)
        {
            ReferenceManager.instance.PopupManager.Show("No Reading Selected", "Please select a reading or multiple readings from the reports section to compare them");
            return;
        }
        List<JointReading> listofJointReadings = selectedReadings.SelectMany(x=>x.jointReadings).ToList();
        ShowJointReadingsFromDB(listofJointReadings);
    }
    public void CreateNew()
    {
        if (videoPlayerView.gameObject.activeSelf)
        {
            videoPlayerView.OnClose();
        }
        RecorderView.gameObject.SetActive(false);
        if (!GeneralStaticManager.GlobalVar.ContainsKey("Subject"))
            GeneralStaticManager.GlobalVar.Add("Subject", "");
        else
            GeneralStaticManager.GlobalVar["Subject"] = "";
        LightBuzzViewer.SetActive(false);
        StopRecButton.onClick.Invoke();
        ResetButton.onClick.Invoke();
        Start();
    }

    public void ReallyCreateNew()
    {
        ResearchMeasurementManager.instance.footDistances.Clear();
        ResearchMeasurementManager.instance.footStrikeAtTimes.Clear();
        ReferenceManager.instance.heelPressDetectionBodies.Clear();
        ReferenceManager.instance.isShowingRecording = false;
        ReferenceManager.instance.SelectedVideoID = null;
        ReferenceManager.instance.CleareVarusValgus();
        ReferenceManager.instance.videoPlayingCount = 0;
        if(ReferenceManager.instance.videoRecordingView.Sensor !=null)
        ReferenceManager.instance.videoRecordingView.Sensor.OptimizationMode = 0;
        ReferenceManager.instance.sensorTypeDropDown.SetValueWithoutNotify(0);
        videoRecorderView.Show();
    }

    List<UnityWebRequest> requests = new List<UnityWebRequest>();
    float progress;

    IEnumerator GetText(string url, Button btn, UserReportFromDB userReportFromDB)
    {
        ClearLastPlayedVideoData();
        btn.interactable = false;
        UnityWebRequest request = UnityWebRequest.Get(url);
        userReportFromDB.request = request;
        requests.Add(request);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            btn.transform.GetChild(0).GetComponent<TMP_Text>().text = "Retry? No Data Found";
            userReportFromDB.ProgressImage.fillAmount = 0;
            userReportFromDB.ProgressImage.gameObject.SetActive(false);
            requests.Remove(request);
            userReportFromDB.request.Dispose();
            userReportFromDB.request = null;
            btn.onClick.AddListener(() => StartCoroutine(GetText(url, btn, userReportFromDB)));
            btn.interactable = true;
            // ReferenceManager.instance.LoadingManager.Hide();
        }
        else
        {
            List<VideoSaveBody> videoSaveBodies = JsonConvert.DeserializeObject<
                List<VideoSaveBody>
            >(request.downloadHandler.text);
            var reportFile = videoSaveBodies.FirstOrDefault(x => x.FileName.Equals("Sample.pdf"));
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(
                () => { CreateFileAndView(videoSaveBodies, url, userReportFromDB.UserNameOfSubject); ReferenceManager.instance.SelectedVideoID = userReportFromDB.videoId; ReferenceManager.instance.azureStorageManager.selectedVideo = userReportFromDB; }
            );
            btn.interactable = true;
            userReportFromDB.ProgressImage.gameObject.SetActive(false);
            userReportFromDB.ButtonText.text = $"Watch";
            if (reportFile != null)
            {
                userReportFromDB.PreviewButton.onClick.RemoveAllListeners();
                userReportFromDB.PreviewButton.interactable = true;
                userReportFromDB.PreviewButton.gameObject.SetActive(true);
                userReportFromDB.PreviewButton.transform.GetChild(0).GetComponent<TMP_Text>().text =
                    "View Report";
                userReportFromDB.PreviewButton.onClick.AddListener(
                    () => CreateReportAndView(reportFile)
                );
            }
            if (userReportFromDB.request.downloadProgress == 0)
            {
                userReportFromDB.ButtonText.text = "Retry? No Data Found";
            }
            requests.Remove(userReportFromDB.request);
            userReportFromDB.request.Dispose();
            userReportFromDB.request = null;
            // Show results as text
            // Debug.Log(request.downloadHandler.text);


            // Or retrieve results as binary data
            // byte[] results = www.downloadHandler.data;
        }
    }

    public void ClearLastPlayedVideoData()
    {
        if (RecentlyPlayedButton != null)
        {
            RecentlyPlayedButton.WatchBtn.onClick.RemoveAllListeners();
            RecentlyPlayedButton.WatchBtn.onClick.AddListener(
                () =>
                    StartCoroutine(
                        GetText(
                            PlayerPrefs.GetString("LastVidURL"),
                            RecentlyPlayedButton.WatchBtn,
                            RecentlyPlayedButton
                        )
                    )
            );
            RecentlyPlayedButton.ButtonText.text = "Download";
            RecentlyPlayedButton.PreviewButton.gameObject.SetActive(false);
            PlayerPrefs.SetString("LastVidURL", "None");
        }
    }

    public void SearchUser(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            userReportFromDBs.ForEach(x => x.gameObject.SetActive(true));
        }
        else
        {
            userReportFromDBs.ForEach(x => x.gameObject.SetActive(false));
            var matchingNames = userReportFromDBs
                .Where(x => x.UserName.text.Contains(name, StringComparison.OrdinalIgnoreCase)|| x.ReportDescription.text.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var item in matchingNames)
            {
                item.gameObject.SetActive(true);
            }
        }
    }

    private void LateUpdate()
    {
        foreach (var item in userReportFromDBs)
        {
            if (item.request != null)
            {
                var request = requests.FirstOrDefault(x => x.url == item.request.url);
                if (request != null)
                {
                    item.ProgressImage.fillAmount = request.downloadProgress;

                    item.ButtonText.text = $"{Math.Round(request.downloadProgress, 4) * 100}%";
                    if (item.ProgressImage.fillAmount >= 0.95f)
                    {
                        item.ProgressImage.gameObject.SetActive(false);
                        item.ButtonText.text = $"Watch";
                        if (request.downloadProgress == 0)
                        {
                            item.ButtonText.text = "Retry? No Data Found";
                        }
                    }
                }
            }
        }
    }

    public async void CreateReportAndView(VideoSaveBody videoSaveBody = null)
    {
        string path1 = Application.persistentDataPath;
        string path = System.IO.Path.Combine(Application.persistentDataPath, "Sample.pdf");
        ReferenceManager.instance.isShowingRecording = true;
        if (videoSaveBody != null)
        {
            string filename = videoSaveBody.FileName;
            string fileData = videoSaveBody.FileData;

            path = System.IO.Path.Combine(Application.persistentDataPath, filename);
            byte[] bytes = System.Convert.FromBase64String(fileData);
            
            File.WriteAllBytes(path, bytes);
        }
        await Task.Delay(3000);
#if UNITY_EDITOR
        System.Diagnostics.Process.Start(path);
#else

        string url = "file://" + path.Replace(" ", "%20");
        Debug.Log("URL = " + url);
        Debug.Log("Persistance = " + path);
        GeneralStaticManager.OpenFile(path);
#endif
    }

    public async void CreateFileAndView(
        List<VideoSaveBody> videoSaveBodies = null,
        string url = "",
        string username = ""
    )
    {
        if (!GeneralStaticManager.GlobalVar.ContainsKey("Subject"))
            GeneralStaticManager.GlobalVar.Add("Subject", username);
        else
            GeneralStaticManager.GlobalVar["Subject"] = username;
        string path1 = System.IO.Path.Combine(Application.persistentDataPath, "Video");
        
        ReferenceManager.instance.isShowingRecording = true;
        if (!string.IsNullOrEmpty(url))
        {
            PlayerPrefs.SetString("LastVidURL", url);
        }
        if (Directory.Exists(path1) && videoSaveBodies != null)
        {
            Directory.Delete(path1, recursive: true);
        }
        if (videoSaveBodies != null)
        {
            Directory.CreateDirectory(path1);
            foreach (var item in videoSaveBodies)
            {
                if (item.FileName.Equals("Sample.pdf"))
                {
                    continue;
                }
                string fileName = item.FileName;
                string fileData = item.FileData;

                string path = System.IO.Path.Combine(
                    Application.persistentDataPath,
                    "Video",
                    fileName
                );
                byte[] bytes = System.Convert.FromBase64String(fileData);
                File.WriteAllBytes(path, bytes);
            }

            var filePaths = Directory.GetFiles(path1);
            while (filePaths.Length < videoSaveBodies.Count - 1)
            {
                await Task.Delay(500);
            }
        }
        ReportPanel.SetActive(false);
        GraphPanel.SetActive(true);
        videoRecorderView.Show();

        while (!LightBuzzViewer.activeSelf)
        {
            await Task.Delay(500);
        }
        await Task.Delay(1000);

        videoPlayerView.Options.Path = path1;
        LighbuzzMain._videoRecorderView._videoPath = path1;
        LighbuzzMain.OnRecordingCompleted();
    }

    public static byte[] ReadFully(Stream input)
    {
        byte[] buffer = new byte[16 * 1024];
        using (MemoryStream ms = new MemoryStream())
        {
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }
    }

    DateTime ParseServerTime(string serverTimeString)
    {
        // Define the expected format of the server time
        string format = "MM/dd/yyyy h:mm:ss tt";
        // Parse the server time string into a DateTime object
        DateTime serverTime = DateTime.ParseExact(
            serverTimeString,
            format,
            System.Globalization.CultureInfo.InvariantCulture
        );
        return serverTime;
    }

    DateTime ConvertToLocalTime(DateTime serverTime)
    {
        // Assuming the server time is in UTC, convert it to local time
        TimeZoneInfo localZone = TimeZoneInfo.Local;

        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(serverTime, localZone);
        return localTime;
    }

    public RectTransform _contentPanel;
    public ScrollRect _scrollRect;
    public float offset;

    private async void SnapToChild(Transform stage)
    {
        await Task.Delay(500);
        Canvas.ForceUpdateCanvases();

        Vector2 endValue =
            (Vector2)_scrollRect.transform.InverseTransformPoint(_contentPanel.position)
            - (Vector2)_scrollRect.transform.InverseTransformPoint(stage.position);

        endValue.x = 0;
        endValue.y -= offset;

        _contentPanel.DOAnchorPos(endValue, 1f);
    }

    public void OrderBy(int order)
    {
        if(order == 1)
        {
            ReportsLayoutGroup.reverseArrangement = false;
            userReportFromDBs = userReportFromDBs.OrderBy(x => x.UserName.text).ToList();
            for(int i = 0;i<userReportFromDBs.Count;i++)
            {
                userReportFromDBs[i].transform.SetSiblingIndex(i);
            }
        }
        if(order == 2)
        {
            userReportFromDBs = userReportFromDBs.OrderBy(x => DateTime.Parse(x.CreatedOn.text)).ToList();
            for(int i = 0;i<userReportFromDBs.Count;i++)
            {
                userReportFromDBs[i].transform.SetSiblingIndex(i);
            }
            ReportsLayoutGroup.reverseArrangement = false;
        }
        if(order == 3)
        {
            userReportFromDBs = userReportFromDBs.OrderBy(x => DateTime.Parse(x.CreatedOn.text)).ToList();
            for(int i = 0;i<userReportFromDBs.Count;i++)
            {
                userReportFromDBs[i].transform.SetSiblingIndex(i);
            }
            ReportsLayoutGroup.reverseArrangement = true;
        }
    }
}
