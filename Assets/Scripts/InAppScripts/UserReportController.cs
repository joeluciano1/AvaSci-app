using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using LightBuzz.AvaSci;
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
                        var user = userReportFromDBs.FirstOrDefault(x =>
                            x.VideoURL == item.VideoURL
                        );
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
                            continue;
                        }
                        UserReportFromDB userReportFromDB = Instantiate(
                            userReportFromDBPrefab,
                            userReportFromDBPrefab.transform.parent
                        );
                        userReportFromDB.videoId = item.Id;
                        userReportFromDB.UserId = item.UserID;
                        userReportFromDB.VideoURL = item.VideoURL;
                        userReportFromDB.gameObject.SetActive(true);
                        userReportFromDB.UserName.text = item.UserName;
                        if (!string.IsNullOrEmpty(item.ReportDescription))
                            userReportFromDB.ReportDescription.text = item.ReportDescription;
                        DateTime serverTime;

                        if (
                            DateTime.TryParseExact(
                                item.CreatedOn,
                                "M/dd/yyyy h:mm:ss tt",
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out serverTime
                            )
                        )
                        {
                            DateTime localTime = ConvertToLocalTime(serverTime);
                            userReportFromDB.CreatedOn.text = localTime.ToString(
                                "MM/dd/yyyy h:mm:ss tt"
                            );
                            // Debug.Log($"Yes: {item.CreatedOn}");
                        }
                        else if (
                            DateTime.TryParseExact(
                                item.CreatedOn,
                                "M/d/yyyy hh:mm:ss tt",
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out serverTime
                            )
                        )
                        {
                            DateTime localTime = ConvertToLocalTime(serverTime);
                            userReportFromDB.CreatedOn.text = localTime.ToString(
                                "MM/dd/yyyy h:mm:ss tt"
                            );
                            Debug.Log($"Yes2: {item.CreatedOn}");
                        }
                        else
                        {
                            Debug.Log($"No: {item.CreatedOn}");
                            userReportFromDB.CreatedOn.text = item.CreatedOn;
                        }

                        userReportFromDB.WatchBtn.interactable = true;
                        if (!PlayerPrefs.GetString("LastVidURL").Equals(item.VideoURL))
                        {
                            userReportFromDB.WatchBtn.onClick.AddListener(
                                () =>
                                    StartCoroutine(
                                        GetText(
                                            item.VideoURL,
                                            userReportFromDB.WatchBtn,
                                            userReportFromDB
                                        )
                                    )
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
                                () => { CreateFileAndView(null, "", userReportFromDB.UserName.text); 
                                ReferenceManager.instance.SelectedVideoID = userReportFromDB.videoId; }
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
                    Debug.Log($"{userReportResponse.serviceErrors}");
                }
            },
            onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show(
                    "Fetching Users Failed!",
                    $"Reasons are: {error}"
                );
                Debug.LogError($"Error: {error}");
            }
        );
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
            Debug.Log(request.error);
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
                () => { CreateFileAndView(videoSaveBodies, url, userReportFromDB.UserName.text); ReferenceManager.instance.SelectedVideoID = userReportFromDB.videoId; }
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
            Debug.Log(fileData);
            File.WriteAllBytes(path, bytes);
        }
        await Task.Delay(3000);
#if UNITY_EDITOR
        System.Diagnostics.Process.Start(path);
        Debug.Log("Is Editor");

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
        Debug.Log(serverTimeString);
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
}
