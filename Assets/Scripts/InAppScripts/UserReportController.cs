using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using FastForward.CAS;
using LightBuzz.AvaSci;
using LightBuzz.AvaSci.Csv;
using LightBuzz.AvaSci.UI;
using LightBuzz.BodyTracking;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserReportController : MonoBehaviour
{
    public UserReportFromDB userReportFromDBPrefab;
    [HideInInspector]public List<UserReportFromDB> userReportFromDBs = new List<UserReportFromDB>();
    public ReportGroupHandler reportGroupHandlerPrefab;
    public List<ReportGroupHandler> addedReportGroupHandlers = new List<ReportGroupHandler>();

    public GameObject ReportPanel;
    public GameObject GraphPanel;
    public Image ImageOfFrame;
    public GameObject LightBuzzViewer;
    public GameObject RecorderView;
    public Main LighbuzzMain;

    public VideoPlayerView videoPlayerView;
    public VideoRecordingView videoRecorderView;

    public UserReportFromDB RecentlyPlayedButton;
    public Button StopRecButton;
    public Button ResetButton;
    // public VerticalLayoutGroup ReportsLayoutGroup;
    public JointReadingFromDB jointReadingFromDBPrefab;
    public TimeBasedReadingFromDB timeBasedReadingFromDBPrefab;
    public GameObject ReadingViewer;
    public Button CreateCSVButton;
    public List<UserReportFromDB> selectedReadings = new List<UserReportFromDB>();
    public List<UserReportFromDB> selectedGaitReadings = new List<UserReportFromDB>();
    public ChatGPTHandler chatGPTHandler;
    public UserReportFromDB itemToSnapTo;
    public ScrollRect reportsParentScrollRect;

    private bool hasSpokenAboutReports;

    private bool hasSpokenWelcomeNote;

    public GameObject UploadHtmlPrompt;
    public Button UploadHtmlButton;

    public TMP_InputField HtmlFileName;
    public TMP_InputField UserEmailToShareWith;

    public GameObject ShareRecordingPopup;
    public Button ShareRecordingButton;

    public bool isSilentReportFetch;
    // Start is called before the first frame update
    public void Start()
    {
        if (!hasSpokenWelcomeNote)
        {
            hasSpokenWelcomeNote = true;
            ReferenceManager.instance.TTSTutorialHandler.NextLine(
                "Hello welcome to AvaSci research project app. Getting reports from our server. This might take a little while.");
        }

        GetReportsBody getReportsBody = new GetReportsBody()
        {
            UserID = GeneralStaticManager.GlobalVar["UserID"]
        };
        string json = JsonConvert.SerializeObject(getReportsBody);
        itemToSnapTo = null;
        APIHandler.instance.Post(
            "UserReport/GetReports",
            json,
            onSuccess: (response) =>
            {
                isSilentReportFetch = false;
                UserReportResponse userReportResponse =
                    JsonConvert.DeserializeObject<UserReportResponse>(response);
                     
                if (userReportResponse.isSuccess)
                {
                    foreach (var item in userReportResponse.result)
                    {
                        
                        UserReportFromDB user = userReportFromDBs.FirstOrDefault(x =>x.VideoURL == item.VideoURL && x.videoId == item.Id);
                        if (user != null)
                        {
                            if(string.IsNullOrEmpty(item.SubjectId))
                                user.UserNamefromDB.text = item.UserName;
                            else
                                user.UserNamefromDB.text = item.SubjectId;
                            if (!string.IsNullOrEmpty(item.ReportDescription))
                                user.ReportDescription.text = item.ReportDescription;
                            
                            user.UserNameOfSubject = item.UserName;
                            
                            if (item.HasHtmlReports)
                            {
                                user.HtmlButton.gameObject.SetActive(true);
                            }
                            else
                            {
                                user.HtmlButton.gameObject.SetActive(false);
                            }
                            // if (!PlayerPrefs.GetString("LastVidURL").Equals(user.VideoURL) || !PlayerPrefs.GetInt("LastVidID").Equals(user.videoId))
                            // {
                            //     user.WatchBtn.onClick.RemoveAllListeners();
                            //     user.WatchBtn.onClick.AddListener(
                            //         () =>
                            //             StartCoroutine(GetText(user.VideoURL, user.WatchBtn, user))
                            //     );
                            //     user.ButtonText.text = "Download";
                            //     user.Download.SetActive(true);
                            //     user.Error.SetActive(false);
                            //     user.Watch.SetActive(false);
                            // }
                            if(item.TimeBasedReadings!=null && item.TimeBasedReadings.Count > 0)
                            {
                                user.CompareViewToggle.transform.GetChild(0).GetComponent<TMP_Text>().text = "Select To Compare";
                                user.CompareViewToggle.interactable = true;
                                // user.jointReadings = item.JointReadings;
                                user.timeBasedReadings = item.TimeBasedReadings;
                                user.timeBasedReadings.ForEach(x=>
                                {
                                    x.VideoName = item.ReportDescription;
                                    
                                });
                                
                                user.CompareViewToggle.onValueChanged.RemoveAllListeners();
                                user.CompareViewToggle.onValueChanged.AddListener((value) => 
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
                                user.CompareViewToggle.transform.GetChild(0).GetComponent<TMP_Text>().text = "No Reading Exists";
                                user.CompareViewToggle.interactable = false;
                            }

                            if (item.GaitReports != null && item.GaitReports.Count > 0)
                            {
                                user.CompareGaitToggle.transform.GetChild(0).GetComponent<TMP_Text>().text = "Select To Compare Gait";
                                user.CompareGaitToggle.interactable = true;
                                user.gaitReports = item.GaitReports.ToList();
                                user.gaitReports.ForEach(x=>x.VideoName = item.ReportDescription);
                                user.CompareGaitToggle.onValueChanged.RemoveAllListeners();
                                user.CompareGaitToggle.onValueChanged.AddListener((value) =>
                                {
                                    if (value)
                                    {
                                        selectedGaitReadings.Add(user);
                                    }
                                    else
                                    {
                                        selectedGaitReadings.Remove(user);
                                    }
                                });
                            }
                            else
                            {
                                user.CompareGaitToggle.transform.GetChild(0).GetComponent<TMP_Text>().text = "No Gait Reading Exists";
                                user.CompareGaitToggle.interactable = false;
                            }
                            continue;
                        }
                        UserReportFromDB userReportFromDB = Instantiate(
                            userReportFromDBPrefab,
                            userReportFromDBPrefab.transform.parent
                        );
                        userReportFromDB.mydata = item;
                        
                        userReportFromDB.videoId = item.Id;
                        userReportFromDB.UserId = item.UserID;
                        userReportFromDB.UserNameOfSubject = item.UserName;
                        userReportFromDB.VideoURL = item.VideoURL;
                        
                        if (item.HasHtmlReports)
                        {
                            userReportFromDB.HtmlButton.gameObject.SetActive(true);
                        }
                        else
                        {
                            userReportFromDB.HtmlButton.gameObject.SetActive(false);
                        }
                        string groupName = string.IsNullOrEmpty(item.GroupName)? "Other" : item.GroupName;
                        ReportGroupHandler alreadyExisting =
                            addedReportGroupHandlers.FirstOrDefault(x => x.GroupName.text == groupName);
                        
                        if (alreadyExisting != null)
                        {
                            userReportFromDB.transform.SetParent(alreadyExisting.MyContent, false);
                            alreadyExisting.DropDownItems.Add(userReportFromDB);
                            userReportFromDB.MyReportGroupHandler = alreadyExisting;
                            userReportFromDB.MyScrollRect = alreadyExisting.MyScrollRect;
                            alreadyExisting.ScaleDownItems();
                            alreadyExisting.ForceRebuildLayout();
                            if (alreadyExisting.isDropped)
                            {
                                alreadyExisting.DropDownItems.ForEach(x=>
                                {
                                    x.transform.SetParent(alreadyExisting.ShowcaseScrollRect.content,false);
                                    x.MyScrollRect = alreadyExisting.ShowcaseScrollRect;
                                });
                            }
                            if (alreadyExisting.DropDownItems.Contains(ReferenceManager.instance.userReportController.itemToSnapTo) && ReferenceManager.instance.userReportController.itemToSnapTo.gameObject.activeSelf)
                            {
                                ReferenceManager.instance.userReportController.SnapToChild(
                                    ReferenceManager.instance.userReportController.itemToSnapTo.transform,
                                    ReferenceManager.instance.userReportController.itemToSnapTo.MyScrollRect,
                                    ReferenceManager.instance.userReportController.itemToSnapTo.MyScrollRect.content);
                            }
                        }
                        else
                        {
                            ReportGroupHandler groupHandler = Instantiate(reportGroupHandlerPrefab, reportGroupHandlerPrefab.transform.parent);
                            groupHandler.gameObject.SetActive(true);
                            groupHandler.GroupName.text = groupName;
                            userReportFromDB.MyReportGroupHandler = groupHandler;
                            userReportFromDB.MyScrollRect = groupHandler.MyScrollRect;
                            groupHandler.DropDownItems.Add(userReportFromDB);
                            userReportFromDB.transform.parent = groupHandler.MyContent;
                            addedReportGroupHandlers.Add(groupHandler);
                            groupHandler.ScaleDownItems();
                            groupHandler.ForceRebuildLayout();
                        }
                        userReportFromDB.gameObject.SetActive(true);
                        if(item.TimeBasedReadings!=null && item.TimeBasedReadings.Count > 0)
                        {
                            userReportFromDB.timeBasedReadings = item.TimeBasedReadings.ToList();
                            userReportFromDB.CompareViewToggle.transform.GetChild(0).GetComponent<TMP_Text>().text = "Select To Compare";
                            userReportFromDB.timeBasedReadings.ForEach(x=>
                            {
                                x.VideoName = item.ReportDescription;
                                
                            });
                            userReportFromDB.CompareViewToggle.interactable = true;
                            if (item.Id == 235)
                            {
                                Debug.Log("Got true");
                            }
                            userReportFromDB.CompareViewToggle.onValueChanged.RemoveAllListeners();
                                userReportFromDB.CompareViewToggle.onValueChanged.AddListener((value) => 
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
                            userReportFromDB.CompareViewToggle.transform.GetChild(0).GetComponent<TMP_Text>().text = "No Reading Exists";
                            userReportFromDB.CompareViewToggle.interactable = false;
                        }
                        if (item.GaitReports != null && item.GaitReports.Count > 0)
                        {
                            // item.GaitReports = item.GaitReports.OrderBy(x => x.FootStrikeAtTime).ToList();
                            userReportFromDB.CompareGaitToggle.transform.GetChild(0).GetComponent<TMP_Text>().text = "Select To Compare Gait";
                            userReportFromDB.CompareGaitToggle.interactable = true;
                            userReportFromDB.gaitReports = item.GaitReports.ToList();
                            userReportFromDB.gaitReports.ForEach(x=>x.VideoName = item.ReportDescription);
                            userReportFromDB.CompareGaitToggle.onValueChanged.RemoveAllListeners();
                            userReportFromDB.CompareGaitToggle.onValueChanged.AddListener((value) =>
                            {
                                if (value)
                                {
                                    selectedGaitReadings.Add(userReportFromDB);
                                }
                                else
                                {
                                    selectedGaitReadings.Remove(userReportFromDB);
                                }
                            });
                        }
                        else
                        {
                            userReportFromDB.CompareGaitToggle.transform.GetChild(0).GetComponent<TMP_Text>().text = "No Gait Reading Exists";
                            userReportFromDB.CompareGaitToggle.interactable = false;
                        }
                        // if(string.IsNullOrEmpty(item.SubjectId))
                        //     userReportFromDB.UserNamefromDB.text = item.UserName;
                        // else
                            userReportFromDB.UserNamefromDB.text = item.SubjectId;
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
                        if (!PlayerPrefs.GetString("LastVidURL").Equals(item.VideoURL) || !PlayerPrefs.GetInt("LastVidID").Equals((int)item.Id))
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
                            userReportFromDB.Download.SetActive(true);
                            userReportFromDB.Error.SetActive(false);
                            userReportFromDB.Watch.SetActive(false);
                        }
                        else if(PlayerPrefs.GetString("LastVidURL").Equals(userReportFromDB.VideoURL)&& PlayerPrefs.GetInt("LastVidID").Equals((int)item.Id))
                        {
                            userReportFromDB.WatchBtn.onClick.RemoveAllListeners();
                            userReportFromDB.ButtonText.text = "Watch";
                            userReportFromDB.Download.SetActive(false);
                            userReportFromDB.Error.SetActive(false);
                            userReportFromDB.Watch.SetActive(true);
                            itemToSnapTo = userReportFromDB;
                            RecentlyPlayedButton = userReportFromDB;
                            userReportFromDB.WatchBtn.onClick.AddListener(
                                () => { CreateFileAndView((int)userReportFromDB.videoId,null, "", userReportFromDB.UserNameOfSubject); 
                                ReferenceManager.instance.SelectedVideoID = userReportFromDB.videoId;
                                ReferenceManager.instance.azureStorageManager.selectedVideo = userReportFromDB; }
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
                    userReportFromDBs.ForEach(x=>x.CheckIfItContainsTimeBasedReadings());
                    reportGroupHandlerPrefab.ForceRebuildLayout();
                    reportGroupHandlerPrefab.ContentSizeFitter.enabled = false;
                    reportGroupHandlerPrefab.ContentSizeFitter.SetLayoutVertical();
                    reportGroupHandlerPrefab.ContentSizeFitter.enabled = true;
                    if (!hasSpokenAboutReports)
                    {
                        hasSpokenAboutReports = true;
                        ReferenceManager.instance.TTSTutorialHandler.NextLine("You can now see in the top of the screen which section you are currently in. Right now we are in the reports section. In the middle of the screen there is scroll view of groups. Which contain reports and recordings related to the group's category. You can click on any of them to go in the next section");
                    }
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
                isSilentReportFetch = false;
                ReferenceManager.instance.PopupManager.Show(
                    "Fetching Users Failed!",
                    $"Reasons are: {error}"
                );
                
            }
        ,isSilentReportFetch);
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
        CreateCSVButton.onClick.AddListener(() => CreateCSV($"{selectedReadings[0].UserNamefromDB.text}_{DateTime.Now.ToShortDateString().Replace("/","-")}_JointReading.csv", jointReading));
    }
    public List<TimeBasedReadingFromDB> addedTimeBasedReadings = new();
    public List<Toggle> toggles = new();
    public List<ToggleGroup> toggleGroups = new();
    public void ShowTimeBasedReadingsFromDB(List<TimeBasedReadingRequest> timeBasedReadings)
    {
        ReadingViewer.SetActive(true);
        addedTimeBasedReadings.ForEach(x => Destroy(x.gameObject));
        addedTimeBasedReadings.Clear();
        toggleGroups.ForEach(x => Destroy(x.gameObject));
        toggleGroups.Clear();
        toggles.Clear();
        var jointsOnDifferentDates = timeBasedReadings.GroupBy(x => x.ReportsRecordId.ToString());
        createdCsvCount = 0;
        timeBasedReadingFromDBPrefab.CSVButton.onClick.RemoveAllListeners();
        timeBasedReadingFromDBPrefab.CreateExcelButton.onClick.RemoveAllListeners();
       foreach (IGrouping<string,TimeBasedReadingRequest> group in jointsOnDifferentDates)
        {
            TimeBasedReadingFromDB groupHeader = Instantiate(timeBasedReadingFromDBPrefab, timeBasedReadingFromDBPrefab.transform.parent);
                addedTimeBasedReadings.Add(groupHeader);
            groupHeader.timeBasedReadings = group.ToList();
            int fieldIndex = 0;
            bool headingadded = false;
            
            // Create a group header
            foreach (var item in group)
            {
                
                int itemIndex = 0;
                
                PropertyInfo[] fields = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                if (!headingadded)
                {
                    foreach (var field in fields)
                    {
                        string variableName = field.Name; // Get the variable name
                        object fielValue = field.GetValue(item);
                        if (variableName == "ReportsRecordId" || variableName == "CreatedOn" ||variableName == "VideoName")
                        {
                            continue;
                        }
                        if (fielValue != null)
                        {
                            GameObject go = Instantiate(groupHeader.ReadingValuePrefab, groupHeader.ReadingValuePrefab.transform.parent);
                            go.SetActive(true);
                            go.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                            toggles.Add(go.GetComponent<Toggle>());
                            go.GetComponent<Toggle>().onValueChanged.AddListener((value)=>
                            {
                                groupHeader.SelectValue(variableName, go.GetComponent<Toggle>());
                                InvokeOtherToggles(variableName, go.GetComponent<Toggle>().isOn);
                            });
                            if (variableName == "UserName" || variableName == "TimeOfReading" || variableName == "CreatedOn" || variableName == "VideoName" || variableName == "CreatedBy" || variableName == "Subject")
                            {
                                go.SetActive(false);
                                if (variableName == "TimeOfReading")
                                {
                                    groupHeader.timeBasedReadingInfoSection.addedColumns.Insert(0, go);
                                }
                            }
                            else
                            {
                                groupHeader.SelectAllToggle.onValueChanged.AddListener((value) => go.GetComponent<Toggle>().isOn = value);
                            }
                            go.name = variableName;
                            go.transform.GetChild(0).GetComponent<TMP_Text>().text = variableName;
                            groupHeader.Time.text = "Select Readings to Compare from "+item.VideoName;
                            // groupHeader.timeBasedReadingInfoSection.addedColumns.Add(go);
                            Debug.Log($"Instantiated GameObject with name: {variableName}");
                        }
                    }
                    
                    
                    timeBasedReadingFromDBPrefab.CSVButton.onClick.AddListener(() => CreateTimeBasedCSV($"{selectedReadings[0].UserNamefromDB.text}_{DateTime.Now.ToShortDateString().Replace("/","-")}_TimeBasedReadings_{createdCsvCount}.csv", groupHeader.timeBasedReadings,null,groupHeader.timeBasedReadingInfoSection.addedColumns.Select(x=>x.gameObject.name).ToList(),false));
                    timeBasedReadingFromDBPrefab.CreateExcelButton.onClick.AddListener(() => CreateTimeBasedCSV($"{selectedReadings[0].UserNamefromDB.text}_{DateTime.Now.ToShortDateString().Replace("/","-")}_TimeBasedReadings_{createdCsvCount}.csv", groupHeader.timeBasedReadings,null,groupHeader.timeBasedReadingInfoSection.addedColumns.Select(x=>x.gameObject.name).ToList(),true));
                    headingadded = true;
                }
            }
            groupHeader.gameObject.SetActive(true);
           LayoutRebuilder.ForceRebuildLayoutImmediate(groupHeader.Content);
        }
        
        
    }
    
    public void ShowGaitReadingsFromDB(List<GetGaitReportResponse> timeBasedReadings)
    {
        ReadingViewer.SetActive(true);
        addedTimeBasedReadings.ForEach(x => Destroy(x.gameObject));
        addedTimeBasedReadings.Clear();
        toggleGroups.ForEach(x => Destroy(x.gameObject));
        toggleGroups.Clear();
        toggles.Clear();
        var jointsOnDifferentDates = timeBasedReadings.GroupBy(x => x.ReportsRecordId.ToString());
        createdCsvCount = 0;
        timeBasedReadingFromDBPrefab.CSVButton.onClick.RemoveAllListeners();
        timeBasedReadingFromDBPrefab.CreateExcelButton.onClick.RemoveAllListeners();
        timeBasedReadingFromDBPrefab.CSVButton.onClick.AddListener(() =>
        {
            timeBasedReadingFromDBPrefab.RecordingPanel.SetActive(false);
            timeBasedReadingFromDBPrefab.streamingSampleMic.microphoneRecord.StopRecord();
        });
       foreach (IGrouping<string,GetGaitReportResponse> group in jointsOnDifferentDates)
        {
            TimeBasedReadingFromDB groupHeader = Instantiate(timeBasedReadingFromDBPrefab, timeBasedReadingFromDBPrefab.transform.parent);
                addedTimeBasedReadings.Add(groupHeader);
            groupHeader.gaitReportReadings = group.ToList();
            int fieldIndex = 0;
            bool headingadded = false;
            
            // Create a group header
            foreach (var item in group)
            {
                
                int itemIndex = 0;
                
                PropertyInfo[] fields = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                if (!headingadded)
                {
                    foreach (var field in fields)
                    {
                        string variableName = field.Name; // Get the variable name
                        object fielValue = field.GetValue(item);
                        if (variableName == "ReportsRecordId" || variableName == "CreatedOn" ||variableName == "VideoName")
                        {
                            continue;
                        }
                        if (fielValue != null)
                        {
                            GameObject go = Instantiate(groupHeader.ReadingValuePrefab, groupHeader.ReadingValuePrefab.transform.parent);
                            go.SetActive(true);
                            go.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                            toggles.Add(go.GetComponent<Toggle>());
                            go.GetComponent<Toggle>().onValueChanged.AddListener((value)=>
                            {
                                groupHeader.SelectValue(variableName, go.GetComponent<Toggle>());
                                InvokeOtherToggles(variableName, go.GetComponent<Toggle>().isOn);
                            });
                            if (variableName == "UserName" || variableName == "TimeOfReading" || variableName == "CreatedOn" || variableName == "VideoName" || variableName == "SubjectStandingAtTime" || variableName == "SelectedLeg" || variableName == "FootStrikeAtTime" || variableName == "HeelPassingAtTime"||variableName == "UserName" || variableName == "TimeOfReading" || variableName == "CreatedOn" || variableName == "VideoName" || variableName == "CreatedBy" || variableName == "Subject")
                            {
                                go.SetActive(false);
                                if (variableName == "TimeOfReading" || variableName == "FootStrikeAtTime" || variableName == "HeelPassingAtTime")
                                {
                                    groupHeader.timeBasedReadingInfoSection.addedColumns.Insert(0, go);
                                    GaitColumnOfTimeName.Add(variableName);
                                }
                            }
                            else
                            {
                                groupHeader.SelectAllToggle.onValueChanged.AddListener((value) => go.GetComponent<Toggle>().isOn = value);
                            }
                            go.name = variableName;
                            go.transform.GetChild(0).GetComponent<TMP_Text>().text = variableName;
                            groupHeader.Time.text = "Select Readings to Compare from "+item.VideoName;
                            // groupHeader.timeBasedReadingInfoSection.addedColumns.Add(go);
                            Debug.Log($"Instantiated GameObject with name: {variableName}");
                        }
                    }

                    if (selectedReadings.Count != 0)
                    {
                        lineGraph = true;
                        timeBasedReadingFromDBPrefab.CSVButton.onClick.AddListener(() =>
                        {
                            CreateTimeBasedCSV(
                                $"{selectedReadings[0].UserNamefromDB.text}_{DateTime.Now.ToShortDateString().Replace("/", "-")}_TimeBasedReadings_{createdCsvCount}.csv",
                                groupHeader.timeBasedReadings, null,
                                groupHeader.timeBasedReadingInfoSection.addedColumns.Select(x => x.gameObject.name)
                                    .ToList(),false);
                        });
                        
                        timeBasedReadingFromDBPrefab.CreateExcelButton.onClick.AddListener(() =>
                        {
                            CreateTimeBasedCSV(
                                $"{selectedReadings[0].UserNamefromDB.text}_{DateTime.Now.ToShortDateString().Replace("/", "-")}_TimeBasedReadings_{createdCsvCount}.csv",
                                groupHeader.timeBasedReadings, null,
                                groupHeader.timeBasedReadingInfoSection.addedColumns.Select(x => x.gameObject.name)
                                    .ToList(),true);
                        });
                    }

                    if (selectedGaitReadings.Count != 0)
                    {
                        lineGraph = false;
                        timeBasedReadingFromDBPrefab.CSVButton.onClick.AddListener(() =>
                        {
                            CreateTimeBasedCSV(
                                $"{selectedGaitReadings[0].UserNamefromDB.text}_{DateTime.Now.ToShortDateString().Replace("/", "-")}_GaitReadings_{createdCsvCount}.csv",
                                null, groupHeader.gaitReportReadings,
                                groupHeader.timeBasedReadingInfoSection.addedColumns.Select(x => x.gameObject.name)
                                    .ToList(), false);
                        });
                        timeBasedReadingFromDBPrefab.CreateExcelButton.onClick.AddListener(() =>
                        {
                            CreateTimeBasedCSV(
                                $"{selectedGaitReadings[0].UserNamefromDB.text}_{DateTime.Now.ToShortDateString().Replace("/", "-")}_GaitReadings_{createdCsvCount}.csv",
                                null, groupHeader.gaitReportReadings,
                                groupHeader.timeBasedReadingInfoSection.addedColumns.Select(x => x.gameObject.name)
                                    .ToList(), true);
                        });
                    }

                    headingadded = true;
                }
            }
            groupHeader.gameObject.SetActive(true);
           LayoutRebuilder.ForceRebuildLayoutImmediate(groupHeader.Content);
        }
        
        
    }

    private bool lineGraph;
    void InvokeOtherToggles(string name, bool value)
    {
        toggles.Where(x=>x.isOn !=value && x.gameObject.name == name).ToList().ForEach(x=>x.isOn = value);
    }
    private int createdCsvCount = 0;
    private List<string> csvPaths = new();
    void CreateCSV(string fileName, List<JointReading> readings)
    {
        // Path to save the file
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        // Use StringBuilder for efficient CSV generation
        StringBuilder csvContent = new StringBuilder();

        // Add header row
        csvContent.AppendLine("Name_Of_Video,Name_Of_Reading,Min_Value,Max_Value,Range,Readings_Taken_Date");

        // Add data rows
        foreach (var reading in readings)
        {
            csvContent.AppendLine($"{reading.VideoNameLink},{reading.NameOfReading},{reading.MinimumValue},{reading.MaximumValue},{reading.RangeValue},{ConvertToLocalTime(reading.CreatedOn)}");
        }

        // Write the CSV content to the file
        File.WriteAllText(filePath, csvContent.ToString());

        // Log the file path
        
        CSVManager.Export(filePath);
        // RunRScript(filePath,fileName);
    }

    public List<string> GaitColumnOfTimeName = new();
    public List<string> CsvDatas = new ();
    async void CreateTimeBasedCSV(string fileName, List<TimeBasedReadingRequest> readings, List<GetGaitReportResponse> gaitReadings,List<string> columnNames, bool showCSV)
    {
        Debug.Log("Itnni bar");
        // Path to save the file
        
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        // Use StringBuilder for efficient CSV generation
        StringBuilder csvContent = new StringBuilder();
        var tempNames = columnNames.ToList();
        foreach (var tempColumnName in columnNames)
        {
            if (tempColumnName.Equals("AngleDifferenceAtTime"))
            {
                var matchingOne = tempNames.FirstOrDefault(x=>x.Equals(tempColumnName));
                tempNames[tempNames.IndexOf(matchingOne)] = "HipKneeABDAngleDifference";
            }
        }
        csvContent.AppendLine($"{string.Join(",",tempNames)}");

        // Add data rows
        if (readings != null)
        {
            
           readings = readings.OrderBy(x => x.TimeOfReading).ToList();
          
            foreach (var reading in readings)
            {
                PropertyInfo[] fields = reading.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.GetValue(reading) != null && x.Name != "ReportsRecordId" &&
                                columnNames.Contains(x.Name)).ToArray();
                
                string lineToAddTimeBased = string.Join(",", fields.Select(x => x.GetValue(reading).ToString()));
                if (showCSV)
                {
                    var items = lineToAddTimeBased.Split(',').ToList();
                    items[0] = TimeSpan.FromSeconds(float.Parse(items[0])).ToString(@"mm\:ss\:fff");
                    lineToAddTimeBased = string.Join(",", items);
                }

                csvContent.AppendLine($"{lineToAddTimeBased}");
            }
        }

        if (gaitReadings != null)
        {
            gaitReadings=gaitReadings.OrderBy(x => x.HeelPassingAtTime).ThenBy(x=>x.FootStrikeAtTime).ToList();
            var footStrikeRows = gaitReadings.Where(x => x.FootStrikeAtTime != 0).ToList();
            
            foreach (var reading in gaitReadings)
            {
                if (reading.HeelPassingAtTime == 0 && reading.FootStrikeAtTime == 0)
                {
                    continue;
                }
                if (reading.HeelPassingAtTime == 0)
                {
                    continue;
                }
                PropertyInfo[] fields = reading.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.GetValue(reading) != null && x.Name != "ReportsRecordId" &&
                                columnNames.Contains(x.Name)).ToArray();


                string lineToAddHeelPass = string.Join(",", fields.Select(x => x.GetValue(reading).ToString()));
                var items = lineToAddHeelPass.Split(',').ToList();
                if (showCSV)
                {
                    items[0] = TimeSpan.FromSeconds(float.Parse(items[0])).ToString(@"mm\:ss\:fff");
                    items[1] = TimeSpan.FromSeconds(float.Parse(items[1])).ToString(@"mm\:ss\:fff");
                    lineToAddHeelPass = string.Join(",", items);
                }

                csvContent.AppendLine($"{lineToAddHeelPass}");
                var nextFootStrikeRow = footStrikeRows.FirstOrDefault(x => x.FootStrikeAtTime > reading.HeelPassingAtTime);
                if (nextFootStrikeRow == null)
                {
                    continue;
                }
                PropertyInfo[] footStrikefields = nextFootStrikeRow.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.GetValue(reading) != null && x.Name != "ReportsRecordId" &&
                                columnNames.Contains(x.Name)).ToArray();

                string lineToAddFootPass = string.Join(",",footStrikefields.Select(x => x.GetValue(nextFootStrikeRow).ToString()));
                if (showCSV)
                {
                    var itemsFootPass = lineToAddFootPass.Split(',').ToList();
                    itemsFootPass[0] = TimeSpan.FromSeconds(float.Parse(itemsFootPass[0])).ToString(@"mm\:ss\:fff");
                    itemsFootPass[1] = TimeSpan.FromSeconds(float.Parse(itemsFootPass[1])).ToString(@"mm\:ss\:fff");
                    lineToAddFootPass = string.Join(",", itemsFootPass);
                }

                csvContent.AppendLine($"{lineToAddFootPass}");
            }
        }

        // Write the CSV content to the file
        File.WriteAllText(filePath, csvContent.ToString());

        // Log the file path
        
        // CSVManager.Export(filePath);
        csvPaths.Add((filePath));
        CsvDatas.Add(csvContent.ToString());
        createdCsvCount += 1;
        if (createdCsvCount == addedTimeBasedReadings.Count)
        {
            if (!showCSV)
            {
               # region If Multiple Choice AI and R Generation
                // IOSNativeAlert.ShowAlertMessage("Select One","Would you like AI report Generation or R Report Generation?",new IOSNativeAlert.AlertButton("Generate R Report",callback:
                //     () =>
                //     {
                //         RunRScript(csvPaths, true);
                //     }),new IOSNativeAlert.AlertButton("Generate AI Report",callback: () =>
                // {
                //     bool isGait = selectedReadings.Count == 0 ? true : false;
                //     chatGPTHandler.AnalyzeCSV(CsvDatas,isGait);
                // }));
// #if UNITY_EDITOR
//                 RunRScript(csvPaths, true);
// #endif
                #endregion

                bool? isChatGPT = new bool();
                isChatGPT = null;
                ReferenceManager.instance.PopupManager.Show("Select Report Type!","Please Select One","R Script",okPressed:
                    () =>
                    {
                        isChatGPT = true;
                    },"Chat GPT",noPressed: () =>
                    {
                        isChatGPT = false;
                    });
                while (isChatGPT == null)
                {
                    await Task.Delay(500);
                }
                if (isChatGPT == true)
                {
                    bool isGait = selectedReadings.Count == 0 ? true : false;
                    chatGPTHandler.AnalyzeCSV(CsvDatas,isGait);
                }
                else
                {
                    RunRScript(csvPaths, true);
                }
                
            }
            else
            {
                if (csvPaths.Count > 1)
                {
                    GeneralStaticManager.CreateZipFile(Path.Combine(Application.persistentDataPath, fileName.Replace(".csv",".zip")),csvPaths);
                }
                else
                {
                    CSVManager.Export(csvPaths[0]);
                }
            }
            createdCsvCount = 0;
            csvPaths.Clear();
        }
        
    }
    public void RunRScript(List<string> csvPath, bool isTimeBased = false)
    {
        // Paths
        if (!isTimeBased)
        {
            string outputPath = Path.Combine(Application.persistentDataPath, "rfile.Rmd");
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
            // GenerateRMarkdown(outputPath, csvPath, fileName);
        }
        else
        {
            string outputPath = Path.Combine(Application.persistentDataPath, "timeBasedRfile.Rmd");
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
            GenerateRMarkdownForTimeBased(outputPath, csvPath);
        }
    }
public void GenerateRMarkdownForTimeBased(string outputRmdPath, List<string> csvFilePaths)
{
    try
    {
        if (selectedReadings.Count != 0)
        {
            // Ensure the output directory exists
            string outputDirectory = Path.GetDirectoryName(outputRmdPath);
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            // Build R Markdown content
            var rmdContent = new System.Text.StringBuilder();
            rmdContent.AppendLine("---");
            rmdContent.AppendLine("title: \"Comparison Report\"");
            rmdContent.AppendLine("output: html_document");
            rmdContent.AppendLine("---\n");

            rmdContent.AppendLine("## Comparison of Multiple Datasets");
            rmdContent.AppendLine("This report compares data from multiple datasets.\n");

            int datasetIndex = 1;

            // Include datasets as inline R data.frames
            foreach (var csvPath in csvFilePaths)
            {
                string[] csvLines = File.ReadAllLines(csvPath);
                if (csvLines.Length < 2)
                {
                    Debug.LogError($"CSV file '{csvPath}' is empty or missing data.");
                    continue;
                }

                string[] headers = csvLines[0].Split(',');
                var rDataFrame = new System.Text.StringBuilder();
                rDataFrame.AppendLine($"data{datasetIndex} <- data.frame(");

                for (int i = 0; i < headers.Length; i++)
                {
                    string header = headers[i].Trim(); // Trim whitespace around headers
                    rDataFrame.Append($"  {header} = c(");

                    for (int j = 1; j < csvLines.Length; j++)
                    {
                        string[] rowValues = csvLines[j].Split(',');
                        string value =
                            i < rowValues.Length ? rowValues[i].Trim() : "NA"; // Handle missing values with "NA"
                        rDataFrame.Append(value);

                        if (j < csvLines.Length - 1)
                            rDataFrame.Append(", ");
                    }

                    if (i < headers.Length - 1)
                        rDataFrame.AppendLine("),");
                    else
                        rDataFrame.AppendLine(")");
                }

                rDataFrame.AppendLine(")");

                // Add dataset to R Markdown content
                var dateTime = EscapeMarkdown(selectedReadings[datasetIndex - 1].CreatedOn.text);
                rmdContent.AppendLine($"### Capture '{dateTime}', {selectedReadings[datasetIndex - 1].UserNameOfSubject}, {selectedReadings[datasetIndex - 1].ReportDescription.text.Replace("<b>Comment:</b>\n", "")}");
                rmdContent.AppendLine($"#The raw data for Dataset {datasetIndex} is shown below:\n");
                rmdContent.AppendLine("```{r echo=FALSE, results='asis'}");
                rmdContent.AppendLine(rDataFrame.ToString());
                rmdContent.AppendLine("library(knitr)");
                rmdContent.AppendLine(
                    "cat('<div style=\"max-height: 300px; overflow-y: auto; border: 1px solid #ccc; padding: 10px;\">')");
                rmdContent.AppendLine(
                    $"print(kable(data{datasetIndex}, caption = \"Dataset {datasetIndex} Table\", format = \"html\", table.attr = \"class='table table-striped'\"))");
                rmdContent.AppendLine("cat('</div>')");
                rmdContent.AppendLine("```");

                datasetIndex++;
            }

            // Add comparison graphs
            rmdContent.AppendLine("## Joint Comparison Across Datasets");
            rmdContent.AppendLine("The following graphs compare joint readings across the datasets.\n");

            rmdContent.AppendLine("```{r echo=FALSE, warning=FALSE, message=FALSE}");
            rmdContent.AppendLine("library(ggplot2)");
            rmdContent.AppendLine("library(svglite)");

            rmdContent.AppendLine("# Label datasets");
            for (int i = 1; i < datasetIndex; i++)
            {
                rmdContent.AppendLine($"data{i}$Dataset <- 'Dataset {i}'");
            }

            rmdContent.AppendLine("# Combine datasets");
            rmdContent.AppendLine("combined_data <- rbind(" +
                                  string.Join(", ", Enumerable.Range(1, datasetIndex - 1).Select(i => $"data{i}")) +
                                  ")");
            rmdContent.AppendLine("plot_vars <- names(data1)[2:length(names(data1))] # Columns to plot");

            rmdContent.AppendLine("# Generate and embed inline SVG plots");
            rmdContent.AppendLine("for (var_name in plot_vars) {");
            rmdContent.AppendLine(
                "  if (var_name == 'Dataset') next # Skip the graph titled 'Dataset Comparison Across Datasets'");

            rmdContent.AppendLine("  cat(paste0('### Comparison of ', var_name, ' Across Datasets\n\n'))");

            rmdContent.AppendLine("  # Create the plot and save as SVG content");
            rmdContent.AppendLine("  svg_file <- tempfile()");
            rmdContent.AppendLine("  svglite::svglite(svg_file, width = 8, height = 6)");
            
            rmdContent.AppendLine("  print(ggplot(combined_data, aes(x = TimeOfReading, y = .data[[var_name]], color = Dataset, group = Dataset)) +");
            
            rmdContent.AppendLine("          geom_line(linewidth = 1) +");
            rmdContent.AppendLine("          ggtitle(paste(var_name, 'Comparison Across Datasets')) +");
            rmdContent.AppendLine("          xlab('Time of Reading') + ylab(var_name) +");
            rmdContent.AppendLine("          theme_minimal() +");
            rmdContent.AppendLine("          theme(axis.text.x = element_text(angle = 45, hjust = 1)))");
            rmdContent.AppendLine("  dev.off()");

            rmdContent.AppendLine("  # Embed SVG content directly in the HTML");
            rmdContent.AppendLine("  svg_content <- paste(readLines(svg_file), collapse = '\n')");
            rmdContent.AppendLine("  cat(svg_content)"); // Embed raw SVG content directly
            rmdContent.AppendLine("}");
            rmdContent.AppendLine("```");
            rmdContent.AppendLine("\n <b>Note:</b> "+timeBasedReadingFromDBPrefab.RecordedText.text);
            // Write the RMD file
            File.WriteAllText(outputRmdPath, rmdContent.ToString());
            RReportGenerateRequest rr = new RReportGenerateRequest()
            {
                Rmd = "@" + rmdContent.ToString(),
            };
            string json = JsonConvert.SerializeObject(rr);
            APIHandler.instance.Post("UserReport/GetRReport", json, onSuccess: (response) =>
            {
                Debug.Log(response);
                RReportResponse rrr = JsonConvert.DeserializeObject<RReportResponse>(response);
                rrr.result.HtmlResponse = rrr.result.HtmlResponse.Replace("&lt;", "<").Replace("&gt;", ">")
                    .Replace("&#39;", "\"").Replace("##", "");
                string path = Path.Combine(Application.persistentDataPath, "report.html");
                File.WriteAllText(path, rrr.result.HtmlResponse);
                // CSVManager.Export(path);
#if UNITY_EDITOR
                System.Diagnostics.Process.Start(path);
                Debug.Log("Is Editor");

#else

            string url = "file://" + path.Replace(" ", "%20");
            Debug.Log("URL = " + url);
            Debug.Log("Persistance = " + path);
            GeneralStaticManager.OpenFile(path);
#endif
                
                UploadHtmlPrompt.SetActive(true);
                UploadHtmlButton.onClick.RemoveAllListeners();
                UploadHtmlButton.onClick.AddListener(()=>
                {
                    string fileName = string.IsNullOrEmpty(HtmlFileName.text)? "report":HtmlFileName.text;
                    ReferenceManager.instance.LoadingManager.Show("Uploading HTML to database");
                    StartCoroutine(AzureConnector.Instance.PutHTMLOnBlob(rrr.result.HtmlResponse, "htmlreports",
                        fileName.ToLower(), UploadHtmlCallback, true));
                });
            }, onError: (error) => { Debug.LogError(error); });
            Debug.Log($"R Markdown file generated successfully at: {outputRmdPath}");
        }
        else
        {
          // Ensure the output directory exists
    string outputDirectory = Path.GetDirectoryName(outputRmdPath);
    if (!Directory.Exists(outputDirectory))
        Directory.CreateDirectory(outputDirectory);

    // Build R Markdown content
    var rmdContent = new System.Text.StringBuilder();
    rmdContent.AppendLine("---");
    rmdContent.AppendLine("title: \"Comparison Report\"");
    rmdContent.AppendLine("output: html_document");
    rmdContent.AppendLine("---\n");

    rmdContent.AppendLine("## Comparison of Multiple Datasets");
    rmdContent.AppendLine("This report compares data from multiple datasets.\n");

    // Add setup chunk
    rmdContent.AppendLine("```{r setup, include=FALSE}");
    rmdContent.AppendLine("# Install and load required packages");
    rmdContent.AppendLine("required_packages <- c(\"ggplot2\", \"svglite\", \"knitr\", \"tidyr\")");
    rmdContent.AppendLine("new_packages <- required_packages[!(required_packages %in% installed.packages()[,\"Package\"])]");
    rmdContent.AppendLine("if(length(new_packages)) install.packages(new_packages, repos = \"https://cran.rstudio.com\")");
    rmdContent.AppendLine("library(ggplot2)");
    rmdContent.AppendLine("library(svglite)");
    rmdContent.AppendLine("library(knitr)");
    rmdContent.AppendLine("library(tidyr)");
    rmdContent.AppendLine("```");

    int datasetIndex = 1;

    // Include datasets as inline R data.frames
    foreach (var csvPath in csvFilePaths)
    {
        string[] csvLines = File.ReadAllLines(csvPath);
        if (csvLines.Length < 2)
        {
            Debug.LogError($"CSV file '{csvPath}' is empty or missing data.");
            continue;
        }

        string[] headers = csvLines[0].Split(',');
        var rDataFrame = new System.Text.StringBuilder();
        rDataFrame.AppendLine($"data{datasetIndex} <- data.frame(");

        for (int i = 0; i < headers.Length; i++)
        {
            string header = headers[i].Trim(); // Trim whitespace around headers
            rDataFrame.Append($"  {header} = c(");

            for (int j = 1; j < csvLines.Length; j++)
            {
                string[] rowValues = csvLines[j].Split(',');
                string value = i < rowValues.Length ? rowValues[i].Trim() : "NA"; // Handle missing values with "NA"
                rDataFrame.Append(value);

                if (j < csvLines.Length - 1)
                    rDataFrame.Append(", ");
            }

            if (i < headers.Length - 1)
                rDataFrame.AppendLine("),");
            else
                rDataFrame.AppendLine(")");
        }

        rDataFrame.AppendLine(")");

        // Add dataset to R Markdown content
        rmdContent.AppendLine($"### Dataset {datasetIndex}");
        rmdContent.AppendLine($"The raw data for Dataset {datasetIndex} is shown below:\n");
        rmdContent.AppendLine("```{r echo=FALSE, results='asis'}");
        rmdContent.AppendLine(rDataFrame.ToString());
        rmdContent.AppendLine($"data{datasetIndex}$Dataset <- 'Dataset {datasetIndex}'"); // Add Dataset column
        rmdContent.AppendLine($"kable(data{datasetIndex}, caption = \"Dataset {datasetIndex} Table\", format = \"html\", table.attr = \"class='table table-striped'\")");
        rmdContent.AppendLine("```");

        datasetIndex++;
    }

    // Combine datasets
    rmdContent.AppendLine("```{r echo=FALSE, warning=FALSE, message=FALSE}");
    rmdContent.AppendLine("# Combine datasets");
    rmdContent.AppendLine("combined_data <- rbind(" +
                          string.Join(", ", Enumerable.Range(1, datasetIndex - 1).Select(i => $"data{i}")) +
                          ")");
    rmdContent.AppendLine("# Identify numeric columns for plotting");
    rmdContent.AppendLine("numeric_columns <- names(combined_data)[sapply(combined_data, is.numeric)]");
    rmdContent.AppendLine("time_columns <- c(\"FootStrikeAtTime\", \"HeelPassingAtTime\")");
    rmdContent.AppendLine("value_columns <- setdiff(numeric_columns, time_columns)");
    rmdContent.AppendLine("```");

    // Generate two graphs for each dataset and combined datasets
    foreach (var timeColumn in new[] { "FootStrikeAtTime", "HeelPassingAtTime" })
    {
        // Graphs for each dataset
        for (int i = 1; i < datasetIndex; i++)
        {
            rmdContent.AppendLine($"## Dataset {i} - Comparison for {timeColumn}");
            rmdContent.AppendLine($"The following graph compares all variables with respect to `{timeColumn}` for Dataset {i}.\n");

            rmdContent.AppendLine("```{r echo=FALSE, warning=FALSE, message=FALSE}");
            rmdContent.AppendLine("svg_file <- tempfile(fileext = '.svg')"); // Save graph as SVG
            rmdContent.AppendLine("svglite::svglite(svg_file, width = 8, height = 6)");

            // Generate the graph
            rmdContent.AppendLine(
                $"p <- ggplot(data{i} %>% pivot_longer(value_columns, names_to = 'Variable', values_to = 'Value'), aes(x = !!sym('{timeColumn}'), y = Value, fill = Variable)) +");
            rmdContent.AppendLine("  geom_bar(stat = 'identity', position = 'dodge') +");
            rmdContent.AppendLine($"  labs(title = 'Dataset {i} - Comparison of All Variables ({timeColumn})',");
            rmdContent.AppendLine($"       x = '{timeColumn}',");
            rmdContent.AppendLine("       y = 'Value') +");
            rmdContent.AppendLine("  theme_minimal()");
            rmdContent.AppendLine("print(p)");
            rmdContent.AppendLine("dev.off()");
            rmdContent.AppendLine("svg_content <- paste(readLines(svg_file), collapse = '\\n')");
            rmdContent.AppendLine("cat(svg_content)");
            rmdContent.AppendLine("```");
        }
    }

    if (!string.IsNullOrEmpty(timeBasedReadingFromDBPrefab.RecordedText.text))
    {
        rmdContent.AppendLine("\n <b>Note:</b> " + timeBasedReadingFromDBPrefab.RecordedText.text);
    }
    // Write the RMD file

    File.WriteAllText(outputRmdPath, rmdContent.ToString());
                RReportGenerateRequest rr = new RReportGenerateRequest()
                {
                    Rmd = "@" + rmdContent.ToString(),
                };
                string json = JsonConvert.SerializeObject(rr);
                APIHandler.instance.Post("UserReport/GetRReport", json, onSuccess: (response) =>
                {
                    Debug.Log(response);
                    RReportResponse rrr = JsonConvert.DeserializeObject<RReportResponse>(response);
                    rrr.result.HtmlResponse = rrr.result.HtmlResponse.Replace("&lt;", "<").Replace("&gt;", ">")
                        .Replace("&#39;", "\"").Replace("##", "");
                    string path = Path.Combine(Application.persistentDataPath, "report.html");
                    File.WriteAllText(path, rrr.result.HtmlResponse);
                    // CSVManager.Export(path);
#if UNITY_EDITOR
                    System.Diagnostics.Process.Start(path);
                    Debug.Log("Is Editor");

#else

            string url = "file://" + path.Replace(" ", "%20");
            Debug.Log("URL = " + url);
            Debug.Log("Persistance = " + path);
            GeneralStaticManager.OpenFile(path);
#endif
                    
                    UploadHtmlPrompt.SetActive(true);
                    UploadHtmlButton.onClick.RemoveAllListeners();
                    UploadHtmlButton.onClick.AddListener(()=>
                    {
                        string fileName = string.IsNullOrEmpty(HtmlFileName.text)? "report":HtmlFileName.text;
                        ReferenceManager.instance.LoadingManager.Show("Uploading HTML to database");
                        StartCoroutine(AzureConnector.Instance.PutHTMLOnBlob(rrr.result.HtmlResponse, "htmlreports",
                            fileName.ToLower(), UploadHtmlCallback, true));
                    });
                }, onError: (error) => { Debug.LogError(error); });
                Debug.Log($"R Markdown file generated successfully at: {outputRmdPath}");
            }
        
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Error generating R Markdown file: {ex.Message}");
    }
}

public void UploadHtmlCallback(bool success, string error, string uri)
{
    UploadHtmlPrompt.SetActive(false);
    if (success)
    {
        if (selectedReadings.Count != 0)
        {
            foreach (var userReportFromDB in selectedReadings)
            {
                CreateHtmlReportBody createHtmlReportBody = new CreateHtmlReportBody()
                {
                    CreatedBy = GeneralStaticManager.GlobalVar["UserName"],
                    HtmlReport = uri,
                    ReportsRecordId = userReportFromDB.videoId
                };
                string json = JsonConvert.SerializeObject(createHtmlReportBody);
                APIHandler.instance.Post("UserReport/UploadHtmlReport",json, onSuccess: (response) =>
                {
                    Debug.Log("Success Uploading HTML report");
                }, onError: (error) =>
                {
                    ReferenceManager.instance.PopupManager.Show(
                        "Uploading Report Failed",
                        $"Reasons are: {error}"
                    );
                });
            }
        }

        if (selectedGaitReadings.Count != 0)
        {
            foreach (var userReportFromDB in selectedGaitReadings)
            {
                CreateHtmlReportBody createHtmlReportBody = new CreateHtmlReportBody()
                {
                    CreatedBy = GeneralStaticManager.GlobalVar["UserName"],
                    HtmlReport = uri,
                    ReportsRecordId = userReportFromDB.videoId
                };
                string json = JsonConvert.SerializeObject(createHtmlReportBody);
                APIHandler.instance.Post("UserReport/UploadHtmlReport",json, onSuccess: (response) =>
                {
                    Debug.Log("Success Uploading HTML report");
                }, onError: (error) =>
                {
                    ReferenceManager.instance.PopupManager.Show(
                        "Uploading Report Failed",
                        $"Reasons are: {error}"
                    );
                });
            }
        }
        ReferenceManager.instance.PopupManager.Show("Uploading Successful","Your report is saved properly in our database");
    }
    else
    {
        ReferenceManager.instance.PopupManager.Show("There was an error",$"Error uploading html report due to:\n{error}");
    }
    ReferenceManager.instance.LoadingManager.Hide();
}
string EscapeMarkdown(string input)
{
    return input.Replace("\\", "\\\\")
        .Replace("`", "\\`")
        .Replace("_", "\\_")
        .Replace("*", "\\*")
        .Replace("#", "\\#")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;");
}



// RReportGenerateRequest rr = new RReportGenerateRequest()
    // {
    //     Rmd = "@"+rmdContent.ToString(),
    // };
    // string json = JsonConvert.SerializeObject(rr);
    // APIHandler.instance.Post("UserReport/GetRReport",json,onSuccess: (response) =>
    // {
    //     Debug.Log(response);
    //     RReportResponse rrr = JsonConvert.DeserializeObject<RReportResponse>(response);
    //     rrr.result.HtmlResponse = rrr.result.HtmlResponse.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&#39;","\"").Replace("##","");
    //     string path = Path.Combine(Application.persistentDataPath, "report.html");
    //     File.WriteAllText(path, rrr.result.HtmlResponse);
    //     CSVManager.Export(path);
    //         
    // },onError: (error) =>
    // {
    //     Debug.LogError(error);
    // });
    public TMP_InputField CustomRContent;

    public void PasteContent()
    {
        CustomRContent.text = UniClipboard.GetText();
    }
    public void GenerateCustomRReport()
    {
        RReportGenerateRequest rr = new RReportGenerateRequest()
        {
            Rmd = "@" + CustomRContent.text.ToString(),
        };
        string json = JsonConvert.SerializeObject(rr);
        APIHandler.instance.Post("UserReport/GetRReport", json, onSuccess: (response) =>
        {
            Debug.Log(response);
            RReportResponse rrr = JsonConvert.DeserializeObject<RReportResponse>(response);
            rrr.result.HtmlResponse = rrr.result.HtmlResponse.Replace("&lt;", "<").Replace("&gt;", ">")
                .Replace("&#39;", "\"").Replace("##", "");
            string path = Path.Combine(Application.persistentDataPath, "CustomReport.html");
            File.WriteAllText(path, rrr.result.HtmlResponse);
            // CSVManager.Export(path);
#if UNITY_EDITOR
            System.Diagnostics.Process.Start(path);
            Debug.Log("Is Editor");

#else

            string url = "file://" + path.Replace(" ", "%20");
            Debug.Log("URL = " + url);
            Debug.Log("Persistance = " + path);
            GeneralStaticManager.OpenFile(path);
#endif
        }, onError: (error) => { Debug.LogError(error); });
    }
    public void GenerateRMarkdown(string OutputRmdPath,string CsvFilePath,string fileName)
    {
        try
        {
            // Ensure output directory exists
            string outputDirectory = Path.GetDirectoryName(OutputRmdPath);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Read CSV data
            string[] csvLines = File.ReadAllLines(CsvFilePath);
            if (csvLines.Length < 2)
            {
                Debug.LogError("CSV file is empty or missing data.");
                return;
            }

            // Extract headers and identify columns
            string[] headers = csvLines[0].Split(',');
            int videoIndex = System.Array.IndexOf(headers, "Name_Of_Video");
            int readingIndex = System.Array.IndexOf(headers, "Name_Of_Reading");
            int minValueIndex = System.Array.IndexOf(headers, "Min_Value");
            int maxValueIndex = System.Array.IndexOf(headers, "Max_Value");
            int rangeValueIndex = System.Array.IndexOf(headers, "Range");

            if (videoIndex == -1 || readingIndex == -1 || minValueIndex == -1 || maxValueIndex == -1 || rangeValueIndex == -1)
            {
                Debug.LogError("One or more required columns not found in the CSV headers.");
                return;
            }

            // Define the R Markdown content
            var rmdContent = new System.Text.StringBuilder();
            rmdContent.AppendLine("---");
            rmdContent.AppendLine("title: \"Generated Report\"");
            rmdContent.AppendLine("output: html_document");
            rmdContent.AppendLine("---");
            rmdContent.AppendLine();

            // Add the Raw Data Section
            rmdContent.AppendLine("## Raw Data Table");
            rmdContent.AppendLine();
            rmdContent.AppendLine("Below is the raw data from the CSV file:");
            rmdContent.AppendLine();
            rmdContent.AppendLine("```{r echo=FALSE}");
            rmdContent.AppendLine($"data <- read.csv('{fileName}')");
            rmdContent.AppendLine("knitr::kable(data, caption = 'Raw Data Table')");
            rmdContent.AppendLine("```");
            rmdContent.AppendLine();

            // Add Video-Based Section
            rmdContent.AppendLine("## Video-Based Report");
            rmdContent.AppendLine("The following sections group data by `Name_Of_Video`.");
            rmdContent.AppendLine();
            rmdContent.AppendLine("```{r}");
            rmdContent.AppendLine("library(ggplot2)");
            rmdContent.AppendLine("```");

            // Process rows grouped by Name_Of_Video
            var videoGroups = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();

            foreach (string line in csvLines[1..])
            {
                string[] values = line.Split(',');
                if (values.Length < headers.Length) continue;

                string videoName = values[videoIndex].Trim();
                if (!videoGroups.ContainsKey(videoName))
                {
                    videoGroups[videoName] = new System.Collections.Generic.List<string>();
                }
                videoGroups[videoName].Add(line);
            }

            foreach (var videoGroup in videoGroups)
            {
                string videoName = videoGroup.Key;
                rmdContent.AppendLine($"## {videoName}");
                rmdContent.AppendLine();

                // Generate dot plots for Mini Value, Max Value, and Range
                foreach (var column in new[] { ("Min_Value", "Min Value"), ("Max_Value", "Max Value"), ("Range", "Range Value") })
                {
                    rmdContent.AppendLine($"### {column.Item2} by Name_Of_Reading");
                    rmdContent.AppendLine();
                    rmdContent.AppendLine("```{r echo=FALSE}");
                    rmdContent.AppendLine($"ggplot(subset(data, Name_Of_Video == '{videoName}'), aes(x = Name_Of_Reading, y = {column.Item1}, group = 1)) +");
                    rmdContent.AppendLine("  geom_point(color = 'blue', size = 3) +");   // Dots on the graph
                    rmdContent.AppendLine("  geom_line(color = 'blue', linewidth = 1) +"); // Line connecting dots
                    rmdContent.AppendLine($"  ggtitle('{column.Item2} by Name_Of_Reading ({videoName})') +");
                    rmdContent.AppendLine("  theme_minimal() +");
                    rmdContent.AppendLine("  theme(axis.text.x = element_text(angle = 45, hjust = 1))");
                    rmdContent.AppendLine("```");
                    rmdContent.AppendLine();
                }
            }

            // Write the R Markdown file
            File.WriteAllText(OutputRmdPath, rmdContent.ToString());
            Debug.Log($"R Markdown file generated successfully at: {OutputRmdPath}");

            // Copy CSV file as Excel-compatible format
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error generating R Markdown file: {ex.Message}");
        }
      
    }
    public void CompareSelectedReadings()
    {
        if(selectedReadings.Count == 0 && selectedGaitReadings.Count == 0)
        {
            ReferenceManager.instance.PopupManager.Show("No Reading Selected", "Please select a reading or multiple readings from the reports section to compare them");
            return;
        }

        if (selectedReadings.Count > 0 && selectedGaitReadings.Count > 0)
        {
            ReferenceManager.instance.PopupManager.Show("Report Category Not Matching", "Please Select Gait Or Per Frame Readings");
            return;
        }
        // List<JointReading> listofJointReadings = selectedReadings.Where(x=>x.jointReadings!=null).SelectMany(x=>x.jointReadings).ToList();
        if (selectedGaitReadings.Count > 0)
        {
            List<GetGaitReportResponse> listOfGaitReadings = selectedGaitReadings.Where(x => x.gaitReports != null).SelectMany(x => x.gaitReports).ToList();
            ShowGaitReadingsFromDB(listOfGaitReadings);
        }

        // ShowJointReadingsFromDB(listofJointReadings);
        if (selectedReadings.Count > 0)
        {
            List<TimeBasedReadingRequest> timeBasedReadings =
                selectedReadings.SelectMany(x => x.timeBasedReadings)?.ToList();
            ShowTimeBasedReadingsFromDB(timeBasedReadings);
        }


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
        ReferenceManager.instance.userReportController.isSilentReportFetch = true;
        Start();
    }

    private bool spokenCreateNew;
    public void ReallyCreateNew()
    {
        if (!spokenCreateNew)
        {
            spokenCreateNew = true;
            ReferenceManager.instance.TTSTutorialHandler.NextLine("There is a dropdown on the top to select the measurements you want to do. Subject should stand in front of the camera and click the recording button to start recording process. Once done recording you will be for some information to fill and after that your video will be saved in our database.");
        }
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
        ReferenceManager.instance.lightBuzzViewer.Visualization = FrameVisualization.Color;
        videoRecorderView.Show();
        ResearchMeasurementManager.instance.isDoneWithLeft =false;
        ResearchMeasurementManager.instance.isDoneWithRight = false;
    }

    public List<UnityWebRequest> requests = new List<UnityWebRequest>();
    public ReportSectionManager reportSectionManager;
    float progress;

    public IEnumerator GetText(string url, Button btn, UserReportFromDB userReportFromDB)
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
            userReportFromDB.Download.SetActive(false);
            userReportFromDB.Error.SetActive(true);
            userReportFromDB.Watch.SetActive(false);
            userReportFromDB.ProgressImage.fillAmount = 0;
            userReportFromDB.ProgressImage.gameObject.SetActive(false);
            requests.Remove(request);
            userReportFromDB.request.Dispose();
            userReportFromDB.request = null;
            btn.onClick.AddListener(() =>
            {
                StartCoroutine(GetText(url, btn, userReportFromDB));
            });
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
                () => { CreateFileAndView((int)userReportFromDB.videoId,videoSaveBodies, url, userReportFromDB.UserNameOfSubject); ReferenceManager.instance.SelectedVideoID = userReportFromDB.videoId; ReferenceManager.instance.azureStorageManager.selectedVideo = userReportFromDB; }
            );
            btn.interactable = true;
            userReportFromDB.ProgressImage.gameObject.SetActive(false);
            userReportFromDB.ButtonText.text = $"Watch";
            userReportFromDB.Download.SetActive(false);
            userReportFromDB.Error.SetActive(false);
            userReportFromDB.Watch.SetActive(true);
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
                userReportFromDB.Download.SetActive(false);
                userReportFromDB.Error.SetActive(true);
                userReportFromDB.Watch.SetActive(false);
            }
            requests.Remove(userReportFromDB.request);
            userReportFromDB.request.Dispose();
            userReportFromDB.request = null;
        }
    }

    public void ClearLastPlayedVideoData()
    {
        if (RecentlyPlayedButton != null)
        {
            RecentlyPlayedButton.WatchBtn.onClick.RemoveAllListeners();
            string lastURL = PlayerPrefs.GetString("LastVidURL");
            Button lastbutton = RecentlyPlayedButton.WatchBtn;
            UserReportFromDB userReportFromDB = RecentlyPlayedButton;
            RecentlyPlayedButton.WatchBtn.onClick.AddListener(
                () =>
                    StartCoroutine(
                        GetText(
                            lastURL,
                            lastbutton,
                            userReportFromDB
                        )
                    )
            );
            RecentlyPlayedButton.ButtonText.text = "Download";
            RecentlyPlayedButton.Download.SetActive(true);
            RecentlyPlayedButton.Error.SetActive(false);
            RecentlyPlayedButton.Watch.SetActive(false);
            RecentlyPlayedButton.PreviewButton.gameObject.SetActive(false);
            RecentlyPlayedButton = null;
            PlayerPrefs.SetString("LastVidURL", "None");
            PlayerPrefs.SetInt("LastVidID",0);
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
           userReportFromDBs.Where((x=>x.MyReportGroupHandler.isDropped)).ToList().ForEach(x=>x.gameObject.SetActive(false));
            var matchingNames = userReportFromDBs
                .Where(x => x.UserNamefromDB.text.Contains(name, StringComparison.OrdinalIgnoreCase)|| x.ReportDescription.text.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var item in matchingNames)
            {
                if (item.MyReportGroupHandler.isDropped)
                {
                    item.gameObject.SetActive(true);
                }
               
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
                        item.Download.SetActive(false);
                        item.Error.SetActive(false);
                        item.Watch.SetActive(true);
                        if (request.downloadProgress == 0)
                        {
                            item.ButtonText.text = "Retry? No Data Found";
                            item.Download.SetActive(false);
                            item.Error.SetActive(true);
                            item.Watch.SetActive(false);
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
        int vidID,
        List<VideoSaveBody> videoSaveBodies = null,
        string url = "",
        string username = ""
    )
    {
        ReferenceManager.instance.LoadingManager.Show("Writing Video Files From Server");
        if (!GeneralStaticManager.GlobalVar.ContainsKey("Subject"))
            GeneralStaticManager.GlobalVar.Add("Subject", username);
        else
            GeneralStaticManager.GlobalVar["Subject"] = username;
        string path1 = System.IO.Path.Combine(Application.persistentDataPath, "Video");
        
        ReferenceManager.instance.isShowingRecording = true;
        if (!string.IsNullOrEmpty(url))
        {
            PlayerPrefs.SetString("LastVidURL", url);
            PlayerPrefs.SetInt("LastVidID", vidID);
            
        }
        if (Directory.Exists(path1) && videoSaveBodies != null)
        {
            Directory.Delete(path1, recursive: true);
        }
        string[] filePaths = null;
        if (videoSaveBodies != null)
        {
            Directory.CreateDirectory(path1);
            foreach (VideoSaveBody item in videoSaveBodies)
            {
                if (item.FileName.Equals("Sample.pdf"))
                {
                    continue;
                }
                string fileName = item.FileName;
                string fileData = item.FileData;
                Debug.Log(fileName);
                string path = System.IO.Path.Combine(
                    Application.persistentDataPath,
                    "Video",
                    fileName
                );
                
                byte[] bytes = System.Convert.FromBase64String(fileData);
                File.WriteAllBytes(path, bytes);
                
               
            }

            filePaths = Directory.GetFiles(path1);
            while (filePaths.Length < videoSaveBodies.Count - 1)
            {
                await Task.Delay(500);
            }
        }

        if (filePaths == null)
        {
            filePaths = Directory.GetFiles(path1);
        }
        if (filePaths.FirstOrDefault(x => x.Contains(".depth")) != null)
        {
            ReferenceManager.instance.lightBuzzViewer.Visualization = FrameVisualization.Depth;
        }
        else
        {
            ReferenceManager.instance.lightBuzzViewer.Visualization = FrameVisualization.Color;
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
        ReferenceManager.instance.LoadingManager.Hide();
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

   public DateTime ConvertToLocalTime(DateTime serverTime)
    {
        // Assuming the server time is in UTC, convert it to local time
        TimeZoneInfo localZone = TimeZoneInfo.Local;

        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(serverTime, localZone);
        return localTime;
    }

    
    
    public float offset;

    public async void SnapToChild(Transform stage,ScrollRect _scrollRect,RectTransform _contentPanel)
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
            addedReportGroupHandlers.ForEach(x=>x.MyVerticalLayoutGroup.reverseArrangement = false);
            // ReportsLayoutGroup.reverseArrangement = false;
            userReportFromDBs = userReportFromDBs.OrderBy(x => x.UserNamefromDB.text).ToList();
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
            addedReportGroupHandlers.ForEach(x=>x.MyVerticalLayoutGroup.reverseArrangement = false);
        }
        if(order == 3)
        {
            userReportFromDBs = userReportFromDBs.OrderBy(x => DateTime.Parse(x.CreatedOn.text)).ToList();
            for(int i = 0;i<userReportFromDBs.Count;i++)
            {
                userReportFromDBs[i].transform.SetSiblingIndex(i);
            }
            addedReportGroupHandlers.ForEach(x=>x.MyVerticalLayoutGroup.reverseArrangement = true);
        }
    }
}
