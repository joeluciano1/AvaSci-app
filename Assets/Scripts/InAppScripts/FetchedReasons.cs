using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FetchedReasons : MonoBehaviour
{
    public int Id;
    public TMP_Text ItemName;
    public TMP_Text SelectedReasons;



    public void AddOrRemoveMe(bool add)
    {
        if (add)
        {
            if (string.IsNullOrWhiteSpace(SelectedReasons.text))
            {
                SelectedReasons.text = gameObject.name;
            }
            else
            {
                SelectedReasons.text += $",{gameObject.name}";
            }
            ReferenceManager.instance.LoginManager.SelectedFetchedReasons.Add(this);
        }
        else
        {
            if (SelectedReasons.text.Contains(gameObject.name + ","))
            {
                SelectedReasons.text = SelectedReasons.text.Replace(gameObject.name + ",", "");
                ReferenceManager.instance.LoginManager.SelectedFetchedReasons.Remove(this);
            }
            else if (SelectedReasons.text.Contains("," + gameObject.name))
            {
                SelectedReasons.text = SelectedReasons.text.Replace("," + gameObject.name, "");
                ReferenceManager.instance.LoginManager.SelectedFetchedReasons.Remove(this);
            }

            else if (SelectedReasons.text.Contains(gameObject.name))
            {
                SelectedReasons.text = SelectedReasons.text.Replace(gameObject.name, "");
                ReferenceManager.instance.LoginManager.SelectedFetchedReasons.Remove(this);
            }

        }
    }

}
