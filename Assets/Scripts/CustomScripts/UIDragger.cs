using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class UIDragger : MonoBehaviour, IDragHandler
{
    // Start is called before the first frame update
    public bool isDragging;
    public RectTransform _rect;
    private static bool isMoving = false;
    public float offset = 100;
    public Vector2 offsetFromJoint;
    public bool isMoved;
    public bool isInside;
    public List<UIDragger> UIDraggersNearMe = new List<UIDragger>();
    void Start()
    {
        _rect = GetComponent<RectTransform>();
        if (!ReferenceManager.instance.UIDraggers.Contains(this))
        {
            ReferenceManager.instance.UIDraggers.Add(this);
        }

        UIDraggersNearMe = ReferenceManager.instance.UIDraggers.Where(x=>x.transform.position == gameObject.transform.position && x !=this && x.transform.parent.transform.parent.name == "AngleManager").ToList();
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
    public void DrawLineWithJointPosition(Vector3 startPoint, Vector3 endPoint)
    {
        
        startPoint.z = 90;
        endPoint.z = 90;
        // Set the positions of the line renderer
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
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

    // private void OnTriggerExit2D(Collider2D other)
    // {   
    //     var draggerInother = other.gameObject.GetComponent<UIDragger>();
    //     if(draggerInother !=null && UIDraggersNearMe.Contains(draggerInother) || UIDraggersNearMe.Count==0)
    //         isInside = false;
    // }

    // private void OnTriggerStay2D(Collider2D other)
    // {
    //     var draggerInother = other.gameObject.GetComponent<UIDragger>();
    //     if (draggerInother != null && UIDraggersNearMe.Contains(draggerInother))
    //     {
    //         isInside = true;
    //         
    //     }
    //     else
    //     {
    //         return;
    //     }
    //     Debug.Log("Staying");
    //     // ReferenceManager.instance.ArrangeNotifiers();
    //     if (!isMoving)
    //     {
    //         var uiElement1 = GetComponent<RectTransform>();
    //         var otherElement = other.GetComponent<RectTransform>();
    //         // Calculate the differences in positions
    //         float deltaX = otherElement.localPosition.x - transform.localPosition.x;
    //         float deltaY = otherElement.localPosition.y - transform.localPosition.y;
    //     
    //         // Determine if the collision is more horizontal or vertical
    //         bool horizontalCollision = Mathf.Abs(deltaX) > Mathf.Abs(deltaY);
    //     
    //         if (horizontalCollision)
    //         {
    //             // Move elements along the x-axis
    //             float midpointX = (transform.localPosition.x + otherElement.localPosition.x) / 2f;
    //             transform.localPosition = new Vector3(
    //                 midpointX - 100 / 2f,
    //                 transform.localPosition.y,
    //                 transform.localPosition.z
    //             );
    //             otherElement.localPosition = new Vector3(
    //                 midpointX + 100 / 2f,
    //                 otherElement.localPosition.y,
    //                 otherElement.localPosition.z
    //             );
    //         }
    //         else
    //         {
    //             // Move elements along the y-axis
    //             float midpointX = (transform.localPosition.x + otherElement.localPosition.x) / 2f;
    //             transform.localPosition = new Vector3(
    //                 midpointX - 100 / 2f,
    //                 transform.localPosition.y,
    //                 transform.localPosition.z
    //             );
    //             otherElement.localPosition = new Vector3(
    //                 midpointX + 100 / 2f,
    //                 otherElement.localPosition.y,
    //                 otherElement.localPosition.z
    //             );
    //             // float midpointY = (transform.localPosition.y + otherElement.localPosition.y) / 2f;
    //             // transform.localPosition = new Vector3(
    //             //     transform.localPosition.x,
    //             //     midpointY - 100 / 2f,
    //             //     transform.localPosition.z
    //             // );
    //             // otherElement.localPosition = new Vector3(
    //             //     otherElement.localPosition.x,
    //             //     midpointY + 100 / 2f,
    //             //     otherElement.localPosition.z
    //             // );
    //         }
    //     
    //         // Reset the flag after moving
    //         isMoving = false;
    //     }
    // }

    private void OnDestroy()
    {
        
        ReferenceManager.instance.UIDraggers.ForEach(x=>x.UIDraggersNearMe.Remove(this));
        ReferenceManager.instance.UIDraggers.Remove(this);
    }
}
