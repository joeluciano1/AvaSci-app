using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class JointToggleAnimations : MonoBehaviour
{
    float InitialYScale;
    float scaleDifference;
    public float ExpandedYScale;

    public RectTransform ParentToggleRect;

    public List<GameObject> ToggleChilds = new List<GameObject>();
    public RectTransform Content;

    private void Start()
    {
        InitialYScale = ParentToggleRect.rect.height;
        scaleDifference = ExpandedYScale - InitialYScale;
    }

    public void DoExpand(bool value)
    {
        if (value)
        {
            ParentToggleRect.DOSizeDelta(new Vector2(ParentToggleRect.rect.width, ExpandedYScale), 1);
            ToggleChilds.ForEach(x => x.SetActive(true));
            Content.sizeDelta += new Vector2(0, scaleDifference);
        }
        else
        {
            ParentToggleRect.DOSizeDelta(new Vector2(ParentToggleRect.rect.width, InitialYScale), 1);
            ToggleChilds.ForEach(x => x.SetActive(false));
            Content.sizeDelta -= new Vector2(0, scaleDifference);
        }
    }
}
