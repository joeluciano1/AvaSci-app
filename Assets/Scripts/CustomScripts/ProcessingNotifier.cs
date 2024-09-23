using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProcessingNotifier : MonoBehaviour
{
    public TMP_Text NotifierText;
    public Image LoadingFill;
    public GameObject buttons;
    
    public void CloseMe()
    {
        gameObject.SetActive(false);
        LoadingFill.transform.parent.gameObject.SetActive(true);
        buttons.SetActive(false);
        NotifierText.text = "";
    }
}
