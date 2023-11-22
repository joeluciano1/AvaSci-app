using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountryModel : MonoBehaviour
{
    public int Id;
    public string countryName;
    public TMP_Text SelectedCountryNameCaption;
    // Start is called before the first frame update
    void Start()
    {
        Id = transform.GetSiblingIndex()-1;
        countryName = gameObject.name.Replace("Item","").Replace(": ","").Replace(" "+Id.ToString(),"");
        

        ReferenceManager.instance.CountryParentDropDown.countryModels.Add(this);
    }

    public void WhenIAmSelected(bool value)
    {
        
            ReferenceManager.instance.LoginManager.selectedCountryId = Id;
            SelectedCountryNameCaption.text = countryName;
        
    }
    
}
