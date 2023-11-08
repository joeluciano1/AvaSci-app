using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class FetchedInterests : MonoBehaviour
{
    public int Id;
    public TMP_Text ItemName;
    public TMP_Text SelectedInterests;



    public void AddOrRemoveMe(bool add)
    {
        if (add)
        {
            if (string.IsNullOrWhiteSpace(SelectedInterests.text))
            {
                SelectedInterests.text = gameObject.name;
            }
            else
            {
                SelectedInterests.text += $",{gameObject.name}";
            }
            ReferenceManager.instance.LoginManager.SelectedFetchedInterests.Add(this);
        }
        else
        {
            if (SelectedInterests.text.Contains(gameObject.name+","))
            {
                SelectedInterests.text =  SelectedInterests.text.Replace(gameObject.name+",", "");
                ReferenceManager.instance.LoginManager.SelectedFetchedInterests.Remove(this);
            }
            else if (SelectedInterests.text.Contains("," + gameObject.name))
            {
                SelectedInterests.text = SelectedInterests.text.Replace("," + gameObject.name, "");
                ReferenceManager.instance.LoginManager.SelectedFetchedInterests.Remove(this);
            }

            else if (SelectedInterests.text.Contains(gameObject.name))
            {
                SelectedInterests.text = SelectedInterests.text.Replace(gameObject.name, "");
                ReferenceManager.instance.LoginManager.SelectedFetchedInterests.Remove(this);
            }
           
        }
    }
}
