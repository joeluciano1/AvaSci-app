using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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
            ParentToggleRect
                .DOSizeDelta(
                    new Vector2(ParentToggleRect.rect.width, ExpandedYScale),
                    StringConstants.ANIMATIONTIME
                )
                .OnComplete(
                    () =>
                        ToggleChilds.ForEach(x =>
                        {
                            x.SetActive(true);
                            x.GetComponent<Toggle>().isOn = true;
                        })
                );

            Content.sizeDelta += new Vector2(0, scaleDifference);
        }
        else
        {
            ParentToggleRect.DOSizeDelta(
                new Vector2(ParentToggleRect.rect.width, InitialYScale),
                StringConstants.ANIMATIONTIME
            );
            ToggleChilds.ForEach(x =>
            {
                x.SetActive(false);
                x.GetComponent<Toggle>().isOn = false;
            });
            Content.sizeDelta -= new Vector2(0, scaleDifference);
        }
    }
}
