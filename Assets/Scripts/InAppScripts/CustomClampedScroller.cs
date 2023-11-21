using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public  class CustomClampedScroller : MonoBehaviour
{
    [SerializeField] protected ScrollRect scrollRect;
    [SerializeField] protected RectTransform contentPanel;
    Vector2 TargetPosition;
    public Vector2 offset;

    public void SnapTo(float positionx)
    {
        Canvas.ForceUpdateCanvases();

        TargetPosition = new Vector2(positionx, 0);
       
        TargetPosition += offset;
    }

    private void Update()
    {
        contentPanel.anchoredPosition = Vector2.Lerp(contentPanel.anchoredPosition, TargetPosition,0.5f);
              
    }
}
