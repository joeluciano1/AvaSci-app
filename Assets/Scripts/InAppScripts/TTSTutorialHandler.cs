using System;
using System.Collections.Generic;
using LMNT;
using Unity.VisualScripting;
using UnityEngine;

public class TTSTutorialHandler : MonoBehaviour
{
    public LMNTSpeech LmntSpeech;
    public List<string> LinesToSay = new List<string>();
    public bool skip;
    int currentLine;
    private void Start()
    {
        if (currentLine > LinesToSay.Count - 1)
        {
            currentLine = 0;
        }
        
        
    }
    
    public void NextLine(string speachLine)
    {
        if (skip)
            return;
        LmntSpeech.dialogue = speachLine;
        StartCoroutine(LmntSpeech.Talk());
        
    }
}
