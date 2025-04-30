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
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    // Track full chat history
    private List<Message> conversationHistory = new List<Message>();
    public TMP_Dropdown modelDropdown;
    public GameObject Thinking;
    void Start()
    {
        // Initialize conversation with system prompt
        conversationHistory.Add(new Message
        {
            role = "system",
            content = "You are a helpful Avasci Assistant."
        });

        sendButton.onClick.AddListener(OnSendClicked);
    }

    void OnSendClicked()
    {
        string userMessage = userInputField.text.Trim();
        
        if (string.IsNullOrEmpty(userMessage)) return;

        AppendMessage("You", userMessage);
        userInputField.text = "";

        // Add user message to history
        conversationHistory.Add(new Message
        {
            role = "user",
            content = userMessage
        });

        StartCoroutine(SendMessageToOpenAI());
    }

    IEnumerator SendMessageToOpenAI()
    {
        var payload = new
        {
            model = modelDropdown.captionText.text, // or "gpt-4" if you have access
            messages = conversationHistory
        };
        Thinking.SetActive(true);
        string jsonBody = JsonConvert.SerializeObject(payload);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();
        Thinking.SetActive(false);
        if (request.result != UnityWebRequest.Result.Success)
        {
            
            AppendMessage("Error", request.error);
        }
        else
        {
            string resultJson = request.downloadHandler.text;
            var gptResponse = JsonConvert.DeserializeObject<ChatGPTResponse>(resultJson);
            string reply = gptResponse.choices[0].message.content.Trim();

            // Add GPT reply to history
            conversationHistory.Add(new Message
            {
                role = "assistant",
                content = reply
            });

            AppendMessage("AI", reply);
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
}