using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public TMP_Text ReasonLoading_Text;
    public void Show(string value)
    {
        gameObject.SetActive(true);
        ReasonLoading_Text.text = value;
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
