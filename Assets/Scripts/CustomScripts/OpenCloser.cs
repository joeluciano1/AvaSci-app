using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class OpenCloser : MonoBehaviour
{
    public Transform openCloser;
    RectTransform myRectTrans;
    bool value;

    private void Start()
    {
        myRectTrans = GetComponent<RectTransform>();
        value = true;
    }

    public void OpenOrClost()
    {
        value = !value;
        if (value)
        {
            openCloser.DORotate(new Vector3(0, 0, 180), 1f);
            myRectTrans.DOAnchorPos(
                new Vector2(
                    myRectTrans.anchoredPosition.x - myRectTrans.sizeDelta.x,
                    myRectTrans.anchoredPosition.y
                ),
                1f
            );
        }
        else if (!value)
        {
            openCloser.DORotate(new Vector3(0, 0, 0), 1f);
            myRectTrans.DOAnchorPos(
                new Vector2(
                    myRectTrans.anchoredPosition.x + myRectTrans.sizeDelta.x,
                    myRectTrans.anchoredPosition.y
                ),
                1f
            );
        }
    }
}
