using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Events;
using System;
using Nrjwolf.Tools;

public class PopupManager : MonoBehaviour
{
    public TMP_Text HeadingText;
    public TMP_Text ContentText;
    [HideInInspector] public bool doFade;
    [HideInInspector] public Image MyImage;
    public UnityEvent onSuccess;

    public GameObject NoButton;
    public async void Show(string heading, string content, bool fade = false, System.Action okPressed = null, bool isAsking = false)
    {

        if (okPressed != null)
        {
            IOSNativeAlert.ShowAlertMessage(heading, content, new IOSNativeAlert.AlertButton("ok", () => { okPressed.Invoke(); }));

        }
        else
        {
            IOSNativeAlert.ShowAlertMessage(heading, content);
        }

    }

    public void OnOkClick()
    {

        onSuccess?.Invoke();

        Destroy(gameObject);
    }

    public void OnCancel()
    {
        Destroy(gameObject);
    }
}
