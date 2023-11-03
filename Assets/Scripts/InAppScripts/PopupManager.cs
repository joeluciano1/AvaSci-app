using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PopupManager : MonoBehaviour
{
    public TMP_Text HeadingText;
    public TMP_Text ContentText;

    public void Show(string heading, string content)
    {
        gameObject.SetActive(true);
        HeadingText.text = heading;
        ContentText.text = content;
    }
}
