using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json;

public class CommentQuestionnaire : MonoBehaviour
{
    public TMP_InputField CommentInputField;
    public TMP_Dropdown GroupNameDropDown;
    public TMP_Dropdown PatientsDropDown;
    public Button DoneButton;
    Button PreviousRefButton;
    TMP_Text PreviousRefText;
    Image previousRefImage;
    public TMP_Dropdown SubgroupDropDown;
    public TMP_InputField SubgroupNameInputField;
    public Button MyButton;
    public TMP_Text MyText;
    
    private void OnEnable()
    {
        PreviousRefText = ReferenceManager.instance.StreamingSampleMic.text;
        PreviousRefButton = ReferenceManager.instance.StreamingSampleMic.button;
        previousRefImage = ReferenceManager.instance.StreamingSampleMic.buttonText;
        
        ReferenceManager.instance.StreamingSampleMic.text = MyText;
        ReferenceManager.instance.StreamingSampleMic.button = MyButton;
        ReferenceManager.instance.StreamingSampleMic.buttonText = MyButton.image;
        MyText.text = string.Empty;
        // MyButton.transform.DOScale(new Vector3(1.5f,1.5f,1.5f),1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        APIHandler.instance.Get("UserReport/GetSubgroups", (response) =>
        {
            BaseSubgroupsResponse baseSubgroupsResponse = JsonConvert.DeserializeObject<BaseSubgroupsResponse>(response);
            if (baseSubgroupsResponse.isSuccess)
            {
                List<TMP_Dropdown.OptionData> subgroupOptions = new List<TMP_Dropdown.OptionData>();
                foreach (var group in baseSubgroupsResponse.result.groups)
                {
                    TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
                    option.text = group.SubGroupName;
                    subgroupOptions.Add(option);
                }
    
                SubgroupDropDown.AddOptions(subgroupOptions);
            }
            else if(baseSubgroupsResponse.isError)
            {
                string reasons = "";
                foreach (var item in baseSubgroupsResponse.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }
                ReferenceManager.instance.PopupManager.Show("Getting Subgroups Failed!", $"Reasons are: {reasons}");
            }
        }, onError: error =>
        {
            ReferenceManager.instance.PopupManager.Show("Getting Subgroups Failed!", $"Reasons are: {error}");
        });
    }

    private void OnDisable()
    {
        ReferenceManager.instance.StreamingSampleMic.text = PreviousRefText;
        ReferenceManager.instance.StreamingSampleMic.button = PreviousRefButton;
        ReferenceManager.instance.StreamingSampleMic.buttonText = previousRefImage;
    }

    public void AskToUpload(string path)
    {
        gameObject.SetActive(true);
        CommentInputField.text = string.Empty;
        DoneButton.onClick.RemoveAllListeners();
        DoneButton.onClick.AddListener(() => ReferenceManager.instance.UploadVideo(path));
        DoneButton.onClick.AddListener(() => gameObject.SetActive(false));
    }
}
