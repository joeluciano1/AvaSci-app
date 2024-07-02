using ChartAndGraph;
using UnityEngine;

public class PinchDetector : MonoBehaviour
{
    private float initialDistance;
    private Vector2 initialScale;
    public GraphChart graphChart;

    void Update()
    {
#if UNITY_EDITOR
        SimulatePinch();
#else
        DetectPinch();
#endif
    }

    void DetectPinch()
    {
        // Check if there are exactly two touches on the screen
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
                    Debug.Log("Pinch Out");

                    graphChart.HorizontalScrolling = midPoint.x;
                    graphChart.VerticalScrolling = midPoint.y;
                    graphChart.DataSource.HorizontalViewSize -= 0.25f;
                    graphChart.DataSource.VerticalViewSize -= 0.25f;
                }
                else if (scaleFactor < 1)
                {
                    Debug.Log("Pinch In");
                    graphChart.HorizontalScrolling = midPoint.x;
                    graphChart.VerticalScrolling = midPoint.y;
                    graphChart.DataSource.HorizontalViewSize += 0.25f;
                    graphChart.DataSource.VerticalViewSize += 0.25f;
                }
            }
        }
    }

    void SimulatePinch()
    {
        // Check if the right mouse button is held down (simulating touch)
        if (Input.GetMouseButton(1))
        {
            Vector2 mousePosition = Input.mousePosition;

            // Simulate touch positions
            Vector2 simulatedTouch1 = mousePosition + Vector2.left * 50;
            Vector2 simulatedTouch2 = mousePosition + Vector2.right * 50;

            float currentDistance = Vector2.Distance(simulatedTouch1, simulatedTouch2);

            if (Input.GetMouseButtonDown(1))
            {
                // Store the initial distance and initial scale when the "touches" begin
                initialDistance = currentDistance;
                initialScale = transform.localScale;
            }
            else if (Input.GetMouseButton(1))
            {
                if (Mathf.Approximately(initialDistance, 0))
                    return;

                // Calculate the scale factor based on the ratio of the current distance to the initial distance
                float scaleFactor = currentDistance / initialDistance;

                // Apply the scale factor to the object's initial scale
                transform.localScale = initialScale * scaleFactor;

                // Determine if the gesture is pinch in or pinch out
                if (scaleFactor > 1)
                {
                    Debug.Log("Pinch Out");

                    graphChart.DataSource.HorizontalViewSize -= 0.25f;
                    graphChart.DataSource.VerticalViewSize -= 0.25f;
                }
                else if (scaleFactor < 1)
                {
                    Debug.Log("Pinch In");
                    graphChart.DataSource.HorizontalViewSize += 0.25f;
                    graphChart.DataSource.VerticalViewSize += 0.25f;
                }
            }
        }
    }
}
