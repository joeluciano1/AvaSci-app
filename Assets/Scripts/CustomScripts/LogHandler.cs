using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogHandler : MonoBehaviour
{
    bool debugging;
    private void OnEnable()
    {
        Application.logMessageReceived += MillGya;
    }

    private void MillGya(string condition, string stackTrace, LogType type)
    {
        if (type != LogType.Log &&debugging)
            ReferenceManager.instance.PopupManager.Show("Unity Error!", $"Kindly show following errors to developer of this app:\n- {condition}\n- {stackTrace}\n- {type}");
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= MillGya;
    }

    public void SetDebugging(bool value)
    {
        debugging = value;
    }
}
