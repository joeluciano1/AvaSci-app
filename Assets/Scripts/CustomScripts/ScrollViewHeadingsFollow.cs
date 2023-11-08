using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollViewHeadingsFollow : MonoBehaviour
{
    public RectTransform ToFollow;
    public Vector2 offset;

    private void Update()
    {
        var myAnchor = GetComponent<RectTransform>();
        myAnchor.anchoredPosition = new Vector2(ToFollow.anchoredPosition.x, myAnchor.anchoredPosition.y)+offset;
    }
}
