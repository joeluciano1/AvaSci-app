using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GraphMinimizer : MonoBehaviour
{
    public LayoutElement GraphToResize;

    Image imageComponent;
    public Sprite maxSprite;

    // Start is called before the first frame update

    private void Start()
    {
        imageComponent = GetComponent<Image>();
    }

    public void DoGraph(bool value)
    {
        if (value)
        {
            GraphToResize.transform.localScale = Vector3.one;
            imageComponent.color = Color.grey;
            GraphToResize.DOPreferredSize(new Vector2(0, 375), StringConstants.ANIMATIONTIME);
        }
        else
        {
            GraphToResize.transform.localScale = Vector3.zero;
            GraphToResize.DOPreferredSize(new Vector2(0, 0), StringConstants.ANIMATIONTIME);
            imageComponent.color = Color.white;
        }
    }
}
