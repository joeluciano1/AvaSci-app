using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AppThemeChanger : MonoBehaviour
{
    // Start is called before the first frame update
    public Color DarkThemeColor;
    public Color LightThemeColor;
    public Color SelectedColor;
    public Color SecondaryButtonColor;
    [Space(20)]
    public Color DarkThemeColorNew;
    public Color LightThemeColorNew;

    public Color NormalNotificationColor;
    public Color WarningNotificationColor;
    public Color ErrorNotificationColor;
    
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

    public List<Image> objects=new List<Image>();

    [ContextMenu("Change Colors")]
    public void ChangeTheme()
    {
        objects = FindObjectsOfType<Image>(true).ToList();
        Debug.Log(objects.Count);
        foreach (var image in objects)
        {
            if (image.color.ToHexString().Equals(DarkThemeColor.ToHexString()) )
            {
                Debug.Log($"Changing {image.name}");
                image.color = DarkThemeColorNew;
            }
            if (image.color.ToHexString().Equals(LightThemeColor.ToHexString()))
            {
                Debug.Log($"Changing {image.name}");
                image.color = LightThemeColorNew;
            }
        }
    }
}
