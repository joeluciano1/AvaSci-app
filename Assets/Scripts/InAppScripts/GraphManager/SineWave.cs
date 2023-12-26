using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChartAndGraph;
using LightBuzz.AvaSci.Measurements;
using UnityEngine;
using UnityEngine.UI;


public class SineWave : MonoBehaviour
{

    public GraphChart graphChart;
    public bool drawGraph;
    public CustomChartPointer customChartPointer;
    
    public Toggle AutoScrollToggle;

    public Material lineMaterial;
    public double lineThickness;
    public MaterialTiling lineTiling;
    public Material innerFill;
    public Material pointMaterial;
    public double pointSize;
    public List<Color> Colors = new List<Color>();
    public float X;
    public bool zoomIn;
    public bool zoomOut;

    public GraphManager myParentGraphManager;
    GraphManager myLinearGraph;

    #region Duplicate graph if circular is added to add linear graph to it
    //public async void Start()
    //{
    //    myParentGraphManager.Title.text += " Circular";
    //    await System.Threading.Tasks.Task.Delay(1000);
    //    if (myParentGraphManager != null)
    //    {
    //        myLinearGraph = Instantiate(myParentGraphManager, myParentGraphManager.transform.parent);
    //        myLinearGraph.Title.text += " Linear";
    //        myLinearGraph.MySineWave.myParentGraphManager = null;
    //    }
    //}
    #endregion

    //private void OnDestroy()
    //{
    //    Destroy(myLinearGraph.gameObject);
    //}
    public void ToggleGraphReeadging(bool value)
    {
        drawGraph = value;
        if (value)
        {
            
        }
        else
        {
            
        }
    }

    public GameObject Body;

    public void Start()
    {
        StartCoroutine(StartReadingGraphs());
    }

    public IEnumerator StartReadingGraphs()
    {

        while (true)
        {
            if (customChartPointer == null)
            {
                customChartPointer = graphChart.GetComponent<CustomChartPointer>();
            }
            if (customChartPointer.IsMouseDown)
                SetGraphsAutoScroll(false);
            else if (customChartPointer.IsOut && !customChartPointer.IsMouseDown)
                SetGraphsAutoScroll(true);

            var main = ReferenceManager.instance.LightBuzzMain;

            //if (myParentGraphManager == null||myLinearGraph==null)
            //{
            //    return;
            //}
            foreach (var item in main._movement.Measurements)
            {
                if (item.Key != myParentGraphManager.JointType)
                {
                    if (item.Key != myParentGraphManager.SecondJointType)
                    {
                        continue;
                    }
                }
                float previousAngle;
                DateTime previousTime;
                string itemName = System.Enum.GetName(typeof(MeasurementType), item.Key);
                previousTime = string.IsNullOrWhiteSpace(PlayerPrefs.GetString(itemName + " previousTime")) ? DateTime.Now : DateTime.Parse(PlayerPrefs.GetString(System.Enum.GetName(typeof(MeasurementType), item.Key) + " previousTime"));
                TimeSpan timeSpan = DateTime.Now - previousTime;
                

                //previousAngle = PlayerPrefs.GetFloat(System.Enum.GetName(typeof(MeasurementType), item.Key) + " PreviousAngle");
                if (!graphChart.DataSource.HasCategory(itemName))
                {
                    Material lineMaterial = new Material(this.lineMaterial);
                    lineMaterial.SetColor("_Color", Colors[UnityEngine.Random.Range(0, Colors.Count)]);
                    string linecolor =ColorUtility.ToHtmlStringRGBA(lineMaterial.color);
                    if(item.Key == myParentGraphManager.JointType)
                        myParentGraphManager.Title.text = $"<color=#{linecolor}>{itemName}</color>";
                    else
                        myParentGraphManager.Title.text += $"\n<color=#{linecolor}>{itemName}</color>";
                    innerFill.color = UnityEngine.Random.ColorHSV();
                    Material pointMat = new Material(pointMaterial);

                    //pointMat.SetColor("_Combine", Colors[UnityEngine.Random.Range(0, Colors.Count)]);
                    pointMat.SetColor("_ColorFrom", Colors[UnityEngine.Random.Range(0, Colors.Count)]);
                    pointMat.SetColor("_ColorTo", Colors[UnityEngine.Random.Range(0, Colors.Count)]);
                    Material innFillMat = new Material(innerFill);
                    var color1 = Colors[UnityEngine.Random.Range(0, Colors.Count)];
                    color1.a = 0.5f;
                    innFillMat.SetColor("_ColorFrom", color1);
                    var color2 = Colors[UnityEngine.Random.Range(0, Colors.Count)];
                    color2.a = 0.1f;
                    innFillMat.SetColor("_ColorTo", color2);
                    graphChart.DataSource.AddCategory(itemName, lineMaterial, lineThickness, lineTiling, null, true, pointMat, pointSize);
                }

                //double angularSpeedY = (GeneralStaticManager.DegreesToRadians(item.Value.Value) - GeneralStaticManager.DegreesToRadians(previousAngle)) / (timeSpan.TotalSeconds);
                //double changeInangleX = (GeneralStaticManager.DegreesToRadians(item.Value.Value) - GeneralStaticManager.DegreesToRadians(previousAngle));

                //graphChart.DataSource.AddPointToCategory(System.Enum.GetName(typeof(MeasurementType), item.Key), changeInangleX, MathF.Round((float)angularSpeedY, 2));
                int value = PlayerPrefs.GetInt(itemName);
                graphChart.DataSource.AddPointToCategoryRealtime(itemName, DateTime.Now, item.Value.Value,1);

                if (GeneralStaticManager.GraphsReadings.ContainsKey(itemName))
                {
                    GeneralStaticManager.GraphsReadings[itemName].Add(item.Value.Value);
                }
                else
                {
                    GeneralStaticManager.GraphsReadings.Add(itemName, new List<float> { item.Value.Value});
                }

                //PlayerPrefs.SetFloat(System.Enum.GetName(typeof(MeasurementType), item.Key) + " PreviousAngle", item.Value.Value);
                //PlayerPrefs.SetString(System.Enum.GetName(typeof(MeasurementType), item.Key) + " previousTime", DateTime.Now.ToString());

                PlayerPrefs.SetInt(itemName, PlayerPrefs.GetInt(itemName) + 1);
            }
            yield return new WaitForSeconds(1);
            X += 1;
        }
    }
    

     public void SetGraphsAutoScroll(bool value)
    {
        //graphChart.AutoScrollHorizontally = value;
        //graphChart.AutoScrollVertically = value;

        //graphChart.DataSource.AutomaticHorizontalView = value;
        //graphChart.DataSource.AutomaticVerticallView = value;

        //customChartPointer.ToggleAutoScroll(value);
    }
    [ContextMenu("Zoom Up")]
    public void ZoomUp(bool value)
    {
        zoomInOutValue = 0.1f;
        zoomOut = value;
        
        AutoScrollToggle.isOn = value;
    }
    [ContextMenu("Zoom Down")]
    public void ZoomDown(bool value)
    {
        zoomInOutValue = 0.1f;
        zoomIn = value;
        
        AutoScrollToggle.isOn = value;
    }
    float zoomInOutValue;
    private void Update()
    {
        if (zoomIn)
        {
            graphChart.DataSource.HorizontalViewSize -= 0.1f;
            graphChart.DataSource.VerticalViewSize -= 0.1f;
            SetGraphsAutoScroll(false);
            zoomInOutValue += 0.1f;
        }
       else if (zoomOut)
        {
            graphChart.DataSource.HorizontalViewSize += 0.1f;
            graphChart.DataSource.VerticalViewSize += 0.1f;
            SetGraphsAutoScroll(false);
            zoomInOutValue += 0.1f;
        }
    }
}