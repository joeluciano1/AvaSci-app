using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public TMP_Text HeadingText;
    public TMP_Text ContentText;
    Image MyImage;
    public async void Show(string heading, string content, bool fade=false)
    {
        MyImage = GetComponent<Image>();
        gameObject.SetActive(true);
        MyImage.color = new Color(MyImage.color.r, MyImage.color.g, MyImage.color.b, 0);
        MyImage.DOFade(1, 1f);
        HeadingText.text = heading;
        ContentText.text = content;
        await System.Threading.Tasks.Task.Delay(3000);
        if (fade)
        {
            MyImage.DOFade(0, 2f).OnComplete(() => gameObject.SetActive(false)) ;
        }
        
    }
}
