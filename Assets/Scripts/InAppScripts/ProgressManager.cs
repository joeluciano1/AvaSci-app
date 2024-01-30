using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public TMP_Text LoadingInfo;
    public Image LoadingBar;

    public void Show(string info, float progress)
    {
        gameObject.SetActive(true);
        LoadingInfo.text = info;
        LoadingBar.fillAmount = progress;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
