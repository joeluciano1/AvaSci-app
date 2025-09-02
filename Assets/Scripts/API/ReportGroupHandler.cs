using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
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
    public List<SubGroupsUnderRGH> SubGroups = new List<SubGroupsUnderRGH>();
    public SubGroupsUnderRGH SubgroupPrefab;
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
        if (ShowcaseScrollRect.content.gameObject.GetComponent<GridLayoutGroup>() == null)
        {
            DestroyImmediate(ShowcaseScrollRect.content.gameObject.GetComponent<VerticalLayoutGroup>());
            var GLG = ShowcaseScrollRect.content.gameObject.AddComponent<GridLayoutGroup>();
            GLG.cellSize = new Vector2(100, 100);
            GLG.spacing = new Vector2(20, 20);
            GLG.childAlignment = TextAnchor.UpperCenter;
            GLG.padding.top = 10;
            DropDownItems.ForEach(x=>x.gameObject.SetActive(false));
            ReferenceManager.instance.reportSectionManager.SearchReportInputField.gameObject.SetActive(false);
            ReferenceManager.instance.reportSectionManager.SortByDropdown.gameObject.SetActive(false);
        }

        DropDownItems.ForEach(x=>
        {
            x.transform.SetParent(ShowcaseScrollRect.content,false);
            x.MyScrollRect = ShowcaseScrollRect;
        });
        SubGroups.ForEach(x =>
        {
            x.transform.SetParent(ShowcaseScrollRect.content,false);
            x.gameObject.SetActive(true);
        });
        if (DropDownItems.Count == 0)
        {
            NoReportNotifier.SetActive(true);
        }
        else
        {
            NoReportNotifier.SetActive(false);
        }
        ReportsScrollView.GetComponent<LayoutElement>().ignoreLayout = true;
        ShowcaseScrollRect.GetComponent<LayoutElement>().ignoreLayout = true;
        ReportsScrollView.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-1000, ReportInitialposition.y), 1f).OnComplete(()=>ReportsScrollView.gameObject.SetActive(false));
        ShowcaseScrollRect.GetComponent<RectTransform>().anchoredPosition = new Vector2(1000, ShowcaseInitialposition.y);
        ShowcaseScrollRect.GetComponent<RectTransform>().DOAnchorPos(new Vector2(ShowcaseInitialposition.x, ShowcaseInitialposition.y), 1f).OnComplete(
            () =>
            {
               
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
            backButton.onClick.AddListener(GoBack);
            // DropperToggle.DORotate(new Vector3(0, 0, 180), 0.5f);
    }

    public void AfterSubgroupIsClicked(string selectedSubgroup)
    {
        ReferenceManager.instance.userReportController.CurrentSelectedSubGroup = selectedSubgroup;
        DestroyImmediate(ShowcaseScrollRect.content.GetComponent<GridLayoutGroup>());
        var VLG = ShowcaseScrollRect.content.AddComponent<VerticalLayoutGroup>();
        VLG.spacing = 16;
        VLG.padding.left = 10;
        VLG.padding.right = 10;
        VLG.reverseArrangement = true;
        VLG.childControlHeight = false;
        VLG.childForceExpandHeight = false;
        MyVerticalLayoutGroup = VLG;
        ReferenceManager.instance.userReportController.addedReportGroupHandlers.ForEach(x=>x.MyVerticalLayoutGroup = VLG);
        ReferenceManager.instance.reportSectionManager.SearchReportInputField.gameObject.SetActive(true);
        ReferenceManager.instance.reportSectionManager.SortByDropdown.gameObject.SetActive(true);
        SubGroups.ForEach(x=>x.gameObject.SetActive(false));
        DropDownItems.Where(x=>x.mySubGroup.Contains(selectedSubgroup)).ToList().ForEach(x=>x.gameObject.SetActive(true));
        if (DropDownItems.Contains(ReferenceManager.instance.userReportController.itemToSnapTo) && ReferenceManager.instance.userReportController.itemToSnapTo.gameObject.activeSelf)
        {
            ReferenceManager.instance.userReportController.SnapToChild(
                ReferenceManager.instance.userReportController.itemToSnapTo.transform,
                ReferenceManager.instance.userReportController.itemToSnapTo.MyScrollRect,
                ReferenceManager.instance.userReportController.itemToSnapTo.MyScrollRect.content);
        }
       
        // myRect.DOSizeDelta(new Vector2(myRect.sizeDelta.x, 170), 0.5f);
        // DropDownItems.ForEach(x=>x.transform.DOScaleY(1,0.5f));
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() =>
        {
            DestroyImmediate(ShowcaseScrollRect.content.gameObject.GetComponent<VerticalLayoutGroup>());
            var GLG = ShowcaseScrollRect.content.gameObject.AddComponent<GridLayoutGroup>();
            GLG.cellSize = new Vector2(100,100);
            GLG.spacing = new Vector2(20, 20);
            GLG.childAlignment = TextAnchor.UpperCenter;
            GLG.padding.top = 10;
            SubGroups.ForEach(x=>x.gameObject.SetActive(true));
            DropDownItems.ForEach(x=>x.gameObject.SetActive(false));
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoBack);
            ReferenceManager.instance.reportSectionManager.SearchReportInputField.gameObject.SetActive(false);
            ReferenceManager.instance.reportSectionManager.SortByDropdown.gameObject.SetActive(false);
            DropDownItems.Where(x=>x.mySubGroup.Contains(selectedSubgroup)).ToList().ForEach(x=>x.gameObject.SetActive(false));
        });
        
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
            SubGroups.ForEach(x =>
            {
                x.transform.SetParent(MyScrollRect.content,false);
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
