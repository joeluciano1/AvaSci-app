using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationPrefab : MonoBehaviour
{
    public Image NotificationImage;
    public TMP_Text NotificationHeading;
    public TMP_Text NotificationBody;
    
    private void Start()
    {
        StartCoroutine(WaitAndClose());
    }

    IEnumerator WaitAndClose()
    {
        yield return new WaitForSeconds(3);
        ReferenceManager.instance.NotificationManager.notificationPrefabs.Remove(this);
        Destroy(gameObject);
    }

    public void CloseMe()
    {
        gameObject.SetActive(false);
    }
}
