using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using FastForward.CAS;
using System;
using Newtonsoft.Json;

public class UserReportFromDB : MonoBehaviour
{
    public TMP_Text UserName;
    public TMP_Text CreatedOn;

    public Button WatchBtn;

    public Image ProgressImage;
    public UnityWebRequest request;

    public TMP_Text ButtonText;
    public string VideoURL;
    public string UserId;
    public GameObject DeleteButton;

    private void Start()
    {
        if (GeneralStaticManager.GlobalVar["UserRoles"].Contains("SuperUser"))
        {
            DeleteButton.SetActive(true);
        }
        else
        {
            DeleteButton.SetActive(false);
        }
    }

    public void DeleteVide()
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
}
