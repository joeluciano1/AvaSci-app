using System.Collections;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2023 FAST FORWARD LLC. All rights reserved.
//-----------------------------------------------------------------------------

namespace FastForward.CAS
{
    public class TestScript : MonoBehaviour
    {
        public string accountName;
        public string accountKey;

        public Texture2D uploadTestImage;
        public Texture2D downloadTestImage;

        public string container;
        public string downloadImageBlobName;
        public string downloadTextBlobName;

        public float delayBetweenCalls = 3;

        public void Start()
        {
            AzureConnector.Instance.Init(accountName, accountKey);

            StartCoroutine(TestRoutine());
        }

        private IEnumerator TestRoutine()
        {
            var waitForSeconds = new WaitForSeconds(delayBetweenCalls);
            yield return waitForSeconds;
            AzureConnector.Instance.ListContainers(ListContainersCallback);
            yield return waitForSeconds;
            AzureConnector.Instance.ListBlobs(container, ListBlobsCallback);
            yield return waitForSeconds;
            AzureConnector.Instance.CreateContainer("script-test", CreatContainerCallback);
            yield return waitForSeconds;
            AzureConnector.Instance.ListContainers(ListContainersCallback);
            yield return waitForSeconds;
            AzureConnector.Instance.ListContainers(ListContainersCallback);
            yield return waitForSeconds;
            AzureConnector.Instance.UploadImage(uploadTestImage.EncodeToJPG(), container, "img", false, UploadImageCallback);
            yield return waitForSeconds;
            AzureConnector.Instance.UploadText("This is a test", container, "testing123", true, UploadTextCallback);
            yield return waitForSeconds;
            AzureConnector.Instance.GetBlob(container, downloadImageBlobName, GetBlobImageCallback);
            yield return waitForSeconds;
            AzureConnector.Instance.GetBlob(container, downloadTextBlobName, GetBlobTextCallback);

        }



        private void CreatContainerCallback(bool success, string error, string uri)
        {
            if (success)
            {
                Debug.Log("Container created.");
            }
            else
            {
                Debug.LogWarning("Container Creation Failed.  " + error);
            }
        }


        private void ListContainersCallback(bool success, string error, string text)
        {
            if (success)
            {
                Debug.Log(text);
            }
            else
            {
                Debug.LogWarning("List Containers failed. " + error);
            }
        }

        private void GetBlobImageCallback(bool success, string error, byte[] data)
        {
            if (success)
            {
                Debug.Log("Got image");

                if (data != null)
                {
                    downloadTestImage.LoadImage(data);
                }
            }
            else
            {
                Debug.LogWarning("Get blob failed. " + error);
            }
        }

        private void GetBlobTextCallback(bool success, string error, byte[] data)
        {
            if (success)
            {
                Debug.Log("Got text file");

                if (data != null)
                {
                    var text = System.Text.Encoding.UTF8.GetString(data);
                    Debug.Log("Text from file: " + text);
                }
            }
            else
            {
                Debug.LogWarning("Get blob failed. " + error);
            }
        }

        private void ListBlobsCallback(bool success, string error, string text)
        {
            if (success)
            {
                Debug.Log(text);
            }
            else
            {
                Debug.LogWarning("List Blobs failed. " + error);
            }
        }

        private void UploadTextCallback(bool success, string error, string uri)
        {
            if (success)
            {
                Debug.Log("Text uploaded");
            }
            else
            {
                Debug.LogWarning("Text upload failed. " + error);
            }
        }

        private void UploadImageCallback(bool success, string error, string uri)
        {
            if (success)
            {
                Debug.Log("Image Uploaded");
            }
            else
            {
                Debug.LogWarning("Image upload failed. " + error);
            }
        }
    }
}