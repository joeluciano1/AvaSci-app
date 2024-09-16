using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using LightBuzz.AvaSci;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.AvaSci.UI;
using LightBuzz.BodyTracking;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReferenceManager : MonoBehaviour
{
	public long? SelectedVideoID;
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
	public ButtonHandler ButtonHandler;
	public CommentQuestionnaire commentQuestionnaire;

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
	public VideoRecordingView videoRecordingView;

	public Image UploadingImage;
	public Transform UploaderAnimation;

	public VideoViewSlider videoSlider;
	public Transform RightSideContents;
	public Transform LeftSideContents;
	public Canvas canvas;
	public Transform LightBuzzPanel;
	public Dropdown OptimizationModeDropDown;

	public bool isDone;
	public bool isShowingRecording;

	public MeasurementSelector measurementSelector;
	public GameObject VideoPlayButton;
	public PlayButton LightBuzzVideoPlayerButton;
	public float Timer;
	bool timerStarted;
	Coroutine coroutine;

	public Dictionary<string, float> maxAngleAtFootStrikingTime = new Dictionary<string, float>();
	public Dictionary<string, float> maxDistanceAtFootStrikingTime = new Dictionary<string, float>();
	public List<HeelPressDetectionBody> heelPressDetectionBodies = new List<HeelPressDetectionBody>();
	public List<StandingDetectionBody> standingDetectionBodies = new List<StandingDetectionBody>();
	public Dictionary<string, float> AngleAtFootStrikingTime = new Dictionary<string, float>();
	public Dictionary<string, float> DistanceAtFootStrikingTime = new Dictionary<string, float>();
	public AngleManager angleManager;
	public Text TimeElapsedLightBuzz;
	// [HideInInspector]
	public Angle2D LeftDistance;
	// [HideInInspector]
	public Angle2D LeftAngleDifference;
	// [HideInInspector]
	public Angle2D RightDistance;
	// [HideInInspector]
	public Angle2D RightAngleDifference;
	VarusValgusNotifier varusValgusNotifierLeft;
	VarusValgusNotifier varusValgusNotifierRight;
	public VarusValgusNotifier varusValgusNotifierPrefab;
	public int videoPlayingCount;
	public bool placeHeelDetectionValues;
	

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
	public void CleareVarusValgus(bool value = false){
		if (!value)
		{
			varusValgusNotifierLeft?.gameObject.SetActive(false);
			varusValgusNotifierRight?.gameObject.SetActive(false);
		}
	}
	public async void SwitchToLidar()
	{
#if !UNITY_EDITOR
		if (
			ReferenceManager.instance.sensorTypeDropDown.value != 1
			&& !isDone
			&& !isShowingRecording
		)
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
	public void SkipToVideo(float sliderValue){
		videoPlayerView.OnSliderHandle();
		
		videoSlider.value = sliderValue;
		videoSlider.onValueChanged.Invoke(sliderValue);
		videoPlayerView.OnSliderHandle();
	}
	public void GenerateGraph(MeasurementType measurementType)
	{
		List<string> jointTypes = new List<string>();
		List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
		jointTypes = Enum.GetNames(typeof(MeasurementType)).ToList();
		foreach (var item in jointTypes)
		{
			if (
				Enum.GetName(typeof(MeasurementType), measurementType) == item
				&& !ListOfJointsDropDown.options.Any(x => x.text == item)
			)
			{
				TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData() { text = item };
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

		ListOfJointsDropDown.options.Add(
			new TMP_Dropdown.OptionData() { text = "Select graph type..." }
		);
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
			SettingsPanel
				.DOAnchorPosX(1000, 0.5f)
				.OnComplete(() =>
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
		while (
			!VideoPlayButton.activeSelf
			|| graphManagers.Count == 0
			|| graphManagers.Any(x =>
				x.MySineWave.graphChart.DataSource.GetMaxXValue()
				< TimeSpan
					.ParseExact(TotalVideoTime.text, "mm':'ss", CultureInfo.InvariantCulture)
					.Duration()
					.TotalSeconds
			)
		)
		{
			if (graphManagers.Count != 0)
			{
				// Debug.Log(graphManagers[0].MySineWave.graphChart.DataSource.GetMaxXValue());
				// Debug.Log(TimeSpan.ParseExact(TotalVideoTime.text, "mm':'ss", CultureInfo.InvariantCulture).Duration().TotalSeconds);
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
	bool reachedEnd;
	public void OnVideoScrolled(float value)
	{
		// if(videoPlayerView.VideoPlayer.IsPaused){

		// }
		// if(placeHeelDetectionValues)
		// {
		// 	HeelPressDetectionBody videoAtSavedValue = heelPressDetectionBodies.FirstOrDefault(x => x.TimeOfHeelPressed == videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds.ToString());
		// 	ResearchMeasurementManager.instance.PutGaitValuesInDetectedTime(videoAtSavedValue);
		// }
		if ((bool)(ResearchMeasurementManager.instance?.isStarted) && angleManager._angles.ContainsKey(MeasurementType.HipKneeLeftDistance))
		{
			if (ShouldHideSkeleton)
			{
				skeletonManager.Toggle(false);
				angleManager._angles[MeasurementType.HipKneeLeftDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = false;
				angleManager._angles[MeasurementType.HipKneeRightDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = false;
				angleManager._angles[MeasurementType.VarusValgusLeftAngleDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = false;
				angleManager._angles[MeasurementType.VarusValgusRightAngleDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = false;
			}
			else if (!ShouldHideSkeleton)
			{
				skeletonManager.Toggle(true);
				angleManager._angles[MeasurementType.HipKneeLeftDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = true;
				angleManager._angles[MeasurementType.HipKneeRightDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = true;
				angleManager._angles[MeasurementType.VarusValgusLeftAngleDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = true;
				angleManager._angles[MeasurementType.VarusValgusRightAngleDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = true;
			}
		}
		if (value.ToString("0.0") == "0.0")
		{
			if (ResearchMeasurementManager.instance.footOnGroundPosition != null)
			{
				ResearchMeasurementManager.instance.ClearFootOnGroundPosition();
			}
		}
		if(ResearchMeasurementManager.instance.processingNotifier.gameObject.activeSelf)
		{
			ResearchMeasurementManager.instance.processingNotifier.LoadingFill.fillAmount = value;
		}
		if (Math.Abs( 1 - value) <= 0.05f && !reachedEnd)
		{
			videoPlayingCount += 1;
			reachedEnd = true;
		}
		else if(Math.Abs(1 - value) > 0.05f && reachedEnd)
		{
			reachedEnd = false;
		}
		// Debug.Log(videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds + "/" + videoPlayerView.VideoPlayer.Duration.TotalSeconds);
		graphManagers.ForEach(x =>
		{
			if (( videoPlayerView.VideoPlayer.Duration.TotalSeconds - videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds) < 0.2f)
			{
				
				if (!x.MySineWave.isVideoDoneLoading)
				{
					x.MySineWave.isVideoDoneLoading = true;
					x.MySineWave.LoadingScreen.SetActive(false);
				}
			}

			x.MySineWave.isReading = false;
			x.MySineWave.graphChart.AutoScrollHorizontally = false;
			x.MySineWave.SetReadingValue(
				(float)videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds
			);
		});

		graphManagers.ForEach(x =>
		{
			if (!x.MySineWave.isReading)
			{
				x.MySineWave.graphChart.HorizontalScrolling = videoPlayerView
					.VideoPlayer
					.TimeElapsed
					.TotalSeconds;
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
	private void LateUpdate()
	{
		if(videoPlayerView.gameObject.activeSelf)
		TimeElapsedLightBuzz.text = videoPlayerView.VideoPlayer.TimeElapsed.ToString(@"mm\:ss\:fff");
	}
	private void Update()
	{
		
		if (videoRecorded)
		{
			commentQuestionnaire.AskToUpload(recorderPath);
			videoRecorded = false;
		}
	}

	public void AskToUploadVideo(string path)
	{
		PopupManager.Show(
			"Save Video?",
			"Would you like the video to be saved to be viewed Later?",
			false,
			okPressed: () => UploadVideo(path),
			true
		);
	}

	public async void UploadVideo(string path)
	{
		var directory = new DirectoryInfo(path);
		var myFiles = directory.GetFiles().ToList();
		if (graphManagers.Count != 0)
		{
			azureStorageManager.reportURL = "Report Is In Video URL";
			ButtonHandler.dontHidePDF = true;
			ButtonHandler.CaptureRectTransform();
			await Task.Delay(1000);
			var secondDirectory = new DirectoryInfo(Application.persistentDataPath);
			FileInfo reportFile = secondDirectory
				.GetFiles()
				.FirstOrDefault(x => x.Name == "Sample.pdf");
			myFiles.Add(reportFile);
		}
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

	public void SetSensor(int value)
	{
		if (videoRecordingView.Sensor != null)
		{
			videoRecordingView.Sensor.OptimizationMode = (OptimizationMode)value;
			PlayerPrefs.SetString("OptimizationMode", value.ToString());
		}
		else
		{
			Debug.Log("Sensor Yet Not Detected");
			PopupManager.Show(
				"Sensor Not Detected",
				"In order to change the type of recording kindly go to the recording view of the app"
			);
		}
	}

	public void StartTimer()
	{
		timerStarted = true;
		Timer = 0;
		coroutine = StartCoroutine(TimerTick());
	}

	IEnumerator TimerTick()
	{
		while (timerStarted)
		{
			yield return new WaitForSeconds(1);
			Timer += 1;
		}
	}

	public void StopTimer()
	{
		timerStarted = false;
		if (coroutine != null)
			StopCoroutine(coroutine);
	}

	[ContextMenu("PauseVid")]
	public async void PauseTheVideo()
	{
		await Task.Delay(50);
		if (!videoPlayerView.VideoPlayer.IsPaused)
			LightBuzzVideoPlayerButton.onClick.Invoke();
	}

	public void PlayTheVideo()
	{
		if (videoPlayerView.VideoPlayer.IsPaused)
			LightBuzzVideoPlayerButton.onClick.Invoke();
	}
	public SkeletonManager skeletonManager;
	public bool ShouldHideSkeleton;
	public void HideSkeleton()
	{
		ShouldHideSkeleton = true;
		skeletonManager.Toggle(false);
		// skeletonManager.Toggle(false);
		// angleManager._angles[MeasurementType.HipKneeLeftDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = false;
		// angleManager._angles[MeasurementType.HipKneeRightDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = false;
		// angleManager._angles[MeasurementType.VarusValgusLeftAngleDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = false;
		// angleManager._angles[MeasurementType.VarusValgusRightAngleDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = false;
	}
	public void ShowSkeleton()
	{
		ShouldHideSkeleton = false;
		skeletonManager.Toggle(true);
		// skeletonManager.Toggle(true);
		// angleManager._angles[MeasurementType.HipKneeLeftDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = true;
		// angleManager._angles[MeasurementType.HipKneeRightDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = true;
		// angleManager._angles[MeasurementType.VarusValgusLeftAngleDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = true;
		// angleManager._angles[MeasurementType.VarusValgusRightAngleDistance].gameObject.GetComponentInChildren<LineRenderer>().enabled = true;
	}
}
