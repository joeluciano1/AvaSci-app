using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResearchProjectCompleteBodyData
    : MonoBehaviour,
        IDragHandler,
        IPointerDownHandler,
        IBeginDragHandler,
        IEndDragHandler
{
    public TMP_Text PositionShowCase;
    public Image BGImage;
    public Vector3 Position3D;
    public Vector2 Position2D;
    public Vector3 offset;
    bool isDragging;
    public RectTransform MyTransform;
    public GameObject ShockWaveEffect;

    private void Awake()
    {
        if (MyTransform == null)
            MyTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        HideMyChilds();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        ShowMyChilds();
    }

    public void HideMyChilds()
    {
        BGImage.color = new Color(BGImage.color.r, BGImage.color.g, BGImage.color.b, 0);
        PositionShowCase.color = new Color(
            PositionShowCase.color.r,
            PositionShowCase.color.g,
            PositionShowCase.color.b,
            0
        );
    }

    public void ShowMyChilds()
    {
        BGImage.color = new Color(BGImage.color.r, BGImage.color.g, BGImage.color.b, 1);
        PositionShowCase.color = new Color(
            PositionShowCase.color.r,
            PositionShowCase.color.g,
            PositionShowCase.color.b,
            1
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition += eventData.delta;
        // offset = transform.position - new Vector3(eventData.position.x, eventData.position.y, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        HideMyChilds();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isDragging)
        {
            if (!ResearchMeasurementManager.instance.SelectedBodyDatas.Contains(this))
            {
                if (ResearchMeasurementManager.instance.selectedJoints < 2)
                {
                    ResearchMeasurementManager.instance.selectedJoints += 1;
                    ResearchMeasurementManager.instance.SelectedBodyDatas.Add(this);
                    transform.GetChild(0).GetComponent<Image>().color = Color.green;
                }
                else
                {
                    ResearchMeasurementManager.instance.selectedJoints = 0;
                    ResearchMeasurementManager.instance.SelectedBodyDatas.ForEach(x =>
                        x.transform.GetChild(0).GetComponent<Image>().color = new Color(
                            0.2830189f,
                            0.2830189f,
                            0.2830189f,
                            0.8f
                        )
                    );
                    ResearchMeasurementManager.instance.SelectedBodyDatas.Clear();
                    transform.GetChild(0).GetComponent<Image>().color = new Color(
                        0.2830189f,
                        0.2830189f,
                        0.2830189f,
                        0.8f
                    );

                    ResearchMeasurementManager.instance.selectedJoints += 1;
                    ResearchMeasurementManager.instance.SelectedBodyDatas.Add(this);
                    transform.GetChild(0).GetComponent<Image>().color = Color.green;
                }
            }
            else
            {
                ResearchMeasurementManager.instance.selectedJoints -= 1;
                ResearchMeasurementManager.instance.SelectedBodyDatas.Remove(this);
                transform.GetChild(0).GetComponent<Image>().color = new Color(
                    0.2830189f,
                    0.2830189f,
                    0.2830189f,
                    0.8f
                );
            }
        }
    }
}
