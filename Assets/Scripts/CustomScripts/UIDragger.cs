using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragger : MonoBehaviour, IDragHandler
{
    // Start is called before the first frame update
    public bool isDragging;
    public RectTransform _rect;
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
}
