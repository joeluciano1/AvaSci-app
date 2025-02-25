using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReportSectionManager : MonoBehaviour
{
    public Color selectedColor;
    public Color nonSelectedColor;
    public Button ReportsButton;
    public Button ClinicsButton;
    public Button SharedReportsButton;
        public GameObject SelectedReportsView;
    public GameObject ReportsSection;
    public GameObject ClinicsSection;
    public GameObject HeadingReports;
    public GameObject SharedReportsSection;
    
    public Transform SharedReportsSectionTransform;
    public ClinicDataFromDB clinicDataFromDB;
    public Button htmlBackButton;
    bool clinicDataFilled;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClinicsButton.onClick.RemoveAllListeners();
        ReportsButton.onClick.RemoveAllListeners();
        SharedReportsButton.onClick.RemoveAllListeners();

        ClinicsButton.onClick.AddListener(() => OpenClinic());
        ReportsButton.onClick.AddListener(()=> OpenReports());
        SharedReportsButton.onClick.AddListener(() => OpenSharedReports());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OpenClinic()
    {
        ReportsButton.targetGraphic.color = nonSelectedColor;
        ClinicsButton.targetGraphic.color = selectedColor;
        SharedReportsButton.targetGraphic.color = nonSelectedColor;
        
        ReportsSection.SetActive(false);
        LastOpened.SetActive(false);
        ClinicsSection.SetActive(true);
        HeadingReports.SetActive(false);
        SharedReportsSection.SetActive(false);

        if(!clinicDataFilled)
        FeedClinicData();
    }
    public void OpenReports()
    {
        ReportsButton.targetGraphic.color = selectedColor;
        ClinicsButton.targetGraphic.color = nonSelectedColor;
        SharedReportsButton.targetGraphic.color = nonSelectedColor;
        if(LastOpened==ReportsSection)
            ReportsSection.SetActive(true);
        else
        {
            LastOpened.SetActive(true);
        }
        ClinicsSection.SetActive(false);
        HeadingReports.SetActive(true);
        SharedReportsSection.SetActive(false);
    }
    List<ClinicDataFromDB> clinicDataFromDBStored = new List<ClinicDataFromDB>();
    public void FeedClinicData()
    {
        if(clinicDataFromDBStored.Count !=0)
        {
            clinicDataFromDBStored.ForEach(x => Destroy(x.gameObject));
            clinicDataFromDBStored.Clear();
        }
        clinicDataFilled = true;
        foreach(var item in ReferenceManager.instance.LoginManager.signinResponse.result.clinics)
        {
            ClinicDataFromDB dataFromDB = Instantiate(clinicDataFromDB, clinicDataFromDB.transform.parent);
            clinicDataFromDBStored.Add(dataFromDB);
            dataFromDB.gameObject.SetActive(true);
            dataFromDB.ClinicName.text = "Clinic Name: "+item.ClinicName;
            var doctorsInThisClinic = ReferenceManager.instance.LoginManager.signinResponse.result.doctors.Where(x => x.DoctorClinicId == item.ClinicId).ToList();
            foreach(var itemDoc in doctorsInThisClinic)
            {
                var patientsInDoctor = ReferenceManager.instance.LoginManager.signinResponse.result.patients.Where(x=>x.DoctorName == itemDoc.DoctorName).ToList();
                dataFromDB.Description.text += $"\n - <b>DR.{itemDoc.DoctorName}</b>";
                foreach (var itemPat in patientsInDoctor)
                    dataFromDB.Description.text += $"\n      -{itemPat.SubjectId}";
            }

            StartCoroutine(ReBuild(dataFromDB));
        }
    }
    public IEnumerator ReBuild(ClinicDataFromDB dataFromDB)
    {
        dataFromDB.verticalLayoutGroup.enabled = false;
        yield return new WaitForEndOfFrame();
        dataFromDB.verticalLayoutGroup.enabled = true;
    }
    List<UserReportFromDB> userReportFromDBs = new List<UserReportFromDB>();
    private GameObject LastOpened;
    public GameObject NoImage;
    public void OpenSharedReports()
    {
        ReportsButton.targetGraphic.color = nonSelectedColor;
        ClinicsButton.targetGraphic.color = nonSelectedColor;
        SharedReportsButton.targetGraphic.color = selectedColor;
        
        ReportsSection.SetActive(false);
        LastOpened.SetActive(false);
        ClinicsSection.SetActive(false);
        HeadingReports.SetActive(false);
        SharedReportsSection.SetActive(true);
        GetSharedRecordingRequest getSharedRecordingRequest = new GetSharedRecordingRequest()
        {
            UserEmail = PlayerPrefs.GetString(StringConstants.LOGINEMAIL)
        };
        string json = JsonConvert.SerializeObject(getSharedRecordingRequest);
        APIHandler.instance.Post("UserReport/GetSharedRecording",json,onSuccess: (response) =>
        {
            GetSharedRecordingResponse getSharedRecordingResponse = JsonConvert.DeserializeObject<GetSharedRecordingResponse>(response);
            if (getSharedRecordingResponse.isSuccess)
            {
                if (getSharedRecordingResponse.result.Recordings.Count == 0)
                {
                    NoImage.SetActive(true);
                    return;
                }
                NoImage.SetActive(false);
                foreach (var item in getSharedRecordingResponse.result.Recordings)
                {
                        
                        UserReportFromDB user = userReportFromDBs.FirstOrDefault(x =>x.VideoURL == item.VideoURL);
                        if (user != null)
                        {
                            if (item.HasHtmlReports)
                            {
                                user.HtmlButton.gameObject.SetActive(true);
                            }
                            else
                            {
                                user.HtmlButton.gameObject.SetActive(false);
                            }
                            if (!PlayerPrefs.GetString("LastVidURL").Equals(user.VideoURL))
                            {
                                user.WatchBtn.onClick.RemoveAllListeners();
                                user.WatchBtn.onClick.AddListener(
                                    () =>
                                        StartCoroutine(ReferenceManager.instance.userReportController.GetText(user.VideoURL, user.WatchBtn, user))
                                );
                                user.ButtonText.text = "Download";
                                user.Download.SetActive(true);
                                user.Error.SetActive(false);
                                user.Watch.SetActive(false);
                            }
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
                                        ReferenceManager.instance.userReportController.selectedReadings.Add(user);
                                    }
                                    else{
                                        ReferenceManager.instance.userReportController.selectedReadings.Remove(user);
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
                                        ReferenceManager.instance.userReportController.selectedGaitReadings.Add(user);
                                    }
                                    else
                                    {
                                        ReferenceManager.instance.userReportController.selectedGaitReadings.Remove(user);
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
                            ReferenceManager.instance.userReportController.userReportFromDBPrefab,
                            SharedReportsSectionTransform
                        );
                        userReportFromDB.videoId = item.Id;
                        userReportFromDB.UserId = item.UserID;
                        userReportFromDB.UserNameOfSubject = item.UserName;
                        userReportFromDB.VideoURL = item.VideoURL;
                        
                        if (item.HasHtmlReports)
                        {
                            userReportFromDB.HtmlButton.gameObject.SetActive(true);
                            userReportFromDB.HtmlButton.onClick.AddListener(()=>SharedReportsSection.SetActive(false));
                            htmlBackButton.onClick.RemoveAllListeners();
                            htmlBackButton.onClick.AddListener(()=>
                            {
                                SharedReportsSection.SetActive(true);
                                SelectedReportsView.SetActive(false);
                            });
                        }
                        else
                        {
                            userReportFromDB.HtmlButton.gameObject.SetActive(false);
                        }
                        string groupName = string.IsNullOrEmpty(item.GroupName)? "Other" : item.GroupName;
                       userReportFromDB.UserName.text += " (" + groupName + ")";
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
                                        ReferenceManager.instance.userReportController.selectedReadings.Add(userReportFromDB);
                                    }
                                    else{
                                        ReferenceManager.instance.userReportController.selectedReadings.Remove(userReportFromDB);
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
                                    ReferenceManager.instance.userReportController.selectedGaitReadings.Add(userReportFromDB);
                                }
                                else
                                {
                                    ReferenceManager.instance.userReportController.selectedGaitReadings.Remove(userReportFromDB);
                                }
                            });
                        }
                        else
                        {
                            userReportFromDB.CompareGaitToggle.transform.GetChild(0).GetComponent<TMP_Text>().text = "No Gait Reading Exists";
                            userReportFromDB.CompareGaitToggle.interactable = false;
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
                            DateTime localTime = ReferenceManager.instance.userReportController.ConvertToLocalTime(serverTime);
                            userReportFromDB.CreatedOn.text = localTime.ToString(
                                "MM/dd/yyyy h:mm:ss tt"
                            );
                            // Debug.Log($"Yes: {item.CreatedOn}");
                        }
                        else if (DateTime.TryParseExact(item.CreatedOn,"M/d/yyyy hh:mm:ss tt",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out serverTime))
                        {
                            DateTime localTime = ReferenceManager.instance.userReportController.ConvertToLocalTime(serverTime);
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
                                            ReferenceManager.instance.userReportController.GetText(
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
                        else
                        {
                            userReportFromDB.WatchBtn.onClick.RemoveAllListeners();
                            userReportFromDB.ButtonText.text = "Watch";
                            userReportFromDB.Download.SetActive(false);
                            userReportFromDB.Error.SetActive(false);
                            userReportFromDB.Watch.SetActive(true);
                            ReferenceManager.instance.userReportController.itemToSnapTo = userReportFromDB;
                            ReferenceManager.instance.userReportController.RecentlyPlayedButton = userReportFromDB;
                            userReportFromDB.WatchBtn.onClick.AddListener(
                                () => { ReferenceManager.instance.userReportController.CreateFileAndView(null, "", userReportFromDB.UserNameOfSubject); 
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
                                    () => ReferenceManager.instance.userReportController.CreateReportAndView()
                                );
                            }
                        }

                        userReportFromDBs.Add(userReportFromDB);
                    }
            }
            else
            {
                string reasons = "";
                foreach (var item in getSharedRecordingResponse.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }
                ReferenceManager.instance.PopupManager.Show("Getting Shared Recordings Failed!!", $"Reasons are: {reasons}");
            }
        },onError: (error) =>
        {
            ReferenceManager.instance.PopupManager.Show("Getting Shared Recordings Failed!", $"Reasons are: {error}");
        });
    }

    public void SetLastOpened(GameObject lastOpened)
    {
        LastOpened  = lastOpened;
    }

    public void BackSharedReports()
    {
        SharedReportsSection.SetActive(false);
        LastOpened.SetActive(true);
    }
    private void LateUpdate()
    {
        foreach (var item in userReportFromDBs)
        {
            if (item.request != null)
            {
                var request = ReferenceManager.instance.userReportController.requests.FirstOrDefault(x => x.url == item.request.url);
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
}
