using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DynamicSnappingScrollRect : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform center; // This should be the point in the viewport to align items with

    private List<float> itemPositions = new List<float>();
    private bool dragging = false;
    private float nearestPosition;
    private float scrollVelocity;
    private RectTransform contentPanel;

    void Start()
    {
        contentPanel = scrollRect.content;
        UpdateItemPositions(); // Initial update for any starting items
    }

    void Update()
    {
        if (!dragging)
        {
            // If we're not dragging, we want the scrollRect to move to the nearest item position
            scrollVelocity = Mathf.Lerp(scrollVelocity, 0, Time.deltaTime * 10f); // Slow down the scroll speed
            Vector2 newPosition = Vector2.Lerp(contentPanel.anchoredPosition, new Vector2(contentPanel.anchoredPosition.x, nearestPosition), Time.deltaTime * 10f);
            contentPanel.anchoredPosition = newPosition;
        }
        else
        {
            scrollVelocity = scrollRect.velocity.y;
        }
    }

    public void UpdateItemPositions()
    {
        itemPositions.Clear();
        for (int i = 0; i < contentPanel.childCount; i++)
        {
            // Assuming a vertical scroll view, modify if horizontal
            itemPositions.Add(-contentPanel.GetChild(i).GetComponent<RectTransform>().anchoredPosition.y);
        }
    }

    public void OnDragEnd()
    {
        dragging = false;
        float closestDistance = float.MaxValue;
        foreach (var itemPos in itemPositions)
        {
            float distance = Mathf.Abs(contentPanel.anchoredPosition.y - itemPos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestPosition = itemPos;
            }
        }
    }

    public void OnDragStart()
    {
        dragging = true;
    }

    public void ContentUpdated()
    {
        UpdateItemPositions();
        // Call this method after adding or removing items from the ScrollView
    }
}
