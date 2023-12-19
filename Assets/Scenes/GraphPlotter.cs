using UnityEngine;
using System.Collections.Generic;

public class GraphPlotter : MonoBehaviour
{
    public RectTransform graphContainer; // The parent container for the graph
    public GameObject pointPrefab; // A prefab for the data points
    public LineRenderer lineRenderer; // The LineRenderer component

    // Define the range of your data
    private float minX = 0f;
    private float maxX = 100f; // Set this to your actual max X value
    private float minY = -300f;
    private float maxY = 300f; // Set this to your actual max Y value

    // Example data
    private List<Vector2> dataPoints = new List<Vector2>();
    

    private void Start()
    {
        dataPoints.Add(new Vector2(Random.Range(0, 10), Random.Range(0, 360)));
        dataPoints.Add(new Vector2(Random.Range(0, 10), Random.Range(0, 360)));
        dataPoints.Add(new Vector2(Random.Range(0, 10), Random.Range(0, 360)));
        dataPoints.Add(new Vector2(Random.Range(0, 10), Random.Range(0, 360)));
        dataPoints.Add(new Vector2(Random.Range(0, 10), Random.Range(0, 360)));
        dataPoints.Add(new Vector2(Random.Range(0, 10), Random.Range(0, 360)));
        dataPoints.Add(new Vector2(Random.Range(0, 10), Random.Range(0, 360)));
        PlotGraph();
    }

    private void PlotGraph()
    {
        lineRenderer.positionCount = dataPoints.Count;

        for (int i = 0; i < dataPoints.Count; i++)
        {
            // Map the data point to screen space
            Vector2 position = ConvertToGraphPosition(dataPoints[i]);
            // Instantiate the data point prefab at the position
            Instantiate(pointPrefab, position, Quaternion.identity, graphContainer);

            // Add a point to the LineRenderer
            lineRenderer.SetPosition(i, new Vector3(position.x, position.y, 0));
        }
    }

    private Vector2 ConvertToGraphPosition(Vector2 dataPoint)
    {
        // Convert the data point to a position within the graphContainer
        float xPercentage = (dataPoint.x - minX) / (maxX - minX);
        float yPercentage = (dataPoint.y - minY) / (maxY - minY);

        float xPosition = graphContainer.sizeDelta.x * xPercentage;
        float yPosition = graphContainer.sizeDelta.y * yPercentage;

        return new Vector2(xPosition, yPosition);
    }
}
