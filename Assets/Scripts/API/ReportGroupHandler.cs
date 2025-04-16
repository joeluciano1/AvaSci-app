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

    private bool hasSpoken;
    public void ToggleDropDown(bool value)
    {
        ReferenceManager.instance.userReportController.reportSectionManager.SearchReportInputField.placeholder.GetComponent<TMP_Text>().text = "Search " +GroupName.text+"...";
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
            x.transform.SetParent(ShowcaseScrollRect.content,false);
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

                if (!hasSpoken)
                {   
                    ReferenceManager.instance.userReportController.addedReportGroupHandlers.ForEach(x=>x.hasSpoken=true);
                    ReferenceManager.instance.TTSTutorialHandler.NextLine(
                        "You can see the list of reports uploaded by different users. Each item has a red button to download or watch the recording session. If the report contains a pdf in the database a blue icon will also appear next to the watch or download button. You can also see in the top left corner of the screen a toggle to enable research mode of the app to enable or disable more detailed features of readings. Click on the red video icon to record a new video or click on the little arrow icon on top to go back to the previous screen.");
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
