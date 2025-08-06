using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using FastForward.CAS;
using System;
using System.Linq;
using Newtonsoft.Json;
using DG.Tweening;

public class UserReportFromDB : MonoBehaviour
{
    public long videoId;
    public string UserNameOfSubject;
    public TMP_Text UserNamefromDB;
    public TMP_Text CreatedOn;
    public TMP_Text ReportDescription;

    public Button WatchBtn;
    public Button PreviewButton;
    public Button HtmlButton;
    public Image ProgressImage;
    public UnityWebRequest request;

    public TMP_Text ButtonText;
    public string VideoURL;
    public string UserId;
    public GameObject DeleteButton;
    public Transform DropperToggle;
    public Transform DropDownItems;
    public RectTransform Content;
    ContentSizeFitter ContentSizeFitter;
    public Toggle CompareViewToggle;
    public Toggle CompareGaitToggle;
    public List<JointReading> jointReadings = new List<JointReading>();
    public List<TimeBasedReadingRequest> timeBasedReadings = new List<TimeBasedReadingRequest>();
    public List<GetGaitReportResponse> gaitReports = new List<GetGaitReportResponse>();
    
    public ScrollRect MyScrollRect;
    public ReportGroupHandler MyReportGroupHandler;

    public GameObject Watch;
    public GameObject Download;
    public GameObject Error;
    
    public HtmlReportFromDb HtmlReportFromDbPrefab;
    public GameObject HtmlReportsScroller;
    public GameObject SelectedReportsViewScroller;
    public UserReportData mydata;
    public string mySubGroup;
    private void Start()
    {
        // jointReadings.ForEach(x => x.VideoNameLink = ReportDescription.text.Replace("<b>Comment:</b>", ""));
        ContentSizeFitter = Content.GetComponent<ContentSizeFitter>();
        if (GeneralStaticManager.GlobalVar["UserRoles"].Contains("SuperUser"))
        {
            DeleteButton.SetActive(true);
        }
        else
        {
            DeleteButton.SetActive(false);
        }
    }

    public void CheckIfItContainsTimeBasedReadings()
    {
        if (timeBasedReadings != null && timeBasedReadings.Count > 0)
        {
            CompareViewToggle.interactable = true;
            CompareViewToggle.transform.GetChild(0).GetComponent<TMP_Text>().text = "Select To Compare";
        }

        if (gaitReports != null && gaitReports.Count > 0)
        {
            CompareGaitToggle.interactable = true;
            CompareGaitToggle.transform.GetChild(0).GetComponent<TMP_Text>().text = "Select To Compare Gait";
        }
    }
   
    public void DeleteVide()
    {
        ReferenceManager.instance.PopupManager.Show("Delete Video?","Are you sure you want to delete this video?",noButtonName:"No",okPressed:()=>
        {
            PerformDeleteOperation();
        });
        
    }

    public void PerformDeleteOperation()
    {
        if (VideoURL.Contains("http"))
        {
            Uri uri = new Uri(VideoURL);
            string containerName = uri.Segments[1].TrimEnd('/');
            string blobName = uri.Segments[2];
            AzureConnector.Instance.DeleteBlob(containerName, blobName, OnDeleteCallBack);
        }
        else
        {
            OnDeleteCallBack(true, "");
        }
    }
    public void OnDeleteCallBack(bool success, string error, string uri = null)
    {
        ReportDeleteBody reportDeleteBody = new ReportDeleteBody()
        {
            UserId = UserId,
            VideoURL = VideoURL
        };
        string json = JsonConvert.SerializeObject(reportDeleteBody);
        APIHandler.instance.Post("UserReport/DeleteReport", json, onSuccess: (response) =>
        {
            ResponseWithNoObject responseWithNoObject = JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
            if (responseWithNoObject.isSuccess)
            {
                ReferenceManager.instance.PopupManager.Show("Report Delete Success!", $"Report Deleted Successfully");
                MyReportGroupHandler.DropDownItems.Remove(this);
                Destroy(gameObject);
            }
            if (responseWithNoObject.isError)
            {
                string reasons = "";
                foreach (var item in responseWithNoObject.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }
                ReferenceManager.instance.PopupManager.Show("Report Delete Failed!", $"Reasons are: {reasons}");
                Debug.Log($"{responseWithNoObject.serviceErrors}");
            }

        }, onError: (error) =>
        {
            ReferenceManager.instance.PopupManager.Show("Report Delete Failed!", $"Reasons are: {error}");
        });
    }
    List<HtmlReportFromDb> addedHtmlReports = new List<HtmlReportFromDb>();
    public void ListAvailableHTMLs()
    {
        GetHtmlReportRequest request = new GetHtmlReportRequest()
        {
            ReportRecordId = videoId
        };
        string json = JsonConvert.SerializeObject(request);
        SelectedReportsViewScroller.SetActive(false);
        HtmlReportsScroller.SetActive(true);
        APIHandler.instance.Post("UserReport/GetHtmlReport", json, onSuccess: (response) =>
        {
            HtmlBaseResponse responseWithNoObject = JsonConvert.DeserializeObject<HtmlBaseResponse>(response);
            if (responseWithNoObject.isSuccess)
            {
                ReferenceManager.instance.PopupManager.Show("Reports Fetch Success!", $"HTML Reports Fetched Successfully");
                foreach (var item in responseWithNoObject.result.htmlContents)
                {
                    if (addedHtmlReports.FirstOrDefault(x => x.HTMLLink == item.HtmlReport) == null)
                    {
                        HtmlReportFromDb htmlReportFromDb = Instantiate(HtmlReportFromDbPrefab,
                            HtmlReportFromDbPrefab.transform.parent);
                        htmlReportFromDb.gameObject.SetActive(true);
                        htmlReportFromDb.CreatedByText.text = item.CreatedBy;
                        htmlReportFromDb.CreatedDateText.text = item.CreatedOn;
                        htmlReportFromDb.HTMLLink = item.HtmlReport;
                        htmlReportFromDb.viewButton.onClick.AddListener(() => Application.OpenURL(item.HtmlReport));
                        addedHtmlReports.Add(htmlReportFromDb);
                    }
                }
            }
            if (responseWithNoObject.isError)
            {
                string reasons = "";
                foreach (var item in responseWithNoObject.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }
                ReferenceManager.instance.PopupManager.Show("HTML Report Fetch Failed!", $"Reasons are: {reasons}");
                Debug.Log($"{responseWithNoObject.serviceErrors}");
            }

        }, onError: (error) =>
        {
            ReferenceManager.instance.PopupManager.Show("HTML Report Fetch Failed!", $"Reasons are: {error}");
        });
    }
    public void ToggleDropDown(bool value)
    {
        RectTransform myRect = GetComponent<RectTransform>();
        
        ContentSizeFitter.enabled = false;
        ContentSizeFitter.SetLayoutVertical(); // can use SetLayoutHorizontal as well
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
        ContentSizeFitter.enabled = true;
        if (value)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
            DropperToggle.DORotate(new Vector3(0, 0, 180), 0.5f);
            myRect.DOSizeDelta(new Vector2(myRect.sizeDelta.x, 170), 0.5f);
            DropDownItems.DOScale(new Vector3(1, 1, 1), 0.5f);
           
        }
        else
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
            DropperToggle.DORotate(new Vector3(0, 0, 0), 0.5f);
            myRect.DOSizeDelta(new Vector2(myRect.sizeDelta.x, 60), 0.5f);
            DropDownItems.DOScale(new Vector3(1, 0, 1), 0.5f);
            
        }
    }

    public void AskToShareRecording()
    {
        ReferenceManager.instance.userReportController.ShareRecordingPopup.SetActive(true);
        ReferenceManager.instance.userReportController.ShareRecordingButton.onClick.RemoveAllListeners();
        ReferenceManager.instance.userReportController.ShareRecordingButton.onClick.AddListener(ShareRecording);
    }
    public void ShareRecording()
    {
        if (string.IsNullOrEmpty(ReferenceManager.instance.userReportController.UserEmailToShareWith.text))
        {
            ReferenceManager.instance.PopupManager.Show("Error", $"Please enter your email address");
            return;
        }
        ReferenceManager.instance.userReportController.ShareRecordingPopup.SetActive(false);
        ShareRecordingRequest shareRecordingRequest = new ShareRecordingRequest()
        {
            SharedBy = UserId,
            UserEmail = ReferenceManager.instance.userReportController.UserEmailToShareWith.text,
            VideoURL = VideoURL,
            RecordingId = videoId
        };
        string json = JsonConvert.SerializeObject(shareRecordingRequest);
        APIHandler.instance.Post("UserReport/ShareRecording", json, onSuccess: (response) =>
        {
            ResponseWithNoObject responseWithNoObject = JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
            if (responseWithNoObject.isSuccess)
            {
                ReferenceManager.instance.PopupManager.Show("Share Recording Success!", $"Shared Recording Successfully");
            }
            else
            {
                string reasons = "";
                foreach (var item in responseWithNoObject.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }
                ReferenceManager.instance.PopupManager.Show("Sharing Recording Failed!!", $"Reasons are: {reasons}");
            }
        }, onError: (error) =>
        {
            ReferenceManager.instance.PopupManager.Show("Sharing Recording Failed!!", $"Reasons are: {error}");
        });
    }
}
