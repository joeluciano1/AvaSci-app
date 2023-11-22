using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LightBuzz.BodyTracking;

public class JointMono : MonoBehaviour
{
    public JointType JointType ;
    public bool Healthy ;
    public bool ExperiencePain ;
    public bool ResultOfInjury ;
    public bool PreviousSurgery ;

    public void SetJointValue(bool value)
    {
        string valueName = EventSystem.current.currentSelectedGameObject.transform.parent.name;

        if(valueName == "Healthy")
        {
            Healthy = value;
        }
        if(valueName == "Experience Pain")
        {
            ExperiencePain = value;
        }
        if(valueName == "Result Of Injury")
        {
            ResultOfInjury = value;
        }
        if(valueName == "Previous Surgery")
        {
            PreviousSurgery = value;
        }
    }
}
