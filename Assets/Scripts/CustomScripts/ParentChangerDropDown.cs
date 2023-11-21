using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;

public class ParentChangerDropDown : MonoBehaviour
{
    public Transform ObjectToGoUnder;
    public Transform Content;
    public List<CountryModel> countryModels = new List<CountryModel>();
    public UnityEvent ActionToPerformWhenIAppear;

    private async void Start()
    {
        ReferenceManager.instance.CountryParentDropDown = this;
        if (ObjectToGoUnder != null)
        {
            transform.SetSiblingIndex(ObjectToGoUnder.GetSiblingIndex() + 1);
            if(Content!=null)
            ObjectToGoUnder.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener((string value) => SearchCountry(value));
        }
        ActionToPerformWhenIAppear.Invoke();
        await Task.Delay(500);
        
        countryModels = countryModels.OrderBy(o => o.countryName).ToList();
        
        for (int i =0; i<countryModels.Count;i++)
        {
            countryModels[i].transform.SetSiblingIndex(i);
        }
        foreach(var item in countryModels)
        {
            if(item.countryName == "Select Country....")
            {
                item.transform.SetSiblingIndex(0);
            }
            if(item.countryName.Contains("United States"))
            {
                item.transform.SetSiblingIndex(0);
            }
        }
    }

    public void SearchCountry(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            for (int i = 1; i < Content.childCount; i++)
            {
                if(Content.GetChild(i).name == "Item")
                {
                    continue;
                }
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
