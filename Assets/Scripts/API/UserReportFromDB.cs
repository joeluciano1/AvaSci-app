using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserReportFromDB : MonoBehaviour
{
    public TMP_Text UserName;
    public TMP_Text CreatedOn;

    public Button WatchBtn;

    public Image ProgressImage;
    public UnityWebRequest request;

    public TMP_Text ButtonText;
    [HideInInspector] public string VideoURL;

}
