using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Samples;

public class TimeBasedReadingFromDB : MonoBehaviour
{
    public TimeBasedReadingInfoSection timeBasedReadingInfoSection;
    public GameObject ReadingValuePrefab;
    public TMP_Text Time;
    public Transform ToggleImage;
    public RectTransform Content;
    public Button CSVButton;
    public Button CreateExcelButton;
    public Toggle SelectAllToggle;
    public List<TimeBasedReadingRequest> timeBasedReadings = new List<TimeBasedReadingRequest>();
    public List<TimeBasedReadingRequest> selectedTimeBasedReadings = new List<TimeBasedReadingRequest>();
    public List<GetGaitReportResponse> gaitReportReadings = new List<GetGaitReportResponse>();
    public GameObject RecordingPanel;
    public TMP_Text RecordedText;
    public StreamingSampleMic streamingSampleMic;
    public void ExpandAndCollapse(bool value)
    {
        if(value)
        {
            timeBasedReadingInfoSection.transform.parent.transform.DOScaleY(1, 0.5f).OnComplete(()=>LayoutRebuilder.ForceRebuildLayoutImmediate(Content));
            ToggleImage.DORotate(new Vector3(0, 0, 180), 0.5f);
        }
        else{
            timeBasedReadingInfoSection.transform.parent.transform.DOScaleY(0, 0.5f).OnComplete(()=>LayoutRebuilder.ForceRebuildLayoutImmediate(Content));;
            ToggleImage.DORotate(new Vector3(0, 0, 0), 0.5f);
        }
        
    }
    public void SelectValue(string propertyName, Toggle toggle)
    {
        if (toggle.isOn)
        {
            timeBasedReadingInfoSection.addedColumns.Add(toggle.gameObject);
        }
        else
        {
            timeBasedReadingInfoSection.addedColumns.Remove(timeBasedReadingInfoSection.addedColumns.FirstOrDefault(x=>x.name == propertyName));
        }
    }
}
