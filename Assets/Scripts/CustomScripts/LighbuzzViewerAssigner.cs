using System;
using DG.Tweening;
using LightBuzz.BodyTracking;
using UnityEngine;
using UnityEngine.UI;

public class LighbuzzViewerAssigner : MonoBehaviour
{
    public TypeOfViewer ViewerType;
    public Image AppContainerBG;
    public SkeletonManager Skeleton3D;
    public Vector3 offset;
    public void AssignMe()
    {
        ReferenceManager.instance.LightBuzzMain._viewer = GetComponent<LightBuzzViewer>();
        if(ViewerType == TypeOfViewer.Type_3D)
        {
            ReferenceManager.instance.Selected3D = true;
            AppContainerBG.DOFade(0, 1f);
            Skeleton3D.transform.localScale = new Vector3(4, 4, 4);
            Skeleton3D.transform.position = Camera.main.transform.position+offset;
        }
        else if(ViewerType == TypeOfViewer.Type_2D)
        {
            ReferenceManager.instance.Selected3D = false;
            AppContainerBG.DOFade(1, 1f);
        }
    }
}

[Serializable]
public enum TypeOfViewer{
    Type_3D,
    Type_2D
}