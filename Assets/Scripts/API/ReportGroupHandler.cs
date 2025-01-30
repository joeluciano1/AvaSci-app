using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReportGroupHandler : MonoBehaviour
{
    public ContentSizeFitter ContentSizeFitter;
    public Transform DropperToggle;
    public RectTransform Content;
    public List<UserReportFromDB> DropDownItems;
    public TMP_Text GroupName;
    public RectTransform MyContent;
    public RectTransform MyScrollView;
    public VerticalLayoutGroup MyVerticalLayoutGroup;
    public bool isDropped;
    private void Start()
    {
        ContentSizeFitter = Content.GetComponent<ContentSizeFitter>();
    }

    public void ScaleDownItems()
    {
        // DropDownItems.ForEach(x=>x.transform.DOScaleY(0,0.5f));
    }
    public void ToggleDropDown(bool value)
    {
        RectTransform myRect = GetComponent<RectTransform>();
        isDropped = value;
        ContentSizeFitter.enabled = false;
        ContentSizeFitter.SetLayoutVertical(); // can use SetLayoutHorizontal as well
        ForceRebuildLayout();
        ContentSizeFitter.enabled = true;
        if (value)
        {
            ForceRebuildLayout();
            DropperToggle.DORotate(new Vector3(0, 0, 180), 0.5f);
            MyScrollView.DOSizeDelta(new Vector2(MyScrollView.sizeDelta.x, 600), 0.5f).OnComplete(() =>
            {
                if (DropDownItems.Contains(ReferenceManager.instance.userReportController.itemToSnapTo) && ReferenceManager.instance.userReportController.itemToSnapTo.gameObject.activeSelf)
                {
                    ReferenceManager.instance.userReportController.SnapToChild(
                        ReferenceManager.instance.userReportController.itemToSnapTo.transform,
                        ReferenceManager.instance.userReportController.itemToSnapTo.MyScrollRect,
                        ReferenceManager.instance.userReportController.itemToSnapTo.MyScrollRect.content);
                }
            });
            
            // myRect.DOSizeDelta(new Vector2(myRect.sizeDelta.x, 170), 0.5f);
            // DropDownItems.ForEach(x=>x.transform.DOScaleY(1,0.5f));

        }
        else
        {
            ForceRebuildLayout();
            DropperToggle.DORotate(new Vector3(0, 0, 0), 0.5f);
            MyScrollView.DOSizeDelta(new Vector2(MyScrollView.sizeDelta.x, 0), 0.5f);
            // myRect.DOSizeDelta(new Vector2(myRect.sizeDelta.x, 60), 0.5f);
            // DropDownItems.ForEach(x=>x.transform.DOScaleY(0,0.5f));
            
        }
    }

    public void ForceRebuildLayout()
    {
        // Content.gameObject.SetActive(false);
        // Content.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
        LayoutRebuilder.ForceRebuildLayoutImmediate(MyContent);
    }
}
