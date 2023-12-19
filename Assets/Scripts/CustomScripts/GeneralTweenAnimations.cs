using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GeneralTweenAnimations : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<RectTransform>().DOAnchorPosX(1000, 0);
        GetComponent<RectTransform>().DOAnchorPosX(0, 1);
    }
    
}
