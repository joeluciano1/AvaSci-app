using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CommentQuestionnaire : MonoBehaviour
{
    public TMP_InputField CommentInputField;
    public TMP_InputField PatientIdInputField;
    public Button DoneButton;

    public void AskToUpload(string path)
    {
        gameObject.SetActive(true);
        CommentInputField.text = string.Empty;
        DoneButton.onClick.RemoveAllListeners();
        DoneButton.onClick.AddListener(() => ReferenceManager.instance.UploadVideo(path));
        DoneButton.onClick.AddListener(() => gameObject.SetActive(false));
    }
}
