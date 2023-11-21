using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountryModel : MonoBehaviour
{
    public int Id;
    public string countryName;
    // Start is called before the first frame update
    void Start()
    {
        Id = transform.GetSiblingIndex()-1;
        countryName = gameObject.name[8..];
        if (countryName[0] == ' ')
        {
            countryName = countryName[1..];
        }

        ReferenceManager.instance.CountryParentDropDown.countryModels.Add(this);
    }

    public void WhenIAmSelected(bool value)
    {
        if (value)
        {
            ReferenceManager.instance.LoginManager.selectedCountryId = Id;
            ReferenceManager.instance.LoginManager.Country_DropDown.captionText.text = countryName;
        }
    }
    
}
