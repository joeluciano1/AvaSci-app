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
    [HideInInspector] public bool doFade;
    [HideInInspector] public Image MyImage;
    public async void Show(string heading, string content, bool fade=false)
    {
        PopupManager popupManager = Instantiate(this, this.transform.parent);
        popupManager.doFade = fade;
        popupManager.MyImage = popupManager.GetComponent<Image>();
        popupManager.gameObject.SetActive(true);
        popupManager.MyImage.color = new Color(popupManager.MyImage.color.r, popupManager.MyImage.color.g, popupManager.MyImage.color.b, 0);
        popupManager.MyImage.DOFade(1, 1f);
        popupManager.HeadingText.DOFade(1, 1f);
        popupManager.ContentText.DOFade(1, 1f);
        popupManager.HeadingText.text = heading;
        popupManager.ContentText.text = content;
        await System.Threading.Tasks.Task.Delay(3000);
        if (popupManager.doFade &&popupManager!=null)
        {
            popupManager.transform.SetAsLastSibling();
            popupManager.HeadingText.DOFade(0, 1f);
            popupManager.ContentText.DOFade(0, 1f);
            popupManager.MyImage.DOFade(0, 2f).OnComplete(() => Destroy(popupManager.gameObject)) ;
            
        }
        
    }

    public void OnOkClick()
    {
        MyImage.DOFade(0, 1f).OnComplete(() => Destroy(gameObject));
    }
}
