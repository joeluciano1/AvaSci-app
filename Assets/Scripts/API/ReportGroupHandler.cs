using System;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

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
    public ScrollRect MyScrollRect;
    
    public ScrollRect ShowcaseScrollRect;
    public ScrollRect ReportsScrollView;
    public GameObject NoReportNotifier;
    public bool isDropped;
    public Button backButton;

    private Vector2 ReportInitialposition;
    private Vector2 ShowcaseInitialposition;
    private void Start()
    {
        ReportInitialposition = ReportsScrollView.GetComponent<RectTransform>().anchoredPosition;
        ShowcaseInitialposition = ShowcaseScrollRect.GetComponent<RectTransform>().anchoredPosition;
        ContentSizeFitter = Content.GetComponent<ContentSizeFitter>();
    }

    public void ScaleDownItems()
    {
        // DropDownItems.ForEach(x=>x.transform.DOScaleY(0,0.5f));
    }
    public void ToggleDropDown(bool value)
    {
        RectTransform myRect = GetComponent<RectTransform>();
        isDropped = true;
        ContentSizeFitter.enabled = false;
        ContentSizeFitter.SetLayoutVertical(); // can use SetLayoutHorizontal as well
        ForceRebuildLayout();
        ContentSizeFitter.enabled = true;
        // ReportsScrollView.gameObject.SetActive(false);
        ShowcaseScrollRect.gameObject.SetActive(true);
        DropDownItems.ForEach(x=>
        {
            x.transform.parent = ShowcaseScrollRect.content;
            x.MyScrollRect = ShowcaseScrollRect;
        });
        ReportsScrollView.GetComponent<LayoutElement>().ignoreLayout = true;
        ShowcaseScrollRect.GetComponent<LayoutElement>().ignoreLayout = true;
        ReportsScrollView.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-1000, ReportInitialposition.y), 1f).OnComplete(()=>ReportsScrollView.gameObject.SetActive(false));
        ShowcaseScrollRect.GetComponent<RectTransform>().anchoredPosition = new Vector2(1000, ShowcaseInitialposition.y);
        ShowcaseScrollRect.GetComponent<RectTransform>().DOAnchorPos(new Vector2(ShowcaseInitialposition.x, ShowcaseInitialposition.y), 1f).OnComplete(
            () =>
            {
                if (DropDownItems.Contains(ReferenceManager.instance.userReportController.itemToSnapTo) && ReferenceManager.instance.userReportController.itemToSnapTo.gameObject.activeSelf)
                {
                    ReferenceManager.instance.userReportController.SnapToChild(
                        ReferenceManager.instance.userReportController.itemToSnapTo.transform,
                        ReferenceManager.instance.userReportController.itemToSnapTo.MyScrollRect,
                        ReferenceManager.instance.userReportController.itemToSnapTo.MyScrollRect.content);
                }
                ReportsScrollView.GetComponent<LayoutElement>().ignoreLayout = false;
                ShowcaseScrollRect.GetComponent<LayoutElement>().ignoreLayout = false;
            });
            ForceRebuildLayout();
            // DropperToggle.DORotate(new Vector3(0, 0, 180), 0.5f);
           
           

            if (DropDownItems.Count == 0)
            {
                NoReportNotifier.SetActive(true);
            }
            else
            {
                NoReportNotifier.SetActive(false);
            }
            // myRect.DOSizeDelta(new Vector2(myRect.sizeDelta.x, 170), 0.5f);
            // DropDownItems.ForEach(x=>x.transform.DOScaleY(1,0.5f));

        backButton.onClick.AddListener(GoBack);
    }

    private void GoBack()
    {
        if (isDropped)
        {
            isDropped = false;
            DropDownItems.ForEach(x =>
            {
                x.transform.parent = MyScrollRect.content;
                x.MyScrollRect = ReportsScrollView;
            });
            backButton.onClick.RemoveListener(GoBack);
            ReportsScrollView.GetComponent<LayoutElement>().ignoreLayout = true;
            ShowcaseScrollRect.GetComponent<LayoutElement>().ignoreLayout = true;
            ReportsScrollView.gameObject.SetActive(true);
            ReportsScrollView.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-1000, ReportInitialposition.y), 0.0f);
            
            ReportsScrollView.GetComponent<RectTransform>().DOAnchorPos(new Vector2(ReportInitialposition.x, ReportInitialposition.y), 1f);
            ShowcaseScrollRect.GetComponent<RectTransform>().DOAnchorPos(new Vector2(1000, ShowcaseInitialposition.y), 1f).OnComplete(()=>
            {
                ShowcaseScrollRect.gameObject.SetActive(false);
                ReportsScrollView.GetComponent<LayoutElement>().ignoreLayout = false;
                ShowcaseScrollRect.GetComponent<LayoutElement>().ignoreLayout = false;
            });
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
