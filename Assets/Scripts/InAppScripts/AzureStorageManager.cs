using System;
using System.Collections;
using System.Collections.Generic;
using FastForward.CAS;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using DG.Tweening;
public class AzureStorageManager : MonoBehaviour
{


    public string accountName;
    public string accountKey;
    public string container;
    public float delayBetweenCalls;
    public string reportURL;
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
    private void UploadTextCallback(bool success, string error, string uri)
    {
        if (success)
        {
            Debug.Log($"Video uploaded this is the url {uri}");
            string ReportDesc = "<b>Video Recorded For:</b>\n";
            if(ReferenceManager.instance.graphManagers.Count == 0) {
                ReportDesc = "No Description";
            }
            foreach(var item in ReferenceManager.instance.graphManagers)
            {
                ReportDesc += item.JointType;
                if(item.SecondJointType != LightBuzz.AvaSci.Measurements.MeasurementType.None)
                {
                    ReportDesc += ", "+item.SecondJointType;
                }
                if(ReferenceManager.instance.graphManagers.IndexOf(item) != ReferenceManager.instance.graphManagers.Count - 1)
                {
                    ReportDesc += ", ";
                }
            }
            ReportRecordBody reportRecordBody = new ReportRecordBody()
            {
                UserName = GeneralStaticManager.GlobalVar["UserName"],
                VideoURL = uri,
                ReportURL = reportURL,
                ReportDescription = ReportDesc
            };
            string json = JsonConvert.SerializeObject(reportRecordBody);
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
