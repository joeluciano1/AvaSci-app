using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public TMP_Text ReasonLoading_Text;
    public TMP_Text DownloadedBytes;
    public Image LoadingImage;
    public void Show(string value)
    {
        DownloadedBytes.text = "";
        LoadingImage.fillAmount = 0;
        gameObject.SetActive(true);
        ReasonLoading_Text.text = value;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
