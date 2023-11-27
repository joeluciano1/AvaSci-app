using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using LightBuzz.BodyTracking;
using LightBuzz.AvaSci.Measurements;

public class ReferenceManager : MonoBehaviour
{
    public static ReferenceManager instance;
    public LoginManager LoginManager;
    public LoadingManager LoadingManager;
    public PopupManager PopupManager;
    public GameObject SigninPanel;
    public GameObject SignupPanel;
    public ParentChangerDropDown CountryParentDropDown;
    public TMP_Text UsernameText;

    public List<GraphManager> graphManagers = new List<GraphManager>();
    public GraphManager GraphmanagerPrefab;
    public TMP_Dropdown ListOfJointsDropDown;
    /// <summary>
    /// Screens
    /// </summary>
    ///
    public GameObject Screen1;

    private void Awake()
    {
        instance = this;
    }

    public  void GenerateGraph(MeasurementType measurementType)
    {
        
        List<string> jointTypes = new List<string>();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        jointTypes = Enum.GetNames(typeof(MeasurementType)).ToList();
        foreach(var item in jointTypes)
        {
            if (Enum.GetName(typeof(MeasurementType), measurementType) == item && !ListOfJointsDropDown.options.Any(x=>x.text == item))
            {
                GraphManager graphManager = Instantiate(GraphmanagerPrefab, GraphmanagerPrefab.transform.parent);
                graphManager.Title.text = item + " Joint";
                graphManager.JointType = Enum.Parse<MeasurementType>(item);
                graphManagers.Add(graphManager);
                graphManager.name = item;
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData()
                {
                    text = item
                };
                options.Add(optionData);
                ListOfJointsDropDown.onValueChanged.AddListener((int value) => graphManager.EnableMe(value - 1));
                graphManager.MySineWave.Start();
            }
        }
        ListOfJointsDropDown.AddOptions(options);
        
    }

    public void ClearGraphs()
    {
        ListOfJointsDropDown.ClearOptions();
        graphManagers.ForEach(x => Destroy(x.gameObject));
        graphManagers.Clear();

        ListOfJointsDropDown.options.Add(new TMP_Dropdown.OptionData() { text = "Select joint type..." });
    }
}
