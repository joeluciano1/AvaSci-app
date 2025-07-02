using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DG.Tweening;
using LightBuzz.AvaSci.Csv;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;

public class ChatGPTHandler : MonoBehaviour
{
    [SerializeField]private string apiKey = "";
    private string openAIEndpoint = "https://api.openai.com/v1/chat/completions";

    public void AnalyzeCSV(List<string> csvDatas, bool isGait)
    {
        if (!isGait)
        {
            AnalyzeNormal(csvDatas);
        }
        else
        {
            AnalyzeGait(csvDatas);
        }
        
    }

    void AnalyzeNormal(List<string> csvDatas)
    {
        string prompt = 
            "You are an expert in data visualization. " +
            "Given the following Csv datas, generate a single self-contained HTML file. " +
            "It should:\n" +
            "1) Include a <canvas> element and use Chart.js from a CDN to display. " +
            "   - The X-axis should be the value of the first column in the csv data (e.g., 00:00:001,00:01:201,00:02:001,...). " +
            "   - The Y-axis should be the values of all the columns with multiple lines representing each column.\n" +
            "2) Include a descriptive summary of the datas above the chart.\n" +
            "3) The chart should have a title and labeled axes.\n" +
            "4) Return only the complete HTML. Do not include any markdown formatting.\n" +
            "5) Summarize interesting points in the data such as max angle, min angle, average, etc. Also include a table view of the csv datas.\n\n" +
            "CSV datas:\n" + csvDatas[0] + " and " + csvDatas[1];



        StartCoroutine(SendRequestToChatGPT(prompt));
    }
    void AnalyzeGait(List<string> csvDatas)
    {
        string prompt = 
            "You are an expert in data visualization. " +
            "Given the following Csv datas, generate a single self-contained HTML file. " +
            "It should:\n" +
            "1) Include two <canvas> element which will show data according to time on column 1 and time on column 2 and use Chart.js from a CDN to display. " +
            "   - The X-axis should be the value of the time column in the csv datas (e.g., 00:00:001,00:01:201,00:02:001,...). " +
            "   - The Y-axis should be the values of all the columns with multiple lines representing each column.\n" +
            "2) Include a descriptive summary of the datas above the chart.\n" +
            "3) The chart should have a title and labeled axes.\n" +
            "4) Return only the complete HTML. Do not include any markdown formatting.\n" +
            "5) Summarize interesting points in the data such as max angle, min angle, average, etc. Also include a table view of the csv datas.\n\n" +
            "CSV datas:\n" + csvDatas[0] + " and " + csvDatas[1];



        StartCoroutine(SendRequestToChatGPT(prompt));
    }
    IEnumerator SendRequestToChatGPT(string prompt)
    {
        var requestData = new
        {
            model = "gpt-4o",  // Use GPT-4 turbo for faster responses
            messages = new[]
            {
                new { role = "system", content = "You are an expert data analyst, doctor, and HTML report generator." },
                new { role = "user", content = prompt }
            },
            max_tokens = 4000,
            temperature = 0.7
        };

        string jsonBody = JsonConvert.SerializeObject(requestData);
        ReferenceManager.instance.LoadingManager.Show("Generating AI Report");
        using (UnityWebRequest request = new UnityWebRequest(openAIEndpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("OpenAI Response Received");

                // Parse response to get generated HTML content
                ProcessResponse(responseText);
            }
            else
            {
                Debug.LogError("Error: " + request.responseCode + " - " + request.downloadHandler.text);
            }
        }
        ReferenceManager.instance.LoadingManager.Hide();
    }
   
    void ProcessResponse(string jsonResponse)
    {
        // Extract report content from JSON response
        var responseData = JsonUtility.FromJson<ChatGPTResponse>(jsonResponse);
        Debug.Log("Generated Report: " + responseData.choices[0].message.content);
        CreatePDFReport(responseData.choices[0].message.content);
    }
    public void CreatePDFReport(string reportContent)
    {
        string path = Application.persistentDataPath + "/Reportt.html";
        File.WriteAllText(path, reportContent);
        // CSVManager.Export(path);
#if UNITY_EDITOR
        System.Diagnostics.Process.Start(path);
        Debug.Log("Is Editor");

#else

            string url = "file://" + path.Replace(" ", "%20");
            Debug.Log("URL = " + url);
            Debug.Log("Persistance = " + path);
            GeneralStaticManager.OpenFile(path);
#endif
        Debug.Log("PDF Report saved at: " + path);
    }

   
    
    [Header("UI Elements")]
    public TMP_InputField userInputField;

    public GameObject GPTText;
    public GameObject UserText;
    public Button sendButton;

    [Header("Settings")]
    private string apiUrl = "https://www.askyourdatabase.com/api/chatbot/v2/session";

    // Track full chat history
    private List<Message> conversationHistory = new List<Message>();
    public TMP_Dropdown modelDropdown;
    public GameObject Thinking;
    void Start()
    {
       
    }

    public void OnSendClicked()
    {
      StartCoroutine(SendMessageToOpenAI());
    }

    public string chatBotAPIKey;
    IEnumerator SendMessageToOpenAI()
    {
        var payload = new
        {
            chatbotid = "2b18e064b48ec7fd9260c55094072c64", // or "gpt-4" if you have access
            name = "Joe",
            email = "joe@avasci.com"
        };
        Thinking.SetActive(true);
        ReferenceManager.instance.LoadingManager.Show("Connecting to Live Database");
        string jsonBody = JsonConvert.SerializeObject(payload);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + chatBotAPIKey);

        yield return request.SendWebRequest();
        Thinking.SetActive(false);
        ReferenceManager.instance.LoadingManager.Hide();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.result + " - " + request.downloadHandler.text);
            // AppendMessage("Error", request.error);
        }
        else
        {
            string resultJson = request.downloadHandler.text;
            var response = JsonConvert.DeserializeObject<AskReponse>(resultJson);
            var webView = gameObject.AddComponent<UniWebView>();
            webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            
// 2. Load a URL.
            webView.Load(response.url);

// 3. Show it. 🎉
            webView.Show();
            webView.EmbeddedToolbar.Show();
            webView.BackgroundColor = Color.black;
            webView.SetOpenLinksInExternalBrowser(true);
            UniClipboard.SetText(response.url);
            webView.OnShouldClose += (view) => {
                webView = null;
                return true;
            };
            // AppendMessage("AI", resultJson);
        }
    }

    
    void AppendMessage(string sender, string message)
    {
        if (sender == "You")
        {
            GameObject go = Instantiate(UserText,UserText.transform.parent);
            go.SetActive(true);
            go.GetComponentInChildren<TMP_Text>().text = $"\n<b>{sender}:</b> {message}\n";
        }
        else
        {
           
            GameObject go = Instantiate(GPTText, GPTText.transform.parent);
            go.SetActive(true);
            if (userInputField.text.ToLower().Contains("csv"))
            {
                message = GeneralStaticManager.FormatCsvAsTable(message);
                go.transform.GetChild(0).GetComponent<TMP_Text>().textWrappingMode = TextWrappingModes.NoWrap;
            }
            go.transform.GetChild(0).GetComponent<TMP_Text>().text = $"\n<b>{sender}:</b> {message}\n";
        }
        ScrollToBottom();
        // chatOutputText.text += $"\n<b>{sender}:</b> {message}\n";
    }
    public ScrollRect scrollRect;

    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases(); // force layout rebuild
        scrollRect.DOVerticalNormalizedPos(0f, 1f);
    }
    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    public void CallBackendAIAPI()
    {
        Thinking.SetActive(true);
        sendButton.interactable = false;
        AppendMessage("You",userInputField.text);
        AIPromptBody body = new AIPromptBody()
        {
            prompt = userInputField.text,
        };
        string json = JsonConvert.SerializeObject(body);
        APIHandler.instance.Post("UserReport/ProcessAiPrompt",json, onSuccess:(response)=>
        {
            AiPromptResponse aiResponse = JsonConvert.DeserializeObject<AiPromptResponse>(response);
            if (aiResponse.isSuccess)
            {
                Thinking.SetActive(false);
                sendButton.interactable = true;
                Debug.Log("AI Response: "+response);
                if (!string.IsNullOrEmpty(aiResponse.result.result))
                {
                   
                    AppendMessage("AI", aiResponse.result.result);
                }
                else if (!string.IsNullOrEmpty(aiResponse.result.pdf))
                {
                    GameObject go = Instantiate(GPTText,GPTText.transform.parent);
                    go.SetActive(true);
                    go.GetComponentInChildren<TMP_Text>().text = $"\n<b>AI:</b> <color=blue><u>Click to view PDF</u></color> \n";
                    Button viewButton = go.AddComponent<Button>();
                    viewButton.onClick.AddListener(() =>
                    {
                        byte[] pdfBytes = Convert.FromBase64String(aiResponse.result.pdf);
                        string path = Path.Combine(Application.persistentDataPath, $"AIreport{GeneralStaticManager.GenerateRandomName()}.pdf");
                        File.WriteAllBytes(path, pdfBytes);

#if UNITY_EDITOR
                        System.Diagnostics.Process.Start(path);
                        Debug.Log("Is Editor");

#else

                            string url = "file://" + path.Replace(" ", "%20");
                            Debug.Log("URL = " + url);
                            Debug.Log("Persistance = " + path);
                            GeneralStaticManager.OpenFile(path);
#endif
                    });
                }
                else if (!string.IsNullOrEmpty(aiResponse.result.html))
                {
                    GameObject go = Instantiate(GPTText,GPTText.transform.parent);
                    GameObject go2 = go.transform.GetChild(1).gameObject;
                    go2.SetActive(true);
                    go.SetActive(true);
                    go.GetComponentInChildren<TMP_Text>().text = $"\n<b>AI:</b> <color=blue><u>Click to view HTML</u></color> \n";
                    Button viewButton = go.transform.GetChild(0).gameObject.AddComponent<Button>();
                    Button ShareButton = go2.AddComponent<Button>();
                    string path = Path.Combine(Application.persistentDataPath, $"AIreport{GeneralStaticManager.GenerateRandomName()}.html");
                    viewButton.onClick.AddListener( () =>
                    {
                        
                        
                        File.WriteAllTextAsync(path, aiResponse.result.html);
                        var webView = gameObject.AddComponent<UniWebView>();
                        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);

// 2. Load a URL.
                        
                        webView.LoadHTMLString(aiResponse.result.html,"");
                        webView.EmbeddedToolbar.Show();
// 3. Show it. 🎉
                        webView.Show();
// #if UNITY_EDITOR
//                         System.Diagnostics.Process.Start(path);
//                         Debug.Log("Is Editor");
//
// #else
//
//                             string url = "file://" + path.Replace(" ", "%20");
//                             Debug.Log("URL = " + url);
//                             Debug.Log("Persistance = " + path);
//                             GeneralStaticManager.OpenFile(path);
// #endif
                    });
                    ShareButton.onClick.AddListener(() =>
                    {
                        CSVManager.Export(path);
                    });
                }
                ScrollToBottom();
            }
            else
            {
                Thinking.SetActive(false);
                sendButton.interactable = true;
                string reasons = "";
                foreach (var item in aiResponse.serviceErrors)
                {
                    reasons += $"\n {item.code} {item.description}";
                }
                ReferenceManager.instance.PopupManager.Show("AI Error!", $"Reasons are: {reasons}");
            }
        },onError: (error) =>
        {
            Thinking.SetActive(false);
            sendButton.interactable = true;
            ReferenceManager.instance.PopupManager.Show("Something Went Wrong!", $"Reasons are: {error}");
        },true);
    }
    [System.Serializable]
    public class ChatGPTResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    public class AskReponse
    {
        public string url { get; set; }
    }
}