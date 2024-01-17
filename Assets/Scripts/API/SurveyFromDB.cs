using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SurveyFromDB : MonoBehaviour
{
    public Text Username;
    public Text Reason;
    public Text Interest;
    public TMP_Text Advanced;

    public Toggle AdvancedToggle;
    void Start()
    {
        if (string.IsNullOrEmpty(Advanced.text))
        {
            AdvancedToggle.interactable = false;
            AdvancedToggle.GetComponent<TMP_Text>().text = "No Data";
        }
    }
    public void AnimateText(bool value)
    {

        var xValue = Advanced.GetComponent<RectTransform>().sizeDelta.x;
        if (value)
            Advanced.GetComponent<RectTransform>().DOSizeDelta(new Vector2(xValue, 100), 1f);
        else
            Advanced.GetComponent<RectTransform>().DOSizeDelta(new Vector2(xValue, 0), 1f);
    }
}
