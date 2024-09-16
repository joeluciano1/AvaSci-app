using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashLogoAnimationHandler : MonoBehaviour
{
    public Image Logo;
    void Start()
    {
        Logo.DOFade(1, 3);
        Logo.transform.DOScale(1.5f, 3).OnComplete(()=>
        {
            StartScene();
        });
        
    }

    async void StartScene()
    {
        await Task.Delay(1000);
        Logo.DOFade(0,1).OnComplete(()=>SceneManager.LoadSceneAsync(1));
    }
}
