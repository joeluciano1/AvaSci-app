using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class GeneralUnityActions : MonoBehaviour
{


    public UnityEvent OnEnableAction;
    public UnityEvent OnDisableAction;
    public UnityEvent OnDistroyAction;
    public UnityEvent WhenNoGraphAction;
    public float scrollSensitivity = 40f;

    public UnityEvent WhenScrolledToTop;
    public GameObject LoaderPrefab;
    [HideInInspector] public GameObject loadedLoader;
    Vector2 initialPositionoOfContent;

    public ScrollRect scrollRect;
    private void OnEnable()
    {

        OnEnableAction?.Invoke();

    }

    private void OnDisable()
    {
        OnDisableAction.Invoke();
        if (ReferenceManager.instance.graphManagers.Count == 0)
        {
            WhenNoGraphAction?.Invoke();
        }
    }

    private void OnDestroy()
    {
        OnDistroyAction.Invoke();
        if (ReferenceManager.instance.graphManagers.Count == 0)
        {
            WhenNoGraphAction?.Invoke();
        }
    }
    public void DetectScrollOnTop(ScrollRect scrollRect)
    {
        
        if (initialPositionoOfContent == Vector2.zero)
        {
            Debug.Log("Yahan Zero Tha");
            initialPositionoOfContent = scrollRect.content.anchoredPosition;
        }
        float difference = initialPositionoOfContent.y - scrollRect.content.anchoredPosition.y;
        Debug.Log("Scroll Sensitivity: " + scrollSensitivity + "\nAnd Difference: "+ difference + "\nAnd Normal Position: "+ scrollRect.verticalNormalizedPosition);
        // Debug.Log(difference);
        if (difference >= scrollSensitivity && loadedLoader == null && Input.GetMouseButton(0))
        {
            loadedLoader = Instantiate(LoaderPrefab, scrollRect.content.transform.parent);
            loadedLoader.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);
            loadedLoader.transform.SetAsFirstSibling();
        }
        if (difference >= scrollSensitivity && Input.GetMouseButtonUp(0))
        {
            WhenScrolledToTop?.Invoke();
            initialPositionoOfContent = Vector2.zero;
        }
        if (difference <= scrollSensitivity && loadedLoader != null)
        {
            Destroy(loadedLoader);
            loadedLoader = null;
            // initialPositionoOfContent = Vector2.zero;
        }

        if (!Input.GetMouseButtonUp(0) && !Input.GetMouseButton(0) && scrollRect.verticalNormalizedPosition.Equals(1) && difference != 0)
        {
            initialPositionoOfContent = Vector2.zero;
        }
    }
    
    public void ScrollToTopPosition()
    {
        // Set verticalNormalizedPosition to 1 (top of the content)
        scrollRect.DOVerticalNormalizedPos(1f, 1f);
    }
}
