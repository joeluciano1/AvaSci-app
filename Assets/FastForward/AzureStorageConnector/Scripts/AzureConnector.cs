using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

using UnityEngine;
using UnityEngine.Networking;

//-----------------------------------------------------------------------------
// Copyright 2023 FAST FORWARD LLC. All rights reserved.
//-----------------------------------------------------------------------------

namespace FastForward.CAS
{
    public class AzureConnector : MonoBehaviour
    {
        private string _accountName;
        private string _accountKey;
        private bool _isInit;

        private string _storageServiceVersion = "2017-04-17";

        #region Public functions

        /// <summary>
        /// Set the account name and key and mark the connector as initialized.
        /// Must be called to set the account name and key before using any other functions.
        /// </summary>
        /// <param name="accountName">Account Name</param>
        /// <param name="accountKey">Shared Key for Authorization</param>
        public void Init(string accountName, string accountKey)
        {
            this._accountName = accountName;
            this._accountKey = accountKey;
            _isInit = true;
        }

        /// <summary>
        /// Upload an JPG to azure blob storage
        /// </summary>
        /// <param name="data">The jpeg as a byte array</param>
        /// <param name="containerName">The name of the container to place the image in</param>
        /// <param name="fileName">The desired file name (without file extension).  File name should abide by the following rules: https://learn.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata#blob-names</param>
        /// <param name="useMillis">Optional. If true the upload time in milliseconds will appended to the filename in the form {fileName}-{milliseconds}.jpg</param>
        /// <param name="uploadCallback">Optional. AzureUploadCallback which will be called when the process completes.</param>
        public void UploadImage(byte[] data, string containerName, string fileName, bool useMillis = false, AzureUploadCallback uploadCallback = null)
        {
            if (_isInit)
            {
                StartCoroutine(UploadImageRoutine(data, containerName, fileName, uploadCallback, useMillis));
            }
            else
            {
                Debug.LogWarning("Attempting to use AzureConnector before calling Init to set account name and key.");
            }
        }

        public void UploadBytes(byte[] data, string containerName, string fileName, bool useMillis = false, AzureUploadCallback uploadCallback = null)
        {
            if (_isInit)
            {
                StartCoroutine(UploadImageRoutine(data, containerName, fileName, uploadCallback, useMillis));
            }
            else
            {
                Debug.LogWarning("Attempting to use AzureConnector before calling Init to set account name and key.");
            }
        }

        /// <summary>
        /// Upload a string as a plain text file
        /// </summary>
        /// <param name="text">The contents of the file</param>
        /// <param name="containerName">The name of the container to place the text file in</param>
        /// <param name="fileName">The desired file name (without file extension).  File name should abide by the following rules: https://learn.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata#blob-names</param>
        /// <param name="useMillis">Optional: If true the upload time in milliseconds will appended to the filename in the form {fileName}-{milliseconds}.jpg</param>
        /// <param name="uploadCallback">Optional: AzureUploadCallback which will be called when the process completes.</param>
        public void UploadText(string text, string containerName, string fileName, bool useMillis = false, AzureUploadCallback uploadCallback = null)
        {
            if (_isInit)
            {
                Debug.Log($"Uploading: {text}");
                StartCoroutine(PutTextBlob(text, containerName, fileName, uploadCallback, useMillis));
            }
            else
            {
                Debug.LogWarning("Attempting to use AzureConnector before calling Init to set account name and key.");
            }
        }


        /// <summary>
        /// Gets the specified blob from azure storage and returns it via callback as a byte array.
        /// When decoding functions like ImageConversion.LoadImage and System.Text.Encoding.UTF8.GetString may be of use.
        /// </summary>
        /// <param name="container">The name of the container to pull the blob from</param>
        /// <param name="blobName">The name of the blob to pull (including file extension).</param>
        /// <param name="callback">AzureDataDownloadCallback which will be called on completion with the byte array and success/error information</param>
        public void GetBlob(string container, string blobName, AzureDataDownloadCallback callback)
        {
            if (_isInit)
            {
                Debug.Log("Listing Containers");
                StartCoroutine(GetBlobRoutine(container, blobName, callback));
            }
            else
            {
                Debug.LogWarning("Attempting to use AzureConnector before calling Init to set account name and key.");
            }
        }


        /// <summary>
        /// Gets a list of blobs in the container and returns it as an xml string via the callback
        /// WARNING: This doesn't handle paging so will max out at ~5,000 items.
        /// </summary>
        /// <param name="container">The name of the container to list the contents of.</param>
        /// <param name="callback">AzureTextDownloadCallback which will be called on completion with the xml string and success/error information</param>
        public void ListBlobs(string container, AzureTextDownloadCallback callback)
        {
            if (_isInit)
            {
                Debug.Log("Listing Containers");
                StartCoroutine(ListBlobsRoutine(container, callback));
            }
            else
            {
                Debug.LogWarning("Attempting to use AzureConnector before calling Init to set account name and key.");
            }
        }

        /// <summary>
        /// Gets a list of all containers within the storage account which is returned via callback as an xml string
        /// </summary>
        /// <param name="callback">AzureTextDownloadCallback which will be called on completion with the xml string and success/error information</param>
        public void ListContainers(AzureTextDownloadCallback callback)
        {
            if (_isInit)
            {
                Debug.Log("Listing Containers");
                StartCoroutine(ListContainersRoutine(callback));
            }
            else
            {
                Debug.LogWarning("Attempting to use AzureConnector before calling Init to set account name and key.");
            }
        }

        /// <summary>
        /// Creates a new container with the provided name on the account.
        /// WARNING: For some reason this causes the next call to the account to fail validation even if it is reading or writing to a different container.
        /// As a result we don't recommend using it, but if you absolutely need this functionality we recommend calling a function you don't care about right after. 
        /// </summary>
        /// <param name="containerName">The name of the new container. Must provide name rules listed here: https://learn.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata#container-names</param>
        /// <param name="uploadCallback">Optional: AzureUploadCallback which will be called when the process completes with success and error information.</param>
        public void CreateContainer(string containerName, AzureUploadCallback uploadCallback = null)
        {
            if (_isInit)
            {
                Debug.Log($"Creating Container: {containerName}");
                StartCoroutine(CreateContainerRoutine(containerName, uploadCallback));
            }
            else
            {
                Debug.LogWarning("Attempting to use AzureConnector before calling Init to set account name and key.");
            }
        }

        #endregion

        #region Write Blobs

        private IEnumerator UploadImageRoutine(byte[] data, string containerName, string fileName, AzureUploadCallback uploadCallback, bool useMillis = true)
        {
            // https://docs.microsoft.com/en-us/rest/api/storageservices/put-blob
            // for emulation use uri:  http://127.0.0.1:10000/devstoreaccount1/mycontainer/myblob

            string requestMethod = "PUT";
            string blobType = "BlockBlob";
            string contentType = "image/jpeg";

            // Get the time
            DateTime now = DateTime.UtcNow;
            string date = now.ToString("R", CultureInfo.InvariantCulture);
            string dateMillis = new DateTimeOffset(now).ToUnixTimeMilliseconds().ToString();

            // Give the blob a name
            string blobName = $"{fileName}{(useMillis ? "-" + dateMillis : "")}.jpg";

            // Get size of the blob
            int blobLength = data.Length;

            // Contruct URI of the blob
            string blobUri = string.Format("{0}/{1}", containerName, blobName);

            // Construct the URI. This will look like this:
            string uri = string.Format("https://{0}.blob.core.windows.net/{1}", _accountName, blobUri);

            using (UnityWebRequest request = UnityWebRequest.Put(uri, data))
            {
                request.uploadHandler.contentType = contentType;

                request.SetRequestHeader("x-ms-blob-type", blobType);
                request.SetRequestHeader("x-ms-date", date);
                request.SetRequestHeader("x-ms-version", _storageServiceVersion);

                //  all x-ms headers in the request must be included here, sorted alphabetically
                string canonicalizedHeaders = string.Format(
                    "x-ms-blob-type:{0}\nx-ms-date:{1}\nx-ms-version:{2}\n",
                    blobType,
                    date,
                    _storageServiceVersion
                );

                string canonicalizedResource = GetCanonicalizedResource(request.uri, _accountName);

                // This is the raw representation of the message signature.
                string signature = string.Format("{0}\n\n\n{3}\n\n{4}\n\n\n\n\n\n\n{1}{2}",
                    requestMethod,
                    canonicalizedHeaders,
                    canonicalizedResource,
                    blobLength,
                    contentType);
                string encodedSignature = EncodeMessageSignature(signature);

                // Add the encoded signature to the request
                request.SetRequestHeader("Authorization", encodedSignature);

                // Send the request.
                request.SendWebRequest();

                while (!request.isDone)
                {
                    Debug.Log($"GET progress: {request.uploadProgress}");
                    yield return 0;
                }

                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogWarning($"PUT error: {request.error}");
                    Debug.LogWarning($"PUT error: {request.downloadHandler.text}");
                    uploadCallback?.Invoke(false, request.error);
                }
                else
                {
                    Debug.Log($"PUT complete {request.downloadHandler.text}");
                    uploadCallback?.Invoke(true, request.error, uri);
                }
            }
        }

        private IEnumerator PutTextBlob(string data, string containerName, string fileName, AzureUploadCallback uploadCallback, bool useMillis = true)
        {
            // https://docs.microsoft.com/en-us/rest/api/storageservices/put-blob
            // for emulation use uri:  http://127.0.0.1:10000/devstoreaccount1/mycontainer/myblob

            string requestMethod = "PUT";
            string blobType = "BlockBlob";
            string contentType = "text/plain; charset=UTF-8";

            // Get the time
            DateTime now = DateTime.UtcNow;
            string date = now.ToString("R", CultureInfo.InvariantCulture);
            string dateMillis = new DateTimeOffset(now).ToUnixTimeMilliseconds().ToString();

            // Give the blob a name
            //string blobName = string.Format("upload-{0}.txt", dateMillis);
            string blobName = $"{fileName}{(useMillis ? "-" + dateMillis : "")}.txt";

            // Get size of the blob
            int blobLength = data.Length;

            // Contruct URI of the blob
            string blobUri = string.Format("{0}/{1}", containerName, blobName);

            // Construct the URI. This will look like this:
            string uri = string.Format("https://{0}.blob.core.windows.net/{1}", _accountName, blobUri);

            using (UnityWebRequest request = UnityWebRequest.Put(uri, data))
            {
                request.uploadHandler.contentType = contentType;

                request.SetRequestHeader("x-ms-blob-type", blobType);
                request.SetRequestHeader("x-ms-date", date);
                request.SetRequestHeader("x-ms-version", _storageServiceVersion);

                //  all x-ms headers in the request must be included here, sorted alphabetically
                string canonicalizedHeaders = string.Format(
                    "x-ms-blob-type:{0}\nx-ms-date:{1}\nx-ms-version:{2}\n",
                    blobType,
                    date,
                    _storageServiceVersion
                );

                string canonicalizedResource = GetCanonicalizedResource(request.uri, _accountName);

                // This is the raw representation of the message signature.
                string signature = string.Format("{0}\n\n\n{3}\n\n{4}\n\n\n\n\n\n\n{1}{2}",
                    requestMethod,
                    canonicalizedHeaders,
                    canonicalizedResource,
                    blobLength,
                    contentType);
                string encodedSignature = EncodeMessageSignature(signature);

                // Add the encoded signature to the request
                request.SetRequestHeader("Authorization", encodedSignature);

                // Send the request.
                request.SendWebRequest();

                while (!request.isDone)
                {
                    Debug.Log($"GET progress: {request.uploadProgress}");
                    ReferenceManager.instance.ProgressManager.Show("Uploading Video", request.uploadProgress);

                    yield return 0;
                }
                ReferenceManager.instance.ProgressManager.Hide();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"PUT error: {request.error}");
                    Debug.LogWarning($"PUT error: {request.downloadHandler.text}");
                    uploadCallback?.Invoke(false, request.error);
                }
                else
                {
                    Debug.Log($"PUT complete {request.downloadHandler.text}");
                    uploadCallback?.Invoke(true, request.error, uri);
                }
            }
        }

        #endregion

        #region Read Blobs

        private IEnumerator GetBlobRoutine(string container, string blobName, AzureDataDownloadCallback callback)
        {
            // Construct the URI. This will look like this:
            string uri = string.Format("https://{0}.blob.core.windows.net/{1}/{2}", _accountName, container, blobName);

            // Get the time
            DateTime now = DateTime.UtcNow;
            string date = now.ToString("R", CultureInfo.InvariantCulture);

            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                // Add the request headers for x-ms-date and x-ms-version.
                request.SetRequestHeader("x-ms-date", date);
                request.SetRequestHeader("x-ms-version", _storageServiceVersion);

                //  all x-ms headers in the request must be included here, sorted alphabetically
                string canonicalizedHeaders = string.Format(
                    "x-ms-date:{0}\nx-ms-version:{1}\n",
                    date,
                    _storageServiceVersion
                );

                string canonicalizedResource = GetCanonicalizedResource(request.uri, _accountName);

                // This is the raw representation of the message signature.
                string signature = string.Format("{0}\n\n\n\n\n\n\n\n\n\n\n\n{1}{2}",
                    request.method,
                    canonicalizedHeaders,
                    canonicalizedResource);
                string encodedSignature = EncodeMessageSignature(signature);

                // Add the encoded signature to the request
                request.SetRequestHeader("Authorization", encodedSignature);

                // Send the request.
                request.SendWebRequest();

                while (!request.isDone)
                {
                    Debug.Log($"GET progress: {request.downloadProgress}");
                    yield return 0;
                }

                callback(request.result == UnityWebRequest.Result.Success, request.error, request.downloadHandler.data);
            }
        }

        /**
     * List Container contents in the account
     */
        private IEnumerator ListBlobsRoutine(string container, AzureTextDownloadCallback callback)
        {
            // Construct the URI. This will look like this:
            string uri = string.Format("https://{0}.blob.core.windows.net/{1}?restype=container&comp=list", _accountName, container);

            // Get the time
            DateTime now = DateTime.UtcNow;
            string date = now.ToString("R", CultureInfo.InvariantCulture);

            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                // Add the request headers for x-ms-date and x-ms-version.
                request.SetRequestHeader("x-ms-date", date);
                request.SetRequestHeader("x-ms-version", _storageServiceVersion);

                //  all x-ms headers in the request must be included here, sorted alphabetically
                string canonicalizedHeaders = string.Format(
                    "x-ms-date:{0}\nx-ms-version:{1}\n",
                    date,
                    _storageServiceVersion
                );

                string canonicalizedResource = GetCanonicalizedResource(request.uri, _accountName);

                // This is the raw representation of the message signature.
                string signature = string.Format("{0}\n\n\n\n\n\n\n\n\n\n\n\n{1}{2}",
                    request.method,
                    canonicalizedHeaders,
                    canonicalizedResource);
                string encodedSignature = EncodeMessageSignature(signature);

                // Add the encoded signature to the request
                request.SetRequestHeader("Authorization", encodedSignature);

                // Send the request.
                request.SendWebRequest();

                while (!request.isDone)
                {
                    Debug.Log($"GET progress: {request.downloadProgress}");
                    yield return 0;
                }

                callback(request.result == UnityWebRequest.Result.Success, request.error, request.downloadHandler.text);
            }
        }



        #endregion

        #region Containers

        /**
     * List Containers in the account
     */
        private IEnumerator ListContainersRoutine(AzureTextDownloadCallback callback)
        {
            // Construct the URI. This will look like this:
            string uri = string.Format("https://{0}.blob.core.windows.net?comp=list", _accountName);

            // Get the time
            DateTime now = DateTime.UtcNow;
            string date = now.ToString("R", CultureInfo.InvariantCulture);

            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                // Add the request headers for x-ms-date and x-ms-version.
                request.SetRequestHeader("x-ms-date", date);
                request.SetRequestHeader("x-ms-version", _storageServiceVersion);

                //  all x-ms headers in the request must be included here, sorted alphabetically
                string canonicalizedHeaders = string.Format(
                    "x-ms-date:{0}\nx-ms-version:{1}\n",
                    date,
                    _storageServiceVersion
                );

                string canonicalizedResource = GetCanonicalizedResource(request.uri, _accountName);

                // This is the raw representation of the message signature.
                string signature = string.Format("{0}\n\n\n\n\n\n\n\n\n\n\n\n{1}{2}",
                    request.method,
                    canonicalizedHeaders,
                    canonicalizedResource);
                string encodedSignature = EncodeMessageSignature(signature);

                // Add the encoded signature to the request
                request.SetRequestHeader("Authorization", encodedSignature);

                // Send the request.
                request.SendWebRequest();

                while (!request.isDone)
                {
                    Debug.Log($"GET progress: {request.downloadProgress}");
                    yield return 0;
                }

                callback(request.result == UnityWebRequest.Result.Success, request.error, request.downloadHandler.text);
            }
        }

        /*
         * Create a container in the account
         */
        private IEnumerator CreateContainerRoutine(string containerName, AzureUploadCallback uploadCallback = null)
        {
            // https://docs.microsoft.com/en-us/rest/api/storageservices/create-container

            string requestMethod = "PUT";
            //string blobType = "BlockBlob";
            string contentType = "text/plain; charset=UTF-8";
            string publicAccess = "blob";

            // Get the time
            DateTime now = DateTime.UtcNow;
            string date = now.ToString("R", CultureInfo.InvariantCulture);
            string dateMillis = new DateTimeOffset(now).ToUnixTimeMilliseconds().ToString();

            //// Give the blob a name
            //string blobName = String.Format("upload-{0}.txt", dateMillis);

            //// Get size of the blob
            //Int32 blobLength = data.Length;

            //// Contruct URI of the blob
            //string blobUri = String.Format("{0}/{1}", CONTAINER_NAME_DATA, blobName);

            // Construct the URI. This will look like this:
            string uri = string.Format("https://{0}.blob.core.windows.net/{1}?restype=container",
                _accountName,
                containerName);

            byte[] data = Encoding.UTF8.GetBytes(" ");
            int contentLength = data.Length;

            using (UnityWebRequest request = UnityWebRequest.Put(uri, data))
            {
                request.method = requestMethod;
                request.uploadHandler.contentType = contentType;

                //request.SetRequestHeader("x-ms-blob-type", blobType);
                request.SetRequestHeader("x-ms-date", date);
                request.SetRequestHeader("x-ms-version", _storageServiceVersion);
                request.SetRequestHeader("x-ms-blob-public-access", publicAccess);

                //  all x-ms headers in the request must be included here, sorted alphabetically
                string canonicalizedHeaders = string.Format(
                    "x-ms-blob-public-access:{0}\nx-ms-date:{1}\nx-ms-version:{2}\n",
                    publicAccess,
                    date,
                    _storageServiceVersion
                );

                string canonicalizedResource = GetCanonicalizedResource(request.uri, _accountName);


                // This is the raw representation of the message signature.
                string signature = string.Format("{0}\n\n\n{3}\n\n{4}\n\n\n\n\n\n\n{1}{2}",
                    requestMethod,
                    canonicalizedHeaders,
                    canonicalizedResource,
                    contentLength,
                    contentType);
                string encodedSignature = EncodeMessageSignature(signature);

                // Add the encoded signature to the request
                request.SetRequestHeader("Authorization", encodedSignature);

                // Send the request.
                request.SendWebRequest();

                while (!request.isDone)
                {
                    Debug.Log($"GET progress: {request.downloadProgress}");
                    yield return null;
                }

                if (!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogWarning($"PUT error: {request.error}");
                    Debug.LogWarning($"PUT error: {request.downloadHandler.text}");
                }
                else
                {
                    Debug.Log($"PUT complete {request.downloadHandler.text}");
                }

                uploadCallback?.Invoke(request.result == UnityWebRequest.Result.Success, request.error, uri);
            }
        }

        #endregion

        #region Helpers

        /**
     *
     *  Builds the CanonicalizedResource string 
     */
        private string GetCanonicalizedResource(Uri address, string storageAccountName)
        {
            // The absolute path is "/" because for we're getting a list of containers.
            StringBuilder sb = new StringBuilder("/").Append(storageAccountName).Append(address.AbsolutePath);

            // Address.Query is the resource, such as "?comp=list".
            // This ends up with a NameValueCollection with 1 entry having key=comp, value=list.
            // It will have more entries if you have more query parameters.
            NameValueCollection values = HttpUtility.ParseQueryString(address.Query);

            foreach (var item in values.AllKeys.OrderBy(k => k))
            {
                sb.Append('\n').Append(item).Append(':').Append(values[item]);
            }

            return sb.ToString().ToLower();
        }


        /*
         *
         * Encodes the message signature using account key
         * 
         * String format for signing requests
         *
         * StringToSign = VERB + "\n" +
         * Content-Encoding + "\n" +
         * Content-Language + "\n" +
         * Content-Length + "\n" +
         * Content-MD5 + "\n" +
         * Content-Type + "\n" +
         * Date + "\n" +
         * If-Modified-Since + "\n" +
         * If-Match + "\n" +
         * If-None-Match + "\n" +
         * If-Unmodified-Since + "\n" +
         * Range + "\n" +
         * CanonicalizedHeaders +
         * CanonicalizedResource;
         */

        private string EncodeMessageSignature(string messageSignature)
        {
            // Now turn it into a byte array.
            byte[] SignatureBytes = Encoding.UTF8.GetBytes(messageSignature);

            // Create the HMACSHA256 version of the storage key.
            HMACSHA256 SHA256 = new HMACSHA256(Convert.FromBase64String(_accountKey));

            // Compute the hash of the SignatureBytes and convert it to a base64 string.
            string signature = Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes));

            // This is the actual header that will be added to the list of request headers.
            string header = $"SharedKey {_accountName}:{signature}";
            return header;
        }

        #endregion

        #region Singleton Init

        private static AzureConnector m_instance;

        public static AzureConnector Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<AzureConnector>();

                    // If nothing was found dynamically create the manager
                    if (m_instance == null)
                    {
                        GameObject go = new GameObject("_FF_AzureConnector");
                        m_instance = go.AddComponent<AzureConnector>();
                    }

                    //Tell unity not to destroy this object when loading a new scene!
                    DontDestroyOnLoad(m_instance.gameObject);
                }

                return m_instance;
            }
        }

        //When the object awakens, we assign the static variable
        private void Awake()
        {
            if (m_instance == null)
            {
                //If I am the first instance, make me the Singleton
                m_instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                //If a Singleton already exists and you find
                //another reference in scene, destroy it!
                if (this != m_instance)
                    Destroy(gameObject);
            }
        }

        #endregion
    }

}