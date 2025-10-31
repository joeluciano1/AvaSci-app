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
    public bool hideSkeleton;
    public ScrollRect scrollRect;
    
    private void OnEnable()
    {

        OnEnableAction?.Invoke();
        if (hideSkeleton)
        {
            
            ReferenceManager.instance.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            ReferenceManager.instance.SkeletonHidingPanels.Add(this.gameObject);
        }
    }

    private void OnDisable()
    {
        OnDisableAction.Invoke();
        if (ReferenceManager.instance.graphManagers.Count == 0)
        {
            WhenNoGraphAction?.Invoke();
        }
        if (hideSkeleton)
        {
            ReferenceManager.instance.SkeletonHidingPanels.Remove(this.gameObject);
            if (ReferenceManager.instance.SkeletonHidingPanels.Count == 0)
            {
                ReferenceManager.instance.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }

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
   
[SerializeField, Range(0.05f, 0.25f)] private float pullThresholdPercent = 0.12f; // 12% of viewport height
[SerializeField] private float minThresholdPx = 60f;
[SerializeField] private float maxThresholdPx = 140f;
[SerializeField, Range(0f, 0.1f)] private float topTolerance = 0.02f; // for verticalNormalizedPosition
private bool readyToRefresh = false;

// Helper: "how much have we pulled past the top?" in pixels
private float GetTopOverscrollPx(ScrollRect sr)
{
    // At top, pulling DOWN makes content.anchoredPosition.y go NEGATIVE.
    // Overscroll is positive distance downwards from the top stop.
    return Mathf.Max(0f, -sr.content.anchoredPosition.y);
}

public void DetectScrollOnTop(ScrollRect scrollRect)
{
    // Compute a size-aware threshold once per call (cheap)
    float viewportH = (scrollRect.viewport ? scrollRect.viewport.rect.height : scrollRect.GetComponent<RectTransform>().rect.height);
    float thresholdPx = Mathf.Clamp(viewportH * pullThresholdPercent, minThresholdPx, maxThresholdPx);

    // Are we basically at the top?
    bool atTop = scrollRect.verticalNormalizedPosition >= (1f - topTolerance);

    // Pointer/drag state (mouse OR touch works)
    bool isPointerDown = Input.GetMouseButton(0) || Input.touchCount > 0;
    bool pointerJustReleased = Input.GetMouseButtonUp(0) || (Input.touchCount == 0 && !Input.GetMouseButton(0)); // good enough for this style

    // How far the user has over-pulled past the top
    float overscroll = GetTopOverscrollPx(scrollRect);

    // ----- UI: show/hide loader while pulling -----
    if (atTop && isPointerDown && overscroll > 4f)  // small noise guard
    {
        if (loadedLoader == null)
        {
            loadedLoader = Instantiate(LoaderPrefab, scrollRect.content.transform.parent);
            var rt = loadedLoader.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -40f);
            loadedLoader.transform.SetAsFirstSibling();
        }
    }
    else if ((!isPointerDown || overscroll <= 2f) && loadedLoader != null && !readyToRefresh)
    {
        Destroy(loadedLoader);
        loadedLoader = null;
    }

    // ----- Logic: threshold & trigger -----
    if (atTop && isPointerDown)
    {
        // Update "ready" state while dragging
        readyToRefresh = overscroll >= thresholdPx;

        // (Optional) you can animate the loader based on overscroll/threshold here
        // e.g., loadedLoader.fillAmount = Mathf.Clamp01(overscroll / thresholdPx);
    }

    // On release: if we crossed the threshold while at top, fire the action
    if (pointerJustReleased)
    {
        if (readyToRefresh && atTop)
        {
            WhenScrolledToTop?.Invoke();
        }

        // Reset state
        readyToRefresh = false;

        // Let ScrollRect bounce back; remove loader after release (unless you keep it during refresh)
        if (loadedLoader != null)
        {
            Destroy(loadedLoader);
            loadedLoader = null;
        }
    }
}

    
    public void ScrollToTopPosition()
    {
        // Set verticalNormalizedPosition to 1 (top of the content)
        scrollRect.DOVerticalNormalizedPos(1f, 1f);
    }
}
