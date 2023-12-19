using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class GeneralUnityActions : MonoBehaviour
{


    public UnityEvent OnEnableAction;
    public UnityEvent OnDisableAction;
    public UnityEvent OnDistroyAction;
    public UnityEvent WhenNoGraphAction;

    private void OnEnable()
    {
        
        OnEnableAction.Invoke();

    }

    private void OnDisable()
    {
        OnDisableAction.Invoke();
        if(ReferenceManager.instance.graphManagers.Count == 0)
        {
            WhenNoGraphAction.Invoke();
        }
    }

    private void OnDestroy()
    {
        OnDistroyAction.Invoke();
        if (ReferenceManager.instance.graphManagers.Count == 0)
        {
            WhenNoGraphAction.Invoke();
        }
    }

}
