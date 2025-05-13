using System;
using System.Collections;
using System.Collections.Generic;
using FastForward.CAS;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System.IO;
using LightBuzz.BodyTracking.Video;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.AvaSci.Csv;
public class AzureStorageManager : MonoBehaviour
{


    public string accountName;
    public string accountKey;
    public string container;
    public float delayBetweenCalls;
    public string reportURL;
    public UserReportFromDB selectedVideo;
    // Start is called before the first frame update
    void Start()
    {
        AzureConnector.Instance.Init(accountName, accountKey);
        // StartCoroutine(InitiateContainers());
    }
    // public IEnumerator InitiateContainers()
    // {
    //     var waitForSeconds = new WaitForSeconds(delayBetweenCalls);
    //     yield return waitForSeconds;
    //     AzureConnector.Instance.ListContainers(ListContainersCallback);
    //     yield return waitForSeconds;
    //     AzureConnector.Instance.ListBlobs(container, ListBlobsCallback);
    // }

    public void UploadVideo(string json, string fileName)
    {
        ReferenceManager.instance.UploaderAnimation.gameObject.SetActive(true);
        ReferenceManager.instance.UploaderAnimation.DOMove(ReferenceManager.instance.UploadingImage.GetComponent<Transform>().position, 2).OnComplete(() =>
        {
            ReferenceManager.instance.UploaderAnimation.gameObject.SetActive(false);
            ReferenceManager.instance.UploaderAnimation.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        });
        ReferenceManager.instance.UploaderAnimation.GetComponent<RectTransform>().DOSizeDelta(ReferenceManager.instance.UploadingImage.GetComponent<RectTransform>().sizeDelta, 2);
        AzureConnector.Instance.UploadText(json, container, fileName, true, UploadTextCallback);

    }
    public async void UploadTextCallback(bool success, string error, string uri)
    {
        if (success)
        {
            Debug.Log($"Video uploaded this is the url {uri}");
            string ReportDesc = "<b>Comment:</b>\n";
            if(string.IsNullOrEmpty(ReferenceManager.instance.commentQuestionnaire.CommentInputField.text)) {
                ReportDesc = "No Description";
            }
            else
            {
                ReportDesc += ReferenceManager.instance.commentQuestionnaire.CommentInputField.text;
            }
            PlayerPrefs.SetString("LastVidURL", uri);
            var selectedPatient = ReferenceManager.instance.LoginManager.signinResponse.result.patients.FirstOrDefault(x => x.SubjectId == ReferenceManager.instance.commentQuestionnaire.PatientsDropDown.captionText.text || x.PatientName == ReferenceManager.instance.commentQuestionnaire.PatientsDropDown.captionText.text);
            ReportRecordBody reportRecordBody = new ReportRecordBody()
            {
                CreatedBy = GeneralStaticManager.GlobalVar["UserName"],
                UserName = selectedPatient != null ? selectedPatient.PatientName : selectedVideo.UserNameOfSubject,
                VideoURL = uri,
                ReportURL = reportURL,
                ReportDescription = ReportDesc,
                GroupName = ReferenceManager.instance.commentQuestionnaire.GroupNameDropDown.captionText.text,
                SubjectId = selectedPatient != null ? selectedPatient.SubjectId: selectedVideo.UserNamefromDB.text
            };
            foreach(var item in ReferenceManager.instance.ButtonHandler.graphDatas)
            {
                JointReading jointReading = new JointReading()
                {
                    NameOfReading = item.Graph1Name,
                    MinimumValue = float.Parse(item.MinGraph1Value),
                    MaximumValue = float.Parse(item.MaxGraph1Value),
                    RangeValue = float.Parse(item.RangeGraph1Value)
                };
                reportRecordBody.jointReadings.Add(jointReading);
                if(string.IsNullOrEmpty(item.Graph2Name)){
                    continue;
                }
                JointReading secondjointReading = new JointReading()
                {
                    NameOfReading = item.Graph2Name,
                    MinimumValue = float.Parse(item.MinGraph2Value),
                    MaximumValue = float.Parse(item.MaxGraph2Value),
                    RangeValue = float.Parse(item.RangeGraph2Value)
                };
                reportRecordBody.jointReadings.Add(secondjointReading);
            }
            //////////////// TimeBasedReadingThing ////////////////////////////
            ///// Helper function to fetch values safely
            ///
            string theCSVJson =
                await GeneralStaticManager.ConvertCsvStringToJson(ReferenceManager.instance.LightBuzzMain
                    .GenerateCSVString());
            reportRecordBody.TimeBasedReadings = JsonConvert.DeserializeObject<List<TimeBasedReadingRequest>>(theCSVJson);
            reportRecordBody.TimeBasedReadings.ForEach(x =>
            {
                x.UserName = selectedPatient != null
                    ? selectedPatient.PatientName
                    : selectedVideo.UserNameOfSubject;
                x.ReportsRecordId = ReferenceManager.instance.SelectedVideoID;
            });
           
        //////////////// TimeBasedReadingThing ////////////////////////////
            string json = JsonConvert.SerializeObject(reportRecordBody);
            Debug.Log("Upload json:" +json);
            APIHandler.instance.Post("UserReport/PostReport", json, onSuccess: (response) =>
            {
                ResponseWithNoObject responseWithNoObject = JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
                if (responseWithNoObject.isSuccess)
                {
                    // ReferenceManager.instance.PopupManager.Show("Video Save Success!", $"Your Video has been saved successfully", true);
                    AzureConnector.Instance.NumberOfVideosUploading -= 1;
                    if (AzureConnector.Instance.NumberOfVideosUploading <= 0)
                    {
                        ReferenceManager.instance.UploadingImage.gameObject.SetActive(false);
                    }
                    ReferenceManager.instance.UploadingImage.transform.GetChild(1).GetComponent<TMP_Text>().text = AzureConnector.Instance.NumberOfVideosUploading.ToString();
                    PlayerPrefs.SetString("LastVidURL", uri);
                }
                if (responseWithNoObject.isError)
                {
                    string reasons = "";
                    foreach (var item in responseWithNoObject.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    ReferenceManager.instance.PopupManager.Show("Video Save To DB Failed!", $"Reasons are: {reasons}");
                    AzureConnector.Instance.NumberOfVideosUploading -= 1;
                    if (AzureConnector.Instance.NumberOfVideosUploading <= 0)
                    {
                        ReferenceManager.instance.UploadingImage.gameObject.SetActive(false);
                    }
                    ReferenceManager.instance.UploadingImage.transform.GetChild(1).GetComponent<TMP_Text>().text = AzureConnector.Instance.NumberOfVideosUploading.ToString();
                }
            }, onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show("Video Save To DB Failed!", $"Reasons are: {error}");
                AzureConnector.Instance.NumberOfVideosUploading -= 1;
                if (AzureConnector.Instance.NumberOfVideosUploading <= 0)
                {
                    ReferenceManager.instance.UploadingImage.gameObject.SetActive(false);
                }
                ReferenceManager.instance.UploadingImage.transform.GetChild(1).GetComponent<TMP_Text>().text = AzureConnector.Instance.NumberOfVideosUploading.ToString();
            });
            
        }
        else
        {
            ReferenceManager.instance.PopupManager.Show("Error", $"Got error while Uploading wana Retry?{error}",yesButtonName:"Yes",noButtonName:"No",okPressed:
                () =>
                {
                    ReferenceManager.instance.UploadVideo(ReferenceManager.instance.recorderPath);
                });
            AzureConnector.Instance.NumberOfVideosUploading -= 1;
            if (AzureConnector.Instance.NumberOfVideosUploading <= 0)
            {
                ReferenceManager.instance.UploadingImage.gameObject.SetActive(false);
            }
            ReferenceManager.instance.UploadingImage.transform.GetChild(1).GetComponent<TMP_Text>().text = AzureConnector.Instance.NumberOfVideosUploading.ToString();
        }
        reportURL = "";
    }

    private void ListBlobsCallback(bool success, string error, string text)
    {
        if (success)
        {
            Debug.Log(text);
        }
        else
        {
            Debug.LogWarning("List Blobs failed. " + error);
        }
    }
    private void ListContainersCallback(bool success, string error, string text)
    {
        if (success)
        {
            Debug.Log(text);
        }
        else
        {
            Debug.LogWarning("List Containers failed. " + error);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
