using System.Linq;
using UnityEngine;
using UnityEngine.iOS;

public class PhysicalCameraHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int loopCount = WebCamTexture.devices.Count();
        for (int i = 0;i<loopCount;i++)
        {
            Debug.Log(WebCamTexture.devices[i].name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
