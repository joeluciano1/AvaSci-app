using System;
using System.Collections;
using System.Collections.Generic;
using FastForward.CAS;
using Newtonsoft.Json;
using UnityEngine;

public class AzureStorageManager : MonoBehaviour
{


    public string accountName;
    public string accountKey;
    public string container;
    public float delayBetweenCalls;
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

        AzureConnector.Instance.UploadText(json, container, fileName, true, UploadTextCallback);

    }
    private void UploadTextCallback(bool success, string error, string uri)
    {
        if (success)
        {
            Debug.Log($"Video uploaded this is the url {uri}");
            ReportRecordBody reportRecordBody = new ReportRecordBody()
            {
                UserName = GeneralStaticManager.GlobalVar["UserName"],
                VideoURL = uri
            };
            string json = JsonConvert.SerializeObject(reportRecordBody);
            APIHandler.instance.Post("UserReport/PostReport", json, onSuccess: (response) =>
            {
                ResponseWithNoObject responseWithNoObject = JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
                if (responseWithNoObject.isSuccess)
                {
                    ReferenceManager.instance.PopupManager.Show("Video Save Success!", $"Your Video has been saved successfully", true);
                }
                if (responseWithNoObject.isError)
                {
                    string reasons = "";
                    foreach (var item in responseWithNoObject.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    ReferenceManager.instance.PopupManager.Show("Video Save Failed!", $"Reasons are: {reasons}");
                    Debug.Log($"{responseWithNoObject.serviceErrors}");
                }
            }, onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show("Video Save Failed!", $"Reasons are: {error}");
                Debug.LogError($"Error: {error}");
            });
        }
        else
        {
            Debug.LogWarning("Video upload failed. " + error);
        }
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
