//from video at https://youtu.be/6C1NPy321Nk
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChartAndGraph;
using UnityEngine;
using UnityEngine.UI;

public class SineWave : MonoBehaviour
{

    public GraphChart graphChart;
    public bool drawGraph;
    public CustomChartPointer customChartPointer;
    public float X;
    public Toggle AutoScrollToggle;

    public bool zoomIn;
    public bool zoomOut;
    public void Start()
    {
        
        graphChart.DataSource.ClearCategory("Spine");
        graphChart.DataSource.ClearCategory("Left Elbow");
        
        X = 0;
        _ = DrawGraph();


    }
    public void ToggleGraphReeadging(bool value)
    {
        drawGraph = value;
        if (value)
        {
            _=DrawGraph();
        }
        else
        {
            
        }
    }

    public async Task DrawGraph()
    {
        while (drawGraph)
        {
            if (customChartPointer == null)
            {
                customChartPointer = graphChart.GetComponent<CustomChartPointer>();
            }
            if (customChartPointer.IsMouseDown)
                SetGraphsAutoScroll(false);
            else if (customChartPointer.IsOut && !customChartPointer.IsMouseDown)
                SetGraphsAutoScroll(true);
            graphChart.DataSource.AddPointToCategoryRealtime("Spine", System.DateTime.Now, Random.Range(0, 90), X);

            graphChart.DataSource.AddPointToCategoryRealtime("Left Elbow", System.DateTime.Now, Random.Range(0, 90), X);

            X += 1;
            await Task.Delay(1 * 1000);

        }
        
    }
     public void SetGraphsAutoScroll(bool value)
    {
        graphChart.AutoScrollHorizontally = value;
        graphChart.AutoScrollVertically = value;

        graphChart.DataSource.AutomaticHorizontalView = value;
        graphChart.DataSource.AutomaticVerticallView = value;

        customChartPointer.ToggleAutoScroll(value);
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