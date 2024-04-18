using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using LightBuzz.BodyTracking;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.AvaSci;
using LightBuzz.AvaSci.UI;
using UnityEngine.UI;
using DG.Tweening;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Globalization;
public class ReferenceManager : MonoBehaviour
{
    public bool isProduction;
    public GameObject DebugButton;
    public TMP_Text AppVersion;
    public static ReferenceManager instance;
    public LoginManager LoginManager;
    public LoadingManager LoadingManager;
    public ProgressManager ProgressManager;
    public PopupManager PopupManager;
    public ForgetPasswordManager forgetPasswordManager;
    public GameObject SigninPanel;
    public GameObject SignupPanel;
    public ParentChangerDropDown CountryParentDropDown;
    public TMP_Text UsernameText;

    public List<GraphManager> graphManagers = new List<GraphManager>();
    public GraphManager GraphmanagerPrefab;
    public TMP_Dropdown ListOfJointsDropDown;
    public GraphMinimizer GraphMinimizer;
    public Dropdown sensorTypeDropDown;
    public int LidarCount;

    public List<Angle2D> AnglesAdded = new List<Angle2D>();
    /// <summary>
    /// Screens
    /// </summary>
    ///
    public GameObject Screen1;

    public Main LightBuzzMain;

    [HideInInspector]
    public bool videoRecorded;
    [HideInInspector]
    public string recorderPath;

    /// <summary>
    /// settings
    /// </summary>
    public Toggle SettingToggle;
    public RectTransform SettingsPanel;
    public UiManager uiManager;
    public AzureStorageManager azureStorageManager;
    public GameObject VideoPlayerView;
    public Text TotalVideoTime;
    public GameObject IAPPAnel;
    public IAPManager iAPManager;
    public VideoPlayerView videoPlayerView;

    public Image UploadingImage;
    public Transform UploaderAnimation;

    private void Awake()
    {
        instance = this;

        AppVersion.text = isProduction ? "" : $"version:{Application.version}";
        if (isProduction)
        {
            AppVersion.transform.parent.gameObject.SetActive(false);
        }
        if (isProduction)
        {
            DebugButton.SetActive(false);
        }
    }
    public bool isDone;
    public bool isShowingRecording;

    public MeasurementSelector measurementSelector;
    public GameObject VideoPlayButton;
    public async void SwitchToLidar()
    {
#if !UNITY_EDITOR
        if (ReferenceManager.instance.sensorTypeDropDown.value != 1 && !isDone && !isShowingRecording)
        {
            ReferenceManager.instance.LoadingManager.Show("Setting Up Lidar Camera Please Wait...");
            await System.Threading.Tasks.Task.Delay(2000);
            if (LidarCount != 0)
            {


                ReferenceManager.instance.sensorTypeDropDown.value = 1;


            }
            isDone = true;
            ReferenceManager.instance.LoadingManager.Hide();
        }
#endif

    }
    public void GenerateGraph(MeasurementType measurementType)
    {

        List<string> jointTypes = new List<string>();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        jointTypes = Enum.GetNames(typeof(MeasurementType)).ToList();
        foreach (var item in jointTypes)
        {
            if (Enum.GetName(typeof(MeasurementType), measurementType) == item && !ListOfJointsDropDown.options.Any(x => x.text == item))
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData()
                {
                    text = item
                };
                options.Add(optionData);
            }
        }
        ListOfJointsDropDown.AddOptions(options);

    }

    public void ClearGraphs()
    {
        ListOfJointsDropDown.ClearOptions();
        graphManagers.ForEach(x => Destroy(x.gameObject));
        graphManagers.Clear();

        ListOfJointsDropDown.options.Add(new TMP_Dropdown.OptionData() { text = "Select graph type..." });
    }
    public void DisableAllGraphs()
    {
        measurementSelector._toggles.ToList().ForEach(x => x.isOn = false);
    }

    public void OpenSettings(bool value)
    {
        if (value)
        {
            SettingToggle.interactable = false;
            SettingToggle.GetComponent<Image>().color = UnityEngine.Color.gray;
            SettingsPanel.DOAnchorPosX(10000, 0f);
            SettingsPanel.DOAnchorPosX(0, 0.5f).OnComplete(() => SettingToggle.interactable = true);
            SettingsPanel.gameObject.SetActive(true);
        }
        else
        {
            SettingsPanel.DOAnchorPosX(1000, 0.5f).OnComplete(() =>
            {
                SettingToggle.interactable = true;
                SettingsPanel.gameObject.SetActive(false);
                SettingToggle.GetComponent<Image>().color = UnityEngine.Color.white;

            });
        }
    }

    public void StopAllGraphs()
    {
        graphManagers.ForEach(x => x.MySineWave.isReading = false);
    }
    public void UnpauseAllGraphs()
    {
        graphManagers.ForEach(x => x.MySineWave.Start());
    }
    bool ready;
    public async void PlayAllGraphs()
    {
        graphManagers.ForEach(x => x.MySineWave.isReading = false);
        if (!VideoPlayerView.activeSelf)
        {
            return;
        }
        while (!VideoPlayButton.activeSelf || graphManagers.Count == 0 || graphManagers.Any(x => x.MySineWave.graphChart.DataSource.GetMaxXValue() < TimeSpan.ParseExact(TotalVideoTime.text, "mm':'ss", CultureInfo.InvariantCulture).Duration().TotalSeconds))
        {
            if (graphManagers.Count != 0)
            {
                Debug.Log(graphManagers[0].MySineWave.graphChart.DataSource.GetMaxXValue());
                Debug.Log(TimeSpan.ParseExact(TotalVideoTime.text, "mm':'ss", CultureInfo.InvariantCulture).Duration().TotalSeconds);
            }
            // else
            // {
            //     Debug.Log("Count is zero");
            // }
            await Task.Delay(100);
        }

        graphManagers.ForEach(x =>
        {
            x.MySineWave.isReading = false;
            x.MySineWave.graphChart.AutoScrollHorizontally = false;
        });

    }

    public void OnVideoScrolled(float value)
    {
        // if (graphManagers.Count == 0 || graphManagers.Any(x => x.MySineWave.graphChart.DataSource.GetMaxXValue() < TimeSpan.ParseExact(TotalVideoTime.text, "mm':'ss", CultureInfo.InvariantCulture).Duration().TotalSeconds))
        // {
        //     return;
        // }

        // if (graphManagers.Any(x => !x.MySineWave.isReading))
        // {

        Debug.Log(videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds);
        graphManagers.ForEach(x =>
        {
            x.MySineWave.isReading = false;
            x.MySineWave.graphChart.AutoScrollHorizontally = false;
            x.MySineWave.SetReadingValue((float)videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds);

        });


        graphManagers.ForEach(x =>
        {
            if (!x.MySineWave.isReading)
            {
                x.MySineWave.graphChart.HorizontalScrolling = videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds;
            }
        });
        // }
    }
    public void ClearAllGraphs() // we clear the graphs when a new recording is started
    {
        graphManagers.ForEach(x =>
       {
           x.MySineWave.Start();
           x.MySineWave.graphChart.AutoScrollHorizontally = true;
       });
        graphManagers.ForEach(x => x.MySineWave.graphChart.DataSource.Clear());
    }
    private void Update()
    {
        if (videoRecorded)
        {
            if (!isProduction)
            {
                AskToUploadVideo(recorderPath);
            }
            else
            {
                UploadVideo(recorderPath);
                videoRecorded = false;
            }
            videoRecorded = false;
        }
    }
    public void AskToUploadVideo(string path)
    {
        PopupManager.Show("Save Video?", "Would you like the video to be saved to be viewed Later?", false, okPressed: () => UploadVideo(path), true);
    }
    public void UploadVideo(string path)
    {
        var directory = new DirectoryInfo(path);
        var myFiles = directory.GetFiles().ToList();

        List<VideoSaveBody> videoSaveBodies = new List<VideoSaveBody>();
        foreach (var myFile in myFiles)
        {
            using (FileStream fs = myFile.OpenRead())
            {
                byte[] bytes;
                using (var memoryStream = new MemoryStream())
                {
                    fs.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }

                string base64 = Convert.ToBase64String(bytes);
                VideoSaveBody videoSaveBody = new VideoSaveBody()
                {
                    FileName = myFile.Name,
                    FileData = base64
                };
                videoSaveBodies.Add(videoSaveBody);
            }
        }
        string json = JsonConvert.SerializeObject(videoSaveBodies);
        azureStorageManager.UploadVideo(json, $"{GeneralStaticManager.GlobalVar["UserName"]}_");

    }
}
