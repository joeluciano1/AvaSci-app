using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragger : MonoBehaviour, IDragHandler
{
    // Start is called before the first frame update
    public bool isDragging;
    public RectTransform _rect;
    private static bool isMoving = false;
    public float offset = 100;

    void Start()
    {
        _rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent.name.Contains("Distance"))
        {
            return;
        }
        if (Input.GetMouseButton(0))
        {
            UpdateMousePosition();
        }
        if (Input.GetMouseButtonDown(0))
        {
            UpdateStartPosition();
            CalculateDifference();
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
                isDragging = false;
        }
    }

    public LineRenderer lineRenderer;

    public void DrawLineWithJoint(RectTransform startPoint, RectTransform endPoint)
    {
        Vector3 startWorldPosition = GetWorldPosition(startPoint);
        Vector3 endWorldPosition = GetWorldPosition(endPoint);
        if (startPoint.name.Contains("Right"))
        {
            startWorldPosition.x += offset;
        }
        if (startPoint.name.Contains("Left"))
        {
            startWorldPosition.x -= offset;
        }
        // Set the positions of the line renderer
        lineRenderer.SetPosition(0, startWorldPosition);
        lineRenderer.SetPosition(1, endWorldPosition);
    }

    private Vector3 GetWorldPosition(RectTransform rectTransform)
    {
        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(
            Camera.main,
            rectTransform.position
        );
        Vector3 worldPosition;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            ReferenceManager.instance.canvas.transform as RectTransform,
            screenPoint,
            ReferenceManager.instance.canvas.worldCamera,
            out worldPosition
        );
        return worldPosition;
    }

    Vector2 mousePosition = new Vector2();
    Vector2 startPosition = new Vector2();
    Vector2 differencePoint = new Vector2();

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        Vector3 targetpos = mousePosition - differencePoint;
        targetpos.z = 0;
        _rect.anchoredPosition = targetpos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    void UpdateMousePosition()
    {
        mousePosition.x = Input.mousePosition.x;
        mousePosition.y = Input.mousePosition.y;
    }

    void UpdateStartPosition()
    {
        startPosition.x = _rect.anchoredPosition.x;
        startPosition.y = _rect.anchoredPosition.y;
    }

    void CalculateDifference()
    {
        differencePoint = mousePosition - startPosition;
    }

    public void OnDrop(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (!isMoving)
        {
            var uiElement1 = GetComponent<RectTransform>();
            var otherElement = other.GetComponent<RectTransform>();
            // Calculate the differences in positions
            float deltaX = otherElement.localPosition.x - transform.localPosition.x;
            float deltaY = otherElement.localPosition.y - transform.localPosition.y;

            // Determine if the collision is more horizontal or vertical
            bool horizontalCollision = Mathf.Abs(deltaX) > Mathf.Abs(deltaY);

            if (horizontalCollision)
            {
                // Move elements along the x-axis
                float midpointX = (transform.localPosition.x + otherElement.localPosition.x) / 2f;
                transform.localPosition = new Vector3(
                    midpointX - 20 / 2f,
                    transform.localPosition.y,
                    transform.localPosition.z
                );
                otherElement.localPosition = new Vector3(
                    midpointX + 20 / 2f,
                    otherElement.localPosition.y,
                    otherElement.localPosition.z
                );
            }
            else
            {
                // Move elements along the y-axis
                float midpointY = (transform.localPosition.y + otherElement.localPosition.y) / 2f;
                transform.localPosition = new Vector3(
                    transform.localPosition.x,
                    midpointY - 20 / 2f,
                    transform.localPosition.z
                );
                otherElement.localPosition = new Vector3(
                    otherElement.localPosition.x,
                    midpointY + 20 / 2f,
                    otherElement.localPosition.z
                );
            }

            // Reset the flag after moving
            isMoving = false;
        }
    }
}
