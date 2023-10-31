using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        }
        else
        {
            cam.backgroundColor = Color.white;
        }
    }
}
