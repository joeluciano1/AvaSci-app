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
using DG.Tweening;
public class ReferenceManager : MonoBehaviour
{
    public static ReferenceManager instance;
    public LoginManager LoginManager;
    public LoadingManager LoadingManager;
    public PopupManager PopupManager;
    public ForgetPasswordManager forgetPasswordManager;
    public GameObject SigninPanel;
    public GameObject SignupPanel;
    public ParentChangerDropDown CountryParentDropDown;
    public TMP_Text UsernameText;

    public List<GraphManager> graphManagers = new List<GraphManager>();
    public GraphManager GraphmanagerPrefab;
    public TMP_Dropdown ListOfJointsDropDown;
    public GraphMinimizer GraphMinimizer;
    public Dropdown sensorTypeDropDown;
    public int LidarCount;

    public List<Angle2D> AnglesAdded = new List<Angle2D>();
    /// <summary>
    /// Screens
    /// </summary>
    ///
    public GameObject Screen1;

    public Main LightBuzzMain;

    /// <summary>
    /// settings
    /// </summary>
    public Toggle SettingToggle;
    public RectTransform SettingsPanel;
    public UiManager uiManager;
    private void Awake()
    {
        instance = this;
    }
    public async void SwitchToLidar()
    {
#if !UNITY_EDITOR
if(ReferenceManager.instance.sensorTypeDropDown.value != 1){
        ReferenceManager.instance.LoadingManager.Show("Setting Up Lidar Camera Please Wait...");
        await System.Threading.Tasks.Task.Delay(2000);
        if (LidarCount != 0)
        {
            

                ReferenceManager.instance.sensorTypeDropDown.value = 1;

        
        }
        ReferenceManager.instance.LoadingManager.Hide();
}
#endif

    }
    public void GenerateGraph(MeasurementType measurementType)
    {

        List<string> jointTypes = new List<string>();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        jointTypes = Enum.GetNames(typeof(MeasurementType)).ToList();
        foreach (var item in jointTypes)
        {
            if (Enum.GetName(typeof(MeasurementType), measurementType) == item && !ListOfJointsDropDown.options.Any(x => x.text == item))
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

    public void OpenSettings(bool value)
    {
        if (value)
        {
            SettingToggle.interactable = false;
            SettingToggle.GetComponent<Image>().color = UnityEngine.Color.gray;
            SettingsPanel.DOAnchorPosY(-10000, 0f);
            SettingsPanel.DOAnchorPosY(0, 0.5f).OnComplete(() => SettingToggle.interactable = true);
            SettingsPanel.gameObject.SetActive(true);
        }
        else
        {
            SettingsPanel.DOAnchorPosY(-1000, 0.5f).OnComplete(() =>
            {
                SettingToggle.interactable = true;
                SettingsPanel.gameObject.SetActive(false);
                SettingToggle.GetComponent<Image>().color = UnityEngine.Color.white;

            });
        }
    }

    public void StopAllGraphs()
    {
        graphManagers.ForEach(x => x.MySineWave.isReading = false);
    }

    public void PlayAllGraphs()
    {
        graphManagers.ForEach(x =>
        {
            x.MySineWave.isReading = false;
            x.MySineWave.graphChart.AutoScrollHorizontally = false;
        });
    }

    public void OnVideoScrolled(float value)
    {
        if (graphManagers.Any(x => !x.MySineWave.isReading))
        {

            graphManagers.ForEach(x =>
            {
                x.MySineWave.graphChart.AutoScrollHorizontally = false;
                x.MySineWave.graphChart.HorizontalScrolling = value * 10;
            });

        }
    }
    public void ClearAllGraphs()
    {
        graphManagers.ForEach(x =>
       {
           x.MySineWave.Start();
           x.MySineWave.graphChart.AutoScrollHorizontally = true;
       });
        graphManagers.ForEach(x => x.MySineWave.graphChart.DataSource.Clear());
    }
}
