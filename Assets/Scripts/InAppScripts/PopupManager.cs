using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Events;
using System;

public class PopupManager : MonoBehaviour
{
    public TMP_Text HeadingText;
    public TMP_Text ContentText;
    [HideInInspector] public bool doFade;
    [HideInInspector] public Image MyImage;
    public UnityEvent onSuccess;
    public Button SampleButton;
    
    public Button NoButton;

    public GameObject Toast;
    public TMP_Text ToastText;
    public async void Show(string heading, string content,string noButtonName = "", System.Action okPressed = null, string yesButtonName = "Yes" )
    {
    
        Debug.Log("Popup Message: " + content);
        gameObject.SetActive(true);
        HeadingText.text = heading;
        ContentText.text = content;
        NoButton.gameObject.SetActive(false);
        SampleButton.onClick.RemoveAllListeners();
        NoButton.onClick.RemoveAllListeners();
        if (okPressed != null)
        {
            // IOSNativeAlert.ShowAlertMessage(heading, content,new IOSNativeAlert.AlertButton("No", null, ButtonStyle.Cancel), new IOSNativeAlert.AlertButton("Yes", () => { okPressed.Invoke(); }));
                SampleButton.GetComponentInChildren<TextMeshProUGUI>().text = yesButtonName;
                SampleButton.onClick.AddListener(()=>okPressed?.Invoke());

                if (!string.IsNullOrEmpty(noButtonName))
                {
                    NoButton.gameObject.SetActive(true);
                    NoButton.GetComponentInChildren<TextMeshProUGUI>().text = noButtonName;


                    NoButton.onClick.AddListener(() => gameObject.SetActive(false));
                }
        }
        else
        {
            // IOSNativeAlert.ShowAlertMessage(heading, content);
            
            SampleButton.GetComponentInChildren<TextMeshProUGUI>().text = "Ok";
            
            SampleButton.onClick.AddListener(()=>gameObject.SetActive(false));
        }

        
    }

    public void ShowToast(string content)
    {
        Toast.SetActive(true);
        ToastText.text = content;
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
