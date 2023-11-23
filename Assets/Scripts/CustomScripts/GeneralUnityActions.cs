using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class GeneralUnityActions : MonoBehaviour
{


    public UnityEvent ActionToPerform;
    public TypeOfAction typeOfAction;

    private void OnEnable()
    {
        if(typeOfAction == TypeOfAction.LightbuzzWorld)
           ActionToPerform.Invoke();
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
public enum TypeOfAction
{
    LightbuzzWorld
}