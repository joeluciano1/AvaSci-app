using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public TMP_Text ReasonLoading_Text;
    public TMP_Text DownloadedBytes;
    public Image LoadingImage;
    public Image closeButton;
    public void Show(string value)
    {
        closeButton.DOFade(0, 0);
        DownloadedBytes.text = "";
        LoadingImage.fillAmount = 0;
        gameObject.SetActive(true);
        ReasonLoading_Text.text = value;
        StartCoroutine(StartCountdown());
    }
    IEnumerator StartCountdown()
    {
        yield return new WaitForSeconds(5);
        ShowCloseButton();
    }
    public void ShowCloseButton()
    {
        closeButton.DOFade(1, 2);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
