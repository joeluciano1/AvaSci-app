using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class APIHandler : MonoBehaviour
{
    public static APIHandler instance;
    

    private void Awake()
    {
        instance = this;
    
    }

    public void Get(string endpoint, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(GetRequest($"{StringConstants.BASEENDPOINT}/{endpoint}", onSuccess, onError));
    }

    private IEnumerator GetRequest(string url, System.Action<string> onSuccess, System.Action<string> onError)
    {
        ReferenceManager.instance.LoadingManager.gameObject.SetActive(true);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                onError?.Invoke(www.error);
            }
            else
            {
                onSuccess?.Invoke(www.downloadHandler.text);
            }
            ReferenceManager.instance.LoadingManager.gameObject.SetActive(false);
        }
    }

    public void Post(string endpoint, string json, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(PostRequest($"{StringConstants.BASEENDPOINT}/{endpoint}", json, onSuccess, onError));
    }

    private IEnumerator PostRequest(string url, string json, System.Action<string> onSuccess, System.Action<string> onError)
    {
        ReferenceManager.instance.LoadingManager.gameObject.SetActive(true);
        var request = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            onError?.Invoke(request.error);
        }
        else
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        ReferenceManager.instance.LoadingManager.gameObject.SetActive(false);
    }
}
