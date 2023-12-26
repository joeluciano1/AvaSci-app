using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    public MeasurementType JointType,SecondJointType;
    public SineWave MySineWave;
    public Text Title;
    public RectTransform MySSRef;
    public Texture2D GraphImage;
    public void EnableMe(int value)
    {
        ReferenceManager.instance.graphManagers.ForEach(x => x.gameObject.SetActive(false));
        var jointGraph = ReferenceManager.instance.graphManagers.FirstOrDefault(x => (int)x.JointType == value);
        if(jointGraph !=null)
        jointGraph.gameObject.SetActive(true);
    }
    
}


