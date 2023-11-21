using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    public JointType JointType;
    public SineWave MySineWave;
    public Text Title;

    public void EnableMe(int value)
    {
        ReferenceManager.instance.graphManagers.ForEach(x => x.gameObject.SetActive(false));
        var jointGraph = ReferenceManager.instance.graphManagers.FirstOrDefault(x => (int)x.JointType == value);
        jointGraph.gameObject.SetActive(true);
    }
}


