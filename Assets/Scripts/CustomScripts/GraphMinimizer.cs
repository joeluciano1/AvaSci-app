using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
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
            imageComponent.color = Color.grey;
            GraphToResize.DOPreferredSize(new Vector2(0, 375), 1);
            
        }
        else
        {
            GraphToResize.DOPreferredSize(new Vector2(0, 0), 1);
            imageComponent.color = Color.white;
            
        }

    }
}
