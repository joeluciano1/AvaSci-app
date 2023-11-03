using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppThemeChanger : MonoBehaviour
{
    // Start is called before the first frame update
    public Color DarkThemeColor;
    
    public void ToggleTheme(bool useDarkTheme)
    {
        Camera cam = GetComponent<Camera>();
        if (useDarkTheme)
        {
            cam.backgroundColor = DarkThemeColor;
            ReferenceManager.instance.LoadingManager.gameObject.GetComponent<Image>().color = DarkThemeColor;
            ReferenceManager.instance.PopupManager.gameObject.GetComponent<Image>().color = DarkThemeColor;
        }
        else
        {
            cam.backgroundColor = Color.white;
            ReferenceManager.instance.LoadingManager.gameObject.GetComponent<Image>().color = Color.white;
            ReferenceManager.instance.PopupManager.gameObject.GetComponent<Image>().color = Color.white;
        }
    }
}
