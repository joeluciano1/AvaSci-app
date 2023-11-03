using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ParentChangerDropDown : MonoBehaviour
{
    public Transform ObjectToGoUnder;
    public Transform Content;

    private void Start()
    {
        if (ObjectToGoUnder != null)
        {
            transform.SetSiblingIndex(ObjectToGoUnder.GetSiblingIndex() + 1);
            if(Content!=null)
            ObjectToGoUnder.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener((string value) => SearchCountry(value));
        }
    }

    public void SearchCountry(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            for (int i = 1; i < Content.childCount; i++)
            {
                if (Content.GetChild(i).name.Contains(value,System.StringComparison.OrdinalIgnoreCase))
                {
                    Content.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    Content.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        else
        {
            for(int i = 1; i < Content.childCount; i++)
            {
                Content.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    public void OnCountrySelected()
    {
        ObjectToGoUnder.GetComponentInChildren<TMP_InputField>().text = string.Empty;
    }
}
