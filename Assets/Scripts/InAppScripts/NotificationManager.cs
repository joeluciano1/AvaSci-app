using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
   public List<NotificationPrefab> notificationPrefabs = new List<NotificationPrefab>();
   public NotificationPrefab notificationPrefab;

   private void Start()
   {
       CheckNewSharedRecordings();
   }

   public async void CheckSharedRecordingAgain()
   {
       await Task.Delay(5000);
       CheckNewSharedRecordings();
   }
   public void CheckNewSharedRecordings()
   {
       if (string.IsNullOrEmpty(PlayerPrefs.GetString(StringConstants.LOGINEMAIL)) || APIHandler.instance==null )
       {
           CheckSharedRecordingAgain();
           return;
       }
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
                
                foreach (var item in getSharedRecordingResponse.result.Recordings)
                {
                    if (item.IsNew)
                    {
                        ShowNormalNoticiation("Notification Video Shared",$"{item.UserName} shared recording with you about {item.ReportDescription}");
                    }   
                    CheckSharedRecordingAgain();
                }
            }
            else
            {
                CheckSharedRecordingAgain();
            }
        },onError: (error) =>
        {
            CheckSharedRecordingAgain();
        },true);
        
   }
   public void ShowNormalNoticiation(string title, string content)
   {
      var notification = Instantiate(notificationPrefab,notificationPrefab.transform.parent);
      notification.gameObject.SetActive(true);
      notification.NotificationHeading.text = title;
      notification.NotificationBody.text = content;
      notification.NotificationImage.color = ReferenceManager.instance.AppThemeChanger.NormalNotificationColor;
      notificationPrefabs.Add(notification);
      notification.NotificationImage.rectTransform.anchoredPosition = new Vector2(notification.NotificationImage.rectTransform.anchoredPosition.x, 0 - notificationPrefabs.IndexOf(notification));
   }
   public void ShowWarningNoticiation(string title, string content)
   {
      var notification = Instantiate(notificationPrefab,notificationPrefab.transform.parent);
      notification.gameObject.SetActive(true);
      notification.NotificationHeading.text = title;
      notification.NotificationBody.text = content;
      notification.NotificationImage.color = ReferenceManager.instance.AppThemeChanger.WarningNotificationColor;
      notificationPrefabs.Add(notification);
      notification.NotificationImage.rectTransform.anchoredPosition = new Vector2(notification.NotificationImage.rectTransform.anchoredPosition.x, 0 - notificationPrefabs.IndexOf(notification));
   }
   public void ShowErrorNoticiation(string title, string content)
   {
      var notification = Instantiate(notificationPrefab,notificationPrefab.transform.parent);
      notification.gameObject.SetActive(true);
      notification.NotificationHeading.text = title;
      notification.NotificationBody.text = content;
      notification.NotificationImage.color = ReferenceManager.instance.AppThemeChanger.ErrorNotificationColor;
      notificationPrefabs.Add(notification);
      notification.NotificationImage.rectTransform.anchoredPosition = new Vector2(notification.NotificationImage.rectTransform.anchoredPosition.x, 0 - notificationPrefabs.IndexOf(notification));
   }
}
