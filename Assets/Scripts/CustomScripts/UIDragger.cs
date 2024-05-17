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

    void Start()
    {
        _rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
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
private void OnTriggerEnter2D(Collider2D other) {
    Debug.Log("Got You");
    if(!isMoving){
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
            transform.localPosition = new Vector3(midpointX - 20 / 2f, transform.localPosition.y, transform.localPosition.z);
            otherElement.localPosition = new Vector3(midpointX + 20 / 2f, otherElement.localPosition.y, otherElement.localPosition.z);
        }
        else
        {
            // Move elements along the y-axis
            float midpointY = (transform.localPosition.y + otherElement.localPosition.y) / 2f;
            transform.localPosition = new Vector3(transform.localPosition.x, midpointY - 20 / 2f, transform.localPosition.z);
            otherElement.localPosition = new Vector3(otherElement.localPosition.x, midpointY + 20 / 2f, otherElement.localPosition.z);
        }

        // Reset the flag after moving
        isMoving = false;
    }
}
    
}
