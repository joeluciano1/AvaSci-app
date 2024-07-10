using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class OpenCloser : MonoBehaviour
{
    public Transform openCloser;
    public RectTransform myRectTrans;
    bool value;

    private void Start() { }

    public void OpenOrClost()
    {
        value = !value;
        if (value)
        {
            openCloser.DORotate(new Vector3(0, 0, 180), 0.5f);
            myRectTrans.DOSizeDelta(
                new Vector2(myRectTrans.sizeDelta.x, 150),
                StringConstants.ANIMATIONTIME
            );
            ReferenceManager.instance.PauseTheVideo();
        }
        else if (!value)
        {
            openCloser.DORotate(new Vector3(0, 0, 0), 0.5f);
            myRectTrans.DOSizeDelta(
                new Vector2(myRectTrans.sizeDelta.x, 55.807f),
                StringConstants.ANIMATIONTIME
            );
        }
    }
}
