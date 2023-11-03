using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceManager : MonoBehaviour
{
    public static ReferenceManager instance;
    public LoginManager LoginManager;
    public LoadingManager LoadingManager;
    public PopupManager PopupManager;
    public GameObject SigninPanel;
    public GameObject SignupPanel;
    private void Awake()
    {
        instance = this;
    }
}
