using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using LightBuzz.BodyTracking;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.AvaSci;
using LightBuzz.AvaSci.UI;
using UnityEngine.UI;

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
    public GraphMinimizer GraphMinimizer;
    public Dropdown sensorTypeDropDown;
    /// <summary>
    /// Screens
    /// </summary>
    ///
    public GameObject Screen1;

    public Main LightBuzzMain;

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
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData()
                {
                    text = item
                };
                options.Add(optionData);
            }
        }
        ListOfJointsDropDown.AddOptions(options);
        
    }

    public void ClearGraphs()
    {
        ListOfJointsDropDown.ClearOptions();
        graphManagers.ForEach(x => Destroy(x.gameObject));
        graphManagers.Clear();

        ListOfJointsDropDown.options.Add(new TMP_Dropdown.OptionData() { text = "Select graph type..." });
    }

    public float secondsSpan;

    [ContextMenu("Test TimeSpan")]
    public void Chalao()
    {
        StartCoroutine(TestRoutine());
    }

    IEnumerator TestRoutine()
    {
        DateTime abhikawakt = DateTime.Now;
        yield return new WaitForSeconds(5);
        TimeSpan span = DateTime.Now - abhikawakt;
        secondsSpan = MathF.Round((float)span.TotalSeconds,2);
    }
}
