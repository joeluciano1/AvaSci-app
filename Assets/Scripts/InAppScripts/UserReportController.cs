using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LightBuzz.AvaSci;
using LightBuzz.AvaSci.UI;
using LightBuzz.BodyTracking;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
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

    // Start is called before the first frame update
    public void Start()
    {

        APIHandler.instance.Get("UserReport/GetReports", onSuccess: (response) =>
        {
            UserReportResponse userReportResponse = JsonConvert.DeserializeObject<UserReportResponse>(response);
            if (userReportResponse.isSuccess)
            {
                foreach (var item in userReportResponse.result)
                {
                    var user = userReportFromDBs.FirstOrDefault(x => x.VideoURL == item.VideoURL);
                    if (user != null)
                    {
                        continue;
                    }
                    UserReportFromDB userReportFromDB = Instantiate(userReportFromDBPrefab, userReportFromDBPrefab.transform.parent);
                    userReportFromDB.VideoURL = item.VideoURL;
                    userReportFromDB.gameObject.SetActive(true);
                    userReportFromDB.UserName.text = item.UserName;
                    userReportFromDB.CreatedOn.text = item.CreatedOn;
                    StartCoroutine(GetText(item.VideoURL, userReportFromDB.WatchBtn, userReportFromDB));
                    userReportFromDBs.Add(userReportFromDB);
                }
            }
            if (userReportResponse.isError)
            {
                string reasons = "";
                foreach (var item in userReportResponse.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }
                ReferenceManager.instance.PopupManager.Show("Fetching Users Failed!", $"Reasons are: {reasons}");
                Debug.Log($"{userReportResponse.serviceErrors}");
            }

        }, onError: (error) =>
        {
            ReferenceManager.instance.PopupManager.Show("Fetching Users Failed!", $"Reasons are: {error}");
            Debug.LogError($"Error: {error}");
        });
    }
    public void CreateNew()
    {
        if (videoPlayerView.gameObject.activeSelf)
        {
            videoPlayerView.OnClose();
        }
        RecorderView.gameObject.SetActive(false);
        LightBuzzViewer.SetActive(false);
        Start();
    }
    public void ReallyCreateNew()
    {
        ReferenceManager.instance.isShowingRecording = false;
        videoRecorderView.Show();
    }
    List<UnityWebRequest> requests = new List<UnityWebRequest>();
    float progress;
    IEnumerator GetText(string url, Button btn, UserReportFromDB userReportFromDB)
    {
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
            List<VideoSaveBody> videoSaveBodies = JsonConvert.DeserializeObject<List<VideoSaveBody>>(request.downloadHandler.text);

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => CreateFileAndView(videoSaveBodies));
            btn.interactable = true;

            // Show results as text
            Debug.Log(request.downloadHandler.text);


            // Or retrieve results as binary data
            // byte[] results = www.downloadHandler.data;
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
            var matchingNames = userReportFromDBs.Where(x => x.UserName.text.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
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
                    if (item.ProgressImage.fillAmount == 1)
                    {
                        item.ProgressImage.gameObject.SetActive(false);
                        item.ButtonText.text = $"Watch";
                    }
                }
            }
        }
    }

    public async void CreateFileAndView(List<VideoSaveBody> videoSaveBodies)
    {
        string path1 = System.IO.Path.Combine(Application.persistentDataPath, "Video");
        ReferenceManager.instance.isShowingRecording = true;

        if (Directory.Exists(path1))
        {
            Directory.Delete(path1, recursive: true);
        }

        Directory.CreateDirectory(path1);
        foreach (var item in videoSaveBodies)
        {
            string fileName = item.FileName;
            string fileData = item.FileData;

            string path = System.IO.Path.Combine(Application.persistentDataPath, "Video", fileName);
            byte[] bytes = System.Convert.FromBase64String(fileData);
            File.WriteAllBytes(path, bytes);


        }
        var filePaths = Directory.GetFiles(path1);
        while (filePaths.Length < videoSaveBodies.Count)
        {
            await Task.Delay(500);
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
    // Update is called once per frame

}
