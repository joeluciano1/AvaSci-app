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
    public void UploadTextCallback(bool success, string error, string uri)
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
                SubjectId = selectedPatient != null ? selectedPatient.SubjectId: selectedVideo.UserName.text
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
            reportRecordBody.TimeBasedReadings = JsonConvert.DeserializeObject<List<TimeBasedReadingRequest>>(GeneralStaticManager.ConvertCsvStringToJson(ReferenceManager.instance.LightBuzzMain.GenerateCSVString()));
            reportRecordBody.TimeBasedReadings.ForEach(x => x.UserName = selectedPatient != null ? selectedPatient.PatientName : selectedVideo.UserNameOfSubject);
            //     float? GetMeasurementValue(string key, int index)
            //     {
            //         var kvp = GeneralStaticManager.GraphsReadings
            //             .FirstOrDefault(x => x.Key == key);
            //         return kvp.Value != null && kvp.Value.Count > index ? kvp.Value[index] : (float?)null;
            //     }

            //     int indexCount = 0;
            //     float timeCount = 0;
            // foreach (var item in GeneralStaticManager.GraphsReadings.ElementAt(0).Value)
            // {

            //     TimeBasedReadingRequest timeBasedReadingRequest = new TimeBasedReadingRequest();

            //     // Assigning values
            //     timeBasedReadingRequest.KneeLeftAbduction = GetMeasurementValue(MeasurementType.KneeLeftAbduction.ToString(), indexCount);
            //     timeBasedReadingRequest.KneeRightAbduction = GetMeasurementValue(MeasurementType.KneeRightAbduction.ToString(), indexCount);
            //     timeBasedReadingRequest.PelvisAngle = GetMeasurementValue(MeasurementType.PelvisAngle.ToString(), indexCount);
            //     timeBasedReadingRequest.AnkleHipLeftAbductionDifference = GetMeasurementValue(MeasurementType.HipAnkleHipKneeLeftAbductionDifference.ToString(), indexCount);
            //     timeBasedReadingRequest.AnkleHipRightAbductionDifference = GetMeasurementValue(MeasurementType.HipAnkleHipKneeRightAbductionDifference.ToString(), indexCount);
            //     timeBasedReadingRequest.HipKneeRightDistance = GetMeasurementValue(MeasurementType.HipKneeRightDistance.ToString(), indexCount);
            //     timeBasedReadingRequest.HipKneeLeftDistance = GetMeasurementValue(MeasurementType.HipKneeLeftDistance.ToString(), indexCount);
            //     timeBasedReadingRequest.NeckLeteralFlexion = GetMeasurementValue(MeasurementType.NeckLateralFlexion.ToString(), indexCount);
            //     timeBasedReadingRequest.NeckRotation = GetMeasurementValue(MeasurementType.NeckRotation.ToString(), indexCount);
            //     timeBasedReadingRequest.ElbowLeftFlexion = GetMeasurementValue(MeasurementType.ElbowLeftFlexion.ToString(), indexCount);
            //     timeBasedReadingRequest.ElbowRightFlexion = GetMeasurementValue(MeasurementType.ElbowRightFlexion.ToString(), indexCount);
            //     timeBasedReadingRequest.ShoulderLeftAbduction = GetMeasurementValue(MeasurementType.ShoulderLeftAbduction.ToString(), indexCount);
            //     timeBasedReadingRequest.ShoulderLeftRotation = GetMeasurementValue(MeasurementType.ShoulderLeftRotation.ToString(), indexCount);
            //     timeBasedReadingRequest.ShoulderRightAbduction = GetMeasurementValue(MeasurementType.ShoulderRightAbduction.ToString(), indexCount);
            //     timeBasedReadingRequest.ShoulderRightRotation = GetMeasurementValue(MeasurementType.ShoulderRightRotation.ToString(), indexCount);
            //     timeBasedReadingRequest.ShoulderLeftFlexion = GetMeasurementValue(MeasurementType.ShoulderLeftFlexion.ToString(), indexCount);
            //     timeBasedReadingRequest.ShoulderRightFlexion = GetMeasurementValue(MeasurementType.ShoulderRightFlexion.ToString(), indexCount);
            //     timeBasedReadingRequest.HipLeftAbduction = GetMeasurementValue(MeasurementType.HipLeftAbduction.ToString(), indexCount);
            //     timeBasedReadingRequest.HipLeftFlexion = GetMeasurementValue(MeasurementType.HipLeftFlexion.ToString(), indexCount);
            //     timeBasedReadingRequest.HipRightAbduction = GetMeasurementValue(MeasurementType.HipRightAbduction.ToString(), indexCount);
            //     timeBasedReadingRequest.HipRightFlexion = GetMeasurementValue(MeasurementType.HipRightFlexion.ToString(), indexCount);
            //     timeBasedReadingRequest.KneeLeftFlexion = GetMeasurementValue(MeasurementType.KneeLeftFlexion.ToString(), indexCount);
            //     timeBasedReadingRequest.KneeRightFlexion = GetMeasurementValue(MeasurementType.KneeRightFlexion.ToString(), indexCount);
            //     timeBasedReadingRequest.AnkleLeftAbduction = GetMeasurementValue(MeasurementType.AnkleLeftAbduction.ToString(), indexCount);
            //     timeBasedReadingRequest.AnkleRightAbduction = GetMeasurementValue(MeasurementType.AnkleRightAbduction.ToString(), indexCount);
            //     timeBasedReadingRequest.VarusValgusLeft = GetMeasurementValue(MeasurementType.VarusValgusLeftAngleDistance.ToString(), indexCount);
            //     timeBasedReadingRequest.VarusValgusRight = GetMeasurementValue(MeasurementType.VarusValgusRightAngleDistance.ToString(), indexCount);
            //     timeBasedReadingRequest.TimeOfReading = TimeSpan.FromSeconds(timeCount).ToString(@"mm\:ss\:fff");
            //     timeBasedReadingRequest.UserName = selectedPatient != null ? selectedPatient.PatientName : selectedVideo.UserNameOfSubject;
            //     indexCount += 1;
            //     timeCount += 0.001f;

            //     reportRecordBody.TimeBasedReadings.Add(timeBasedReadingRequest);
            // }
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
                    ReferenceManager.instance.PopupManager.Show("Video Save Failed!", $"Reasons are: {reasons}");
                    AzureConnector.Instance.NumberOfVideosUploading -= 1;
                    if (AzureConnector.Instance.NumberOfVideosUploading <= 0)
                    {
                        ReferenceManager.instance.UploadingImage.gameObject.SetActive(false);
                    }
                    ReferenceManager.instance.UploadingImage.transform.GetChild(1).GetComponent<TMP_Text>().text = AzureConnector.Instance.NumberOfVideosUploading.ToString();
                }
            }, onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show("Video Save Failed!", $"Reasons are: {error}");
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
