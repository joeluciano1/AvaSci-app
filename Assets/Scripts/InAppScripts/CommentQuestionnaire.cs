using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
public class CommentQuestionnaire : MonoBehaviour
{
    public TMP_InputField CommentInputField;
    public TMP_Dropdown GroupNameDropDown;
    public TMP_Dropdown PatientsDropDown;
    public Button DoneButton;
    Button PreviousRefButton;
    TMP_Text PreviousRefText;
    Image previousRefImage;

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
