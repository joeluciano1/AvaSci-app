#define Graph_And_Chart_PRO
using System;
using System.Collections;
using ChartAndGraph;
using UnityEngine;
using UnityEngine.UI;
using static ChartAndGraph.GraphChartBase;

/// <summary>
/// this is an example of zoom using mouse for the graph chart
/// </summary>
[RequireComponent(typeof(GraphChart))]
public class GraphZoom : MonoBehaviour
{
    GraphChart graph;
    Vector3 mZoomBasePosition;
    DoubleVector3 mZoomBaseChartSpace;
    DoubleVector3 InitalScrolling;
    DoubleVector3 InitalViewSize;
    DoubleVector3 InitalViewDirection;
    DoubleVector3 InitialOrigin;
    public float errorMargin = 5f;
    public float ZoomSpeed = 20f;
    public float MaxViewSize = 10f;
    public float MinViewSize = 0.1f;
    float totalZoom = 0;
    private float initialDistance;
    private Vector2 initialScale;

    // Use this for initialization
    void Start() { }

    void OnEnable()
    {
        graph = GetComponent<GraphChart>();
    }

    void ResetZoomAnchor()
    {
        totalZoom = 0;
        InitalScrolling = new DoubleVector3(graph.HorizontalScrolling, graph.VerticalScrolling);
        InitalViewSize = new DoubleVector3(
            graph.DataSource.HorizontalViewSize,
            graph.DataSource.VerticalViewSize
        );
        InitalViewDirection = new DoubleVector3(
            Math.Sign(InitalViewSize.x),
            Math.Sign(InitalViewSize.y)
        );
        InitialOrigin = new DoubleVector3(
            graph.DataSource.HorizontalViewOrigin,
            graph.DataSource.VerticalViewOrigin
        );
    }

    bool CompareWithError(Vector3 a, Vector3 b)
    {
        if (Mathf.Abs(a.x - b.x) > errorMargin)
            return false;
        if (Mathf.Abs(a.y - b.y) > errorMargin)
            return false;
        return true;
    }

    public Vector2 mousePos;
    public Vector2 mousePos2;

    // Update is called once per frame
    void Update()
    {
        if (graph == null) // no graph attached to this script for some reason
            return;

        // mousePos = Input.mousePosition;
        double mouseX,
            mouseY;
        graph.PointToClient(mousePos, out mouseX, out mouseY);
        if (CompareWithError(mousePos, mZoomBasePosition) == false) // the mouse has moved beyond the erroo
        {
            Debug.Log("Happened");
            mZoomBasePosition = mousePos;
            graph.PointToClient(mousePos, out mouseX, out mouseY);
            mZoomBaseChartSpace = new DoubleVector3(mousePos2.x, mousePos2.y);
            ResetZoomAnchor();
        }
        else
        {
            // mousePos = mZoomBasePosition;
        }
        float delta = Input.mouseScrollDelta.y;

        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // Calculate the distance between the two touches in the current frame
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);

            // Check the phase of both touches
            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                // Store the initial distance and initial scale when the touches begin
                initialDistance = currentDistance;
                initialScale = transform.localScale;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                if (Mathf.Approximately(initialDistance, 0))
                    return;

                // Calculate the scale factor based on the ratio of the current distance to the initial distance
                float scaleFactor = currentDistance / initialDistance;

                // Apply the scale factor to the object's initial scale
                // transform.localScale = initialScale * scaleFactor;
                Vector3 midPoint = (touch1.position + touch2.position) / 2;

                // Determine if the gesture is pinch in or pinch out
                if (scaleFactor > 1)
                {
                    delta = -0.25f;
                }
                else if (scaleFactor < 1)
                {
                    delta = 0.25f;
                }
            }
        }
        totalZoom += delta;
        // Debug.Log(delta);
        //accumilate the delta change for the currnet positions

        if (delta != 0 && graph.PointToClient(mousePos, out mouseX, out mouseY))
        {
            graph.DataSource.AutomaticHorizontalView = false;
            graph.DataSource.AutomaticVerticallView = false;
            Debug.Log($"MouseX = {mouseX}, {mousePos2.x}");
            Debug.Log($"MouseY = {mouseY}, {mousePos2.y}");
            DoubleVector3 ViewCenter = InitialOrigin + InitalScrolling;
            DoubleVector3 trans = new DoubleVector3(
                (mZoomBaseChartSpace.x - ViewCenter.x),
                (mZoomBaseChartSpace.y - ViewCenter.y)
            );
            float growFactor = Mathf.Pow(2, totalZoom / ZoomSpeed);
            double hSize = InitalViewSize.x * growFactor;
            double vSize = InitalViewSize.y * growFactor;
            if (
                hSize * InitalViewDirection.x < MaxViewSize
                && hSize * InitalViewDirection.x > MinViewSize
                && vSize * InitalViewDirection.y < MaxViewSize
                && vSize * InitalViewDirection.y > MinViewSize
            )
            {
                graph.HorizontalScrolling = InitalScrolling.x + trans.x - (trans.x * growFactor);
                graph.VerticalScrolling = InitalScrolling.y + trans.y - (trans.y * growFactor);
                graph.DataSource.HorizontalViewSize = hSize;
                graph.DataSource.VerticalViewSize = vSize;

                Debug.Log($"Hsize = {hSize}");
                Debug.Log($"Vsize = {vSize}");
            }
        }
    }

    public void PointClicked(GraphEventArgs arg)
    {
        mousePos = GetComponent<CustomChartPointer>().ScreenPosition;
        // RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //     Canvas,
        //     Input.mousePosition,
        //     Camera.main,
        //     out Vector2 mousePosi
        // );
        // Debug.Log(mousePosi);

        mousePos2 = new Vector2(float.Parse(arg.XString), float.Parse(arg.YString));
    }
}
