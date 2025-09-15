using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DG.Tweening;
using LightBuzz.AvaSci.Csv; // kept to avoid bigger diffs
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Reflection;


public class ChatGPTHandler : MonoBehaviour
{
    [SerializeField] private string apiKey = "";
    [SerializeField] private string openAIEndpoint = "https://api.openai.com/v1/chat/completions";

    [Header("Streaming")]
    [SerializeField] private bool useStreaming = true;
    [SerializeField] private string chatModel = "gpt-4o-mini"; // default model

    [SerializeField] private TMP_Dropdown ReportsDropdown; // optional
    [SerializeField] private CustomDropDown _customDropDown; // your dropdown

    [Header("UI Elements")]
    public TMP_InputField userInputField;
    public GameObject GPTText;   // prefab with TMP_Text at child(0); child(1) optional share button
    public GameObject UserText;  // prefab with TMP_Text at child(0)
    public Button sendButton;
    public ScrollRect scrollRect;
    public GameObject Thinking;

    [Header("External DB Chat (unchanged)")]
    private string apiUrl = "https://www.askyourdatabase.com/api/chatbot/v2/session";
    public string chatBotAPIKey;

    // ================= Chat history =================
    [SerializeField] private int maxHistoryMessages = 24; // (kept for reference, pruning uses char budget)
    [SerializeField] private int maxHistoryChars = 16000;                 // hard cap for total history size
    [SerializeField] private int maxAssistantCharsForHistory = 4000;      // cap a single assistant turn
    [SerializeField] private bool compressLargeAssistantReplies = true;   // store head+tail for big HTML
    private readonly List<Message> conversationHistory = new List<Message>();

    // Cached JSON for writing data.js
    private string TimeBasedJson;
    private string GaitJson;

    // Cached analysis packet for simple Q&A context
    private string _lastAnalysisPacketJson;

    // Stream options
    [SerializeField] private bool showStreamingHtmlInChat = true; // stream raw HTML string into bubble
    [SerializeField] private int streamPreviewChars = 1200;

    // NEW: HTML render toggle settings
    [SerializeField] private bool renderHtmlWithWebView = true; // use UniWebView for rendered view
    [SerializeField] private Vector2 webViewMinSize = new Vector2(300, 200); // min size for panel

    // NEW: stream mode enum
    enum StreamRenderMode { Unknown, Html, Code, Plain }

    void Start() { }

    // ===== Public entry-points =====

    public void AnalyzeCSV(List<string> csvDatas, bool isGait)
    {
        if (!isGait) AnalyzeNormal(csvDatas);
        else AnalyzeGait(csvDatas);
    }

    public void ChangeModel(int value)
    {
        if (value == 1) chatModel = "gpt-4o-mini";
        if (value == 2) chatModel = "gpt-4.1";
    }

    public void TakeOutData()
    {
        Thinking?.SetActive(true);
        AppendMessage("You", userInputField.text);
        AddToHistory("user", userInputField.text);

        var timeBasedReadingRequests = new List<TimeBasedReadingRequest>();
        var gaitReports = new List<GetGaitReportResponse>();

        foreach (var userReportFromDB in _customDropDown.selectedUserReports)
        {
            if (userReportFromDB?.timeBasedReadings != null)
                timeBasedReadingRequests.AddRange(userReportFromDB.timeBasedReadings);

            if (userReportFromDB?.gaitReports != null)
                gaitReports.AddRange(userReportFromDB.gaitReports);
        }

        AnalyzeWholeData(timeBasedReadingRequests, gaitReports);
    }

    /// <summary>Lightweight ask (uses streaming text if it seems like a simple question).</summary>
    public void Ask(string text) => StartCoroutine(SendRequestToChatGPT(text));

    public void OnSendClicked()
    {
        StartCoroutine(SendMessageToOpenAI()); // your external service
    }

    // ===== Prompt builders =====

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

        AddToHistory("user", prompt);
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

        AddToHistory("user", prompt);
        StartCoroutine(SendRequestToChatGPT(prompt));
    }

    public void AnalyzeWholeData(List<TimeBasedReadingRequest> timeBasedReadings,
                                 List<GetGaitReportResponse> gaitReports)
    {
        string timeBasedJson = GeneralStaticManager.ToJsonExcludingNulls(timeBasedReadings);
        string gaitReportJson = GeneralStaticManager.ToJsonExcludingNulls(gaitReports);
        TimeBasedJson = timeBasedJson;
        GaitJson = gaitReportJson;

        // Build compact analysis packet for the AI to reason on
        _lastAnalysisPacketJson = BuildAnalysisPacketJson(timeBasedReadings);

        // Build a prompt that tells the model to use data.js for full rows
        string prompt = BuildReportPrompt(_lastAnalysisPacketJson, !string.IsNullOrEmpty(gaitReportJson));

        // Optional: include any user freeform text at the end
        if (userInputField && !string.IsNullOrEmpty(userInputField.text))
            prompt += "\n\nUSER_NOTES:\n" + userInputField.text;

        AddToHistory("user", prompt);
        StartCoroutine(SendRequestToChatGPT(prompt));
    }

    // ===== Router: decide simple-question vs. report =====
    static bool LooksLikeReportTask(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        s = s.ToLowerInvariant();

        string[] hardHints = {
            "<html", "<!doctype", "chart.js", "chartjs", "canvas>",
            "report", "visualization", "visualisation", "graph", "plot",
            "table view", "bootstrap", "summary section", "key metrics",
            "csv", "json", "timeofreading", "pelvisangle", "varus", "valgus",
            "readings per miliseconds", "gait"
        };
        if (hardHints.Any(h => s.Contains(h))) return true;

        if (s.Length > 1200) return true;
        int braces = s.Count(c => c == '{' || c == '[');
        if (braces >= 3) return true;

        return false;
    }
    static bool IsSimpleQuestion(string s)
    {
        if (string.IsNullOrEmpty(s)) return true;
        if (LooksLikeReportTask(s)) return false;
        bool shortEnough = s.Length <= 400;
        bool fewBraces = s.Count(c => c == '{' || c == '[') == 0;
        return shortEnough && fewBraces;
    }

    IEnumerator SendRequestToChatGPT(string prompt)
    {
        // If the user's raw text is a simple Q, answer conversationally with streaming.
        if (IsSimpleQuestion(userInputField != null ? userInputField.text : prompt))
        {
            Thinking?.SetActive(true);
            // Provide the analysis packet to help the assistant answer about current data,
            // but do NOT dump the giant raw JSON.
            var ctx = string.IsNullOrEmpty(_lastAnalysisPacketJson) ? "" :
                      "\n\nANALYSIS_PACKET:\n" + _lastAnalysisPacketJson;
            yield return StreamChatText($"{(userInputField!=null?userInputField.text:prompt)}{ctx}");
            yield break;
        }

        // Otherwise, treat it as HTML report generation
        bool doStream = useStreaming && Application.platform != RuntimePlatform.WebGLPlayer;

        if (doStream) yield return StreamRequest(prompt);    // streaming HTML to bubble
        else          yield return NonStreamingRequest(prompt);

        ReferenceManager.instance.LoadingManager.Hide();
    }

    // ===== Build messages with history =====
    List<object> BuildMessagesWithHistory(string systemInstruction, string newUserPrompt)
    {
        var msgs = new List<object> { new { role = "system", content = systemInstruction } };

        foreach (var m in conversationHistory)
            msgs.Add(new { role = m.role, content = m.content });

        msgs.Add(new { role = "user", content = newUserPrompt });
        return msgs;
    }

    // ===== Non-streaming HTML response =====
    IEnumerator NonStreamingRequest(string prompt)
    {
        var requestData = new
        {
            model = chatModel,
            messages = BuildMessagesWithHistory(
                "You are an expert physiotherapist and data-visualization report generator. " +
                "The full dataset will be available at runtime from window.__READINGS__ and window.__GAIT__ (via data.js). " +
                "Do NOT inline large raw JSON. Build charts/tables reading from those globals. " +
                "Use the provided ANALYSIS_PACKET values (in the user message) to populate metric cards and narrative.",
                prompt),
            temperature = 0.2f,
            top_p = 1,
            n = 1,
            stream = false
        };

        string jsonBody = JsonConvert.SerializeObject(requestData);

        using (UnityWebRequest request = new UnityWebRequest(openAIEndpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;

                // extract assistant content
                string content = TryExtractContent(responseText);

                // Store assistant turn (compressed if huge)
                AddToHistory("assistant", content, compressIfLarge: true);

                // Save & show using external data.js loader
                CreateHTMLReportWithData(content, TimeBasedJson, GaitJson);
            }
            else
            {
                Debug.LogError("Error: " + request.responseCode + " - " + request.downloadHandler.text);
            }
        }
    }

    // ======= UI components for HTML toggle bubble =======
    class AIBubbleParts
    {
        public GameObject bubbleGO;
        public TMP_Text aiText;
        public Button toggleBtn;
        public TMP_Text toggleLabel;

        // runtime state
        public bool htmlHidden = false;
        public string liveBuffer = "";   // the incoming streamed text
    }

    AIBubbleParts CreateAIBubbleWithToggle()
    {
        var bubble = Instantiate(GPTText, GPTText.transform.parent);
        bubble.SetActive(true);

        var aiText = bubble.transform.GetChild(0).GetComponent<TMP_Text>();
        // //aiText.richText = false; // show the raw html string; avoids TMP tag parsing

        // Toggle button
        var toggleGO = new GameObject("ToggleHtmlTextButton", typeof(RectTransform), typeof(Button), typeof(Image));
        var toggleRT = toggleGO.GetComponent<RectTransform>();
        toggleRT.SetParent(bubble.transform, false);
        toggleRT.anchorMin = new Vector2(1, 0);
        toggleRT.anchorMax = new Vector2(1, 0);
        toggleRT.pivot = new Vector2(1, 0);
        toggleRT.anchoredPosition = new Vector2(-10, 0);
        toggleRT.sizeDelta = new Vector2(160, 36);

        var toggleBtn = toggleGO.GetComponent<Button>();
        var toggleImg = toggleGO.GetComponent<Image>();
        toggleImg.color = new Color(0.078f, 0.0313f, 0.0941f, 1f);

        var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        var labelRT = labelGO.GetComponent<RectTransform>();
        var layoutElement = toggleGO.AddComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;
        labelRT.sizeDelta = new Vector2(160, 36);
        labelRT.SetParent(toggleRT, false);
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;
        var label = labelGO.GetComponent<TextMeshProUGUI>();
        label.text = "Show HTML Text";
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 12; label.fontSizeMax = 24;

        return new AIBubbleParts
        {
            bubbleGO = bubble,
            aiText = aiText,
            toggleBtn = toggleBtn,
            toggleLabel = label,
            htmlHidden = true,
            liveBuffer = "",
        };
    }

    void FitWebViewTo(RectTransform panel, UniWebView web)
    {
        var worldCorners = new Vector3[4];
        panel.GetWorldCorners(worldCorners);
        var bl = RectTransformUtility.WorldToScreenPoint(null, worldCorners[0]);
        var tr = RectTransformUtility.WorldToScreenPoint(null, worldCorners[2]);
        var width = Mathf.Max(webViewMinSize.x, tr.x - bl.x);
        var height = Mathf.Max(webViewMinSize.y, tr.y - bl.y);

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
        web.Frame = new Rect(bl.x, bl.y, width, height);
#endif
    }

    // ===== Streaming HTML (SSE) with detection + toggle =====
    IEnumerator StreamRequest(string prompt)
{
    AddToHistory("user", prompt);

    // Create enhanced bubble with HTML-text toggle
    var parts = CreateAIBubbleWithToggle();
    var aiText = parts.aiText;

    // Detect what’s being generated
    // enum StreamRenderMode { Unknown, Html, Code, Plain }
    var mode = StreamRenderMode.Unknown;
    bool modeLocked = false;

    // Toggle is useful for HTML/code; disable until we’re sure
    parts.toggleBtn.interactable = false;

    // Toggle behavior: show/hide the raw HTML/code text inside the bubble
    parts.toggleBtn.onClick.AddListener(() =>
    {
        parts.htmlHidden = !parts.htmlHidden;
        parts.toggleLabel.text = parts.htmlHidden ? "Show HTML Text" : "Hide HTML Text";

        if (parts.htmlHidden)
        {
            // Keep the buffer but hide it from view
            aiText.text = "<i>(Generating Code)</i>";
        }
        else
        {
            aiText.text = parts.liveBuffer;
        }
        ScrollToBottom();
    });

    var payload = new
    {
        model = chatModel,
        temperature = 0.2f,
        stream = true,
        messages = BuildMessagesWithHistory(
            "You are an expert physiotherapist and data-visualization report generator. " +
            "The full dataset will be available at runtime from window.__READINGS__ and window.__GAIT__ (via data.js). " +
            "Do NOT inline large raw JSON. Build charts/tables reading from those globals. " +
            "Use the provided ANALYSIS_PACKET values (in the user message) to populate metric cards and narrative.\n"+
$@"
You are an expert in data visualization also. Analyze the given data yourself and provide an HTML report with proper graphs and suggestions based on your own observations because you are an expert physiotherapist and report generator. Apply statistical calculations such as mean, standard deviation, percentiles (P5, P50, P95), and trends if necessary and provide your findings and observations in the readings as a paragraph at the end of the HTML file. Keep the HTML beautiful by adding styles.

IMPORTANT NOTE: 
1. Never truncate the data from JSON. Show all of them in the report. Never ask the user to put data in the HTML; include the data shared with you yourself.
2. If the dataset is too large, consider displaying only the first few rows and summarizing the rest, or paginate the data to improve performance.

Ensure that the HTML includes all the provided data directly. 
**Always Include All the data provided in the json**
Do **not** include any placeholder text such as ""Data not available"" or truncation like `/* ... */` or // ... all other rows ...
when the data exists in the provided JSON. If data is missing, simply leave it empty or use a note like ""No data available"".

Generate a **fully formatted HTML report** based on the provided data in JSONs.
Requirements:
- Use clear headings and subheadings.
- Use tables for numeric results (with borders, alternating row colors, and padding).
- Use bullet points for observations.
- Make it visually appealing using inline CSS styles or Bootstrap classes if allowed.
- Include a summary section at the top.
- Use `<h1>`, `<h2>`, `<h3>` appropriately.
- Do NOT escape HTML or wrap it in code blocks—return pure HTML.
Design requirements:
- Full, valid HTML document (<!DOCTYPE html>…)
- Responsive layout with a centered container (max-width ~1100px), fluid spacing, large headings
- Hero header with subject + summary stats
- Use a clean, neutral palette (soft gray background, white cards, subtle shadows, rounded corners)
- Typography: sans-serif, 16px base, 28–36px H1, clear hierarchy; line-height 1.5
- Components: card grid for key metrics, sticky section headings, tasteful dividers
- Tables: zebra stripes, fixed header, hover row highlight
- Buttons/chips: subtle accent for tags like “preop / postop”
- Accessibility: ensure color contrast, semantic tags, alt text, and aria-labels for images

Chart rules:
- Labels, legends, and axes must be clear and legible.
- Put all JavaScript in `window.onload` to initialize the charts.
- Always include graphs/charts for the readings you are provided, as well as charts for key metrics if raw rows are present.
- Include all rows from the provided data JSONs in the HTML, do not truncate data like /* ... all other rows ... */.",
            prompt)
    };

    var json = JsonConvert.SerializeObject(payload);
    var bytes = Encoding.UTF8.GetBytes(json);

    var queue = new ConcurrentQueue<string>();
    var dl = new SseDownloadHandler(queue);
    var req = new UnityWebRequest(openAIEndpoint, "POST")
    {
        uploadHandler = new UploadHandlerRaw(bytes),
        downloadHandler = dl
    };
    req.SetRequestHeader("Authorization", "Bearer " + apiKey);
    req.SetRequestHeader("Content-Type", "application/json");
    req.SetRequestHeader("Accept", "text/event-stream");

    var op = req.SendWebRequest();

    var htmlBuilder = new StringBuilder();
    bool done = false;

    float nextUiUpdate = 0f;
    const float uiUpdateInterval = 0.06f; // ~16 fps

    while (!op.isDone)
    {
        while (queue.TryDequeue(out var evt))
        {
            if (evt == "[DONE]") { done = true; break; }

            try
            {
                var jo = JObject.Parse(evt);
                var delta = jo["choices"]?[0]?["delta"]?["content"]?.ToString();
                if (!string.IsNullOrEmpty(delta))
                {
                    Thinking?.SetActive(false);
                    htmlBuilder.Append(delta);

                    // Detect stream mode from early tokens and lock once known
                    if (!modeLocked)
                    {
                        var peek = htmlBuilder.ToString();
                        if (peek.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) ||
                            peek.StartsWith("<html", StringComparison.OrdinalIgnoreCase) ||
                            peek.Contains("<head", StringComparison.OrdinalIgnoreCase))
                        {
                            mode = StreamRenderMode.Html;
                            modeLocked = true;
                            parts.toggleBtn.interactable = true; // allow show/hide for HTML
                            parts.toggleLabel.text = "Hide HTML Text";
                            //aiText.richText = false; // keep raw html visible
                        }
                        else if (peek.StartsWith("```"))
                        {
                            mode = StreamRenderMode.Code;
                            modeLocked = true;
                            parts.toggleBtn.interactable = true; // also useful for long code
                            parts.toggleLabel.text = "Hide Code Text";
                            //aiText.richText = false;
                        }
                        else if (peek.Length > 200)
                        {
                            mode = StreamRenderMode.Plain;
                            modeLocked = true;
                            // plain answers: we can still let user hide, but default off to avoid confusion
                            parts.toggleBtn.interactable = false;
                            aiText.richText = true;
                        }
                    }

                    // Update buffer and UI (respect hidden state)
                    parts.liveBuffer = htmlBuilder.ToString();
                    if (Time.unscaledTime >= nextUiUpdate)
                    {
                        if (!parts.htmlHidden) aiText.text = parts.liveBuffer;
                        ScrollToBottom();
                        nextUiUpdate = Time.unscaledTime + uiUpdateInterval;
                    }
                }

                var finish = jo["choices"]?[0]?["finish_reason"]?.ToString();
                if (!string.IsNullOrEmpty(finish) && finish != "null")
                {
                    done = true;
                    break;
                }
            }
            catch { /* ignore partial lines */ }
        }

        if (done) break;
        yield return null;
    }

    if (req.result != UnityWebRequest.Result.Success && !done)
    {
        aiText.text = "\nAI: Failed to generate report.\n";
        ScrollToBottom();
        Debug.LogError($"HTTP {req.responseCode}: {req.error}\n{req.downloadHandler?.text}");
        yield break;
    }

    // Finalize: clean fences, save, and keep full HTML in the bubble buffer
    var html = CleanHtmlFences(htmlBuilder.ToString());
    AddToHistory("assistant", html, compressIfLarge: true);
    CreateHTMLReportWithData(html, TimeBasedJson, GaitJson);

    parts.liveBuffer = html;
    if (!parts.htmlHidden) aiText.text = parts.liveBuffer;
    ScrollToBottom();

    // Cleanup selection
    if (_customDropDown != null)
    {
        _customDropDown.selectedItems.Clear();
        _customDropDown.items.ForEach(x => x.myToggle.isOn = false);
    }
}


    // ===== Streaming plain text for simple questions =====
    IEnumerator StreamChatText(string userText)
    {
        // History/user bubble already handled by caller sometimes, ensure we add here:
        AddToHistory("user", userText);

        GameObject aiBubble = AppendMessage("AI", "");
        var aiText = aiBubble.transform.GetChild(0).GetComponent<TMP_Text>();
        aiText.richText = true;

        // Include compact packet if available so answers can reference current dataset
        string helperContext = string.IsNullOrEmpty(_lastAnalysisPacketJson)
            ? ""
            : "\n\nANALYSIS_PACKET (summary of current data):\n" + _lastAnalysisPacketJson;

        var payload = new
        {
            model = chatModel,
            temperature = 0.2f,
            stream = true,
            messages = BuildMessagesWithHistory(
                "You are a helpful assistant. Suggest: 'Open the left dropdown, select captures, and I will generate reports.' " +
                "If ANALYSIS_PACKET is present in the user message, provide clinical-grade insights and suggestions like a physiotherapist, in proper detail. Avoid HTML unless asked. Also be a little friendly in your response",
                userText + helperContext)
        };

        var req = new UnityWebRequest(openAIEndpoint, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))),
            downloadHandler = new SseDownloadHandler(new ConcurrentQueue<string>())
        };
        req.SetRequestHeader("Authorization", "Bearer " + apiKey);
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Accept", "text/event-stream");

        var op = req.SendWebRequest();

        var sse = (SseDownloadHandler)req.downloadHandler;
        var queue = sse.Queue;

        var full = new StringBuilder();
        while (!op.isDone)
        {
            while (queue.TryDequeue(out var evt))
            {
                if (evt == "[DONE]") { break; }

                try
                {
                    var jo = JObject.Parse(evt);
                    var delta = jo["choices"]?[0]?["delta"]?["content"]?.ToString();
                    if (!string.IsNullOrEmpty(delta))
                    {
                        Thinking?.SetActive(false);
                        full.Append(delta);
                        aiText.text = $"\n<b>AI:</b> {full}\n";
                        ScrollToBottom();
                    }
                }
                catch { /* partial lines */ }
            }
            yield return null;
        }

        if (req.result != UnityWebRequest.Result.Success)
        {
            AppendMessage("AI",
                "Selected model cannot handle this large amount of data. Please select the Accuracy Model from top left menu to handle large data");
            Thinking?.SetActive(false);
        }

        // Finalize assistant message into history
        var finalReply = full.ToString();
        Thinking?.SetActive(false);
        AddToHistory("assistant", finalReply);
    }

    // ===== Utilities =====

    static string CleanHtmlFences(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = s.Trim();
        if (s.StartsWith("```"))
        {
            s = s.Replace("```html", "")
                 .Replace("```HTML", "")
                 .Replace("```", "")
                 .Trim();
        }
        return s;
    }

    string TryExtractContent(string jsonResponse)
    {
        try
        {
            var jo = JObject.Parse(jsonResponse);
            var content = jo["choices"]?[0]?["message"]?["content"]?.ToString();
            if (!string.IsNullOrEmpty(content))
                return CleanHtmlFences(content);
        }
        catch { /* fallback */ }

        var responseData = JsonUtility.FromJson<ChatGPTResponse>(jsonResponse);
        if (responseData?.choices != null &&
            responseData.choices.Length > 0 &&
            responseData.choices[0]?.message != null)
        {
            return CleanHtmlFences(responseData.choices[0].message.content);
        }

        Debug.LogWarning("OpenAI response parse failed.");
        return "<!doctype html><html><body><p>Failed to parse response.</p></body></html>";
    }

    // ======== Hybrid output: HTML + data.js (full dataset) ========
    public void CreateHTMLReportWithData(string html, string readingsJson, string gaitJson)
    {
        var folder = Application.persistentDataPath;
        var htmlPath = Path.Combine(folder, "Report.html");
        var dataJsPath = Path.Combine(folder, "data.js");

        // Ensure data.js is referenced
        if (!string.IsNullOrEmpty(html) && !html.Contains("data.js", StringComparison.OrdinalIgnoreCase))
        {
            int headClose = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
            string tag = "\n<script src=\"data.js\"></script>\n";
            if (headClose >= 0) html = html.Insert(headClose, tag);
            else html += tag;
        }

        // Write data.js with full arrays
        var safeReadings = string.IsNullOrEmpty(readingsJson) ? "[]" : readingsJson;
        var safeGait = string.IsNullOrEmpty(gaitJson) ? "[]" : gaitJson;
        var js = $"window.__READINGS__={safeReadings};\nwindow.__GAIT__={safeGait};\n";
        File.WriteAllText(dataJsPath, js);

        // Save HTML
        File.WriteAllText(htmlPath, html);

#if UNITY_EDITOR
        System.Diagnostics.Process.Start(htmlPath);
#else
        // If you use UniWebView, pass baseUrl so relative data.js resolves
        var webView = gameObject.AddComponent<UniWebView>();
        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
        string baseUrl = "file://" + folder.Replace(" ", "%20") + "/";
        webView.LoadHTMLString(html, baseUrl);
        webView.EmbeddedToolbar.Show();
        webView.Show();
#endif
        Debug.Log("HTML Report + data.js saved to: " + folder);
    }

    // Legacy single-file writer (kept if you still need it elsewhere)
    public void CreateHTMLReport(string reportContent)
    {
        string path = Application.persistentDataPath + "/Reportt.html";
        File.WriteAllText(path, reportContent);
#if UNITY_EDITOR
        System.Diagnostics.Process.Start(path);
#else
        string url = "file://" + path.Replace(" ", "%20");
        GeneralStaticManager.OpenFile(path);
#endif
        Debug.Log("HTML Report saved at: " + path);
    }

    GameObject AppendMessage(string sender, string message)
    {
        if (sender == "You")
        {
            GameObject go = Instantiate(UserText, UserText.transform.parent);
            go.SetActive(true);
            go.GetComponentInChildren<TMP_Text>().text = $"\n<b>{sender}:</b> {message}\n";
            ScrollToBottom();
            return go;
        }
        else
        {
            GameObject go = Instantiate(GPTText, GPTText.transform.parent);
            go.SetActive(true);
            if (userInputField != null && userInputField.text.ToLower().Contains("csv"))
            {
                message = GeneralStaticManager.FormatCsvAsTable(message);
                go.transform.GetChild(0).GetComponent<TMP_Text>().textWrappingMode = TextWrappingModes.NoWrap;
            }
            go.transform.GetChild(0).GetComponent<TMP_Text>().text = $"\n<b>{sender}:</b> {message}\n";
            ScrollToBottom();
            return go;
        }
    }

    // Smooth, reliable "stick to bottom" without bouncing
    private Coroutine _scrollRoutine;
    void ScrollToBottom()
    {
        if (scrollRect == null) return;

        if (_scrollRoutine != null)
        {
            StopCoroutine(_scrollRoutine);
            _scrollRoutine = null;
        }
        _scrollRoutine = StartCoroutine(ScrollToBottomCo());
    }
    IEnumerator ScrollToBottomCo()
    {
        scrollRect.StopMovement();
        yield return null; // wait for layout
        Canvas.ForceUpdateCanvases();
        if (scrollRect.content) LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        // Force to bottom a few frames to avoid “bounce”
        scrollRect.verticalNormalizedPosition = 0f;
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        yield return null;
        scrollRect.verticalNormalizedPosition = 0f;

        _scrollRoutine = null;
    }

    // ===== External DB Chat - unchanged =====

    IEnumerator SendMessageToOpenAI()
    {
        var payload = new
        {
            chatbotid = "2b18e064b48ec7fd9260c55094072c64",
            name = "Joe",
            email = "joe@avasci.com"
        };
        Thinking?.SetActive(true);
        ReferenceManager.instance.LoadingManager.Show("Connecting to Live Database");
        string jsonBody = JsonConvert.SerializeObject(payload);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + chatBotAPIKey);

        yield return request.SendWebRequest();
        Thinking?.SetActive(false);
        ReferenceManager.instance.LoadingManager.Hide();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.result + " - " + request.downloadHandler.text);
        }
        else
        {
            string resultJson = request.downloadHandler.text;
            var response = JsonConvert.DeserializeObject<AskReponse>(resultJson);
            var webView = gameObject.AddComponent<UniWebView>();
            webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            webView.Load(response.url);
            webView.Show();
            webView.EmbeddedToolbar.Show();
            webView.BackgroundColor = Color.black;
            webView.SetOpenLinksInExternalBrowser(true);
            UniClipboard.SetText(response.url);
            webView.OnShouldClose += (view) => { webView = null; return true; };
        }
    }
    [Header("UniWebView")]
    [SerializeField] private UniWebView webView;
    [SerializeField] private string juliusUrl = "https://julius.ai/chat"; // Update if needed

    // [Header("Data")]
    // public List<UserReportData> userReports = new List<UserReportData>();

    [Header("Behavior")]
    [Tooltip("Click the (+) upload button before attaching files.")]
    public bool clickUploadButtonFirst = true;

    [Tooltip("Wait after page load before injecting (ms).")]
    public int initialDelayMs = 500;

    [Tooltip("Retries if DOM isn't ready.")]
    public int attachRetries = 5;

    [Tooltip("Wait between retries (ms).")]
    public int retryDelayMs = 500;

    [Header("Chunking")]
    [Tooltip("If JSON length exceeds this many chars, split into parts. ~1 char ≈ 1 byte for ASCII.")]
    public int maxJsonCharsPerFile = 4_000_000; // ~4 MB chunks

    [Tooltip("Base filename without extension.")]
    public string baseFileName = "UserReports";

    [Header("Chunked Paste Fallback")]
    public bool enablePasteFallback = true;
    public int pasteChunkChars = 50000; // 50k chars per message
    public float pasteInterChunkDelay = 0.25f; // seconds

    private bool injected;
    private string lastResult = "";
    public void OpenJulius()
    {
        if (_customDropDown.selectedUserReports.Count == 0)
        {
            ReferenceManager.instance.PopupManager.Show("No Report","Please Select a report from the dropdown so that AI can analyze it.");
            return;
        }

        injected = false;
        webView = gameObject.AddComponent<UniWebView>();
        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
        webView.EmbeddedToolbar.Show();
        webView.BackgroundColor = Color.black;
        webView.SetOpenLinksInExternalBrowser(false);


        webView.SetSupportMultipleWindows(true,true);
        



        webView.OnShouldClose += (view) => { webView = null; return true; };
        webView.OnShouldClose += (view) => { webView = null; return true; };

        if (webView == null)
        {
            Debug.LogError("[Julius] UniWebView is not assigned.");
            return;
        }

        // auth-ready scheme + handlers
        webView.AddUrlScheme("uniwebview");
        webView.OnMessageReceived += OnMessageReceived;

        webView.OnPageFinished += OnPageFinished;
        webView.Load(juliusUrl);
        webView.Show(); 
    }
    private void OnPageFinished(UniWebView view, int code, string url)
    {
        if (injected) return;
        injected = true;
        StartCoroutine(BootstrapFlow());
    }
    private IEnumerator BootstrapFlow()
    {
        // Small delay so initial DOM mounts
        yield return new WaitForSeconds(initialDelayMs / 1000f);

        // Try to detect if app is already ready
        yield return Eval(JS_PROBE_APP);
        if (lastResult.StartsWith("APP_READY"))
        {
            StartCoroutine(AttachFlow());
            yield break;
        }

        // Install page-side watcher to ping uniwebview://auth-ready when chat mounts
        yield return Eval(JS_INSTALL_LOGIN_WATCHER);

        // Unity-side polling fallback (fires even if custom-scheme is blocked)
        StartCoroutine(WaitForAppReadyThenUpload());
    }
    private IEnumerator WaitForAppReadyThenUpload()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            yield return Eval(JS_PROBE_APP);
            if (lastResult.StartsWith("APP_READY"))
            {
                Debug.Log("[Julius] Poller detected app ready. Starting attach flow.");
                StartCoroutine(AttachFlow());
                yield break;
            }
        }
    }
    private void OnMessageReceived(UniWebView view, UniWebViewMessage msg)
    {
        if (msg.Scheme != "uniwebview") return;
        if (msg.Path == "auth-ready")
        {
            Debug.Log("[Julius] auth-ready received. Starting attach flow.");
            StartCoroutine(AttachFlow());
        }
    }
    

    [ContextMenu("Check Json")]
    public void TestMethod()
    {
        var json = JsonConvert.SerializeObject(_customDropDown.selectedUserReports.Select(x=>x.mydata).ToList());
    }
    private IEnumerator AttachFlow()
    {
        // Serialize compact JSON from your selected reports
        var json = JsonConvert.SerializeObject(
            _customDropDown.selectedUserReports.Select(x => x.mydata).ToList() ?? new List<UserReportData>(),
            new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            }
        );

        // Optionally click "+" to init uploader
        if (clickUploadButtonFirst)
        {
            yield return Eval(JS_CLICK_PLUS);
        }

        // Wait up to 5s for any file input (deep search + shadow)
        yield return Eval(JS_WAIT_FOR_FILE_INPUT);

        // Split into parts if large
        var parts = new List<string>();
        parts.Add(json);
        bool success = false;

        for (int attempt = 0; attempt < attachRetries && !success; attempt++)
        {
            success = true;

            for (int i = 0; i < parts.Count; i++)
            {
                string name = parts.Count == 1
                    ? $"{baseFileName}.json"
                    : $"{baseFileName}.part{(i + 1).ToString("00")}.json";

                yield return AttachJsonAsFile(parts[i], name);

                if (!lastResult.StartsWith("FILE_ATTACHED"))
                {
                    success = false;
                    yield return new WaitForSeconds(retryDelayMs / 1000f);
                    break;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }

        if (success)
        {
            Debug.Log($"[Julius] Uploaded {parts.Count} JSON file(s) successfully.");
        }
        else
        {
#if UNITY_IOS || UNITY_TVOS || UNITY_VISIONOS
            Debug.LogWarning("[Julius] File attach blocked by WKWebView. Falling back to chunked paste...");
            if (enablePasteFallback) { yield return PasteJsonInChunks(json); }
            else { Debug.LogError("[Julius] Paste fallback disabled; cannot deliver JSON."); }
#else
            Debug.LogWarning("[Julius] File attach failed. Falling back to chunked paste...");
            if (enablePasteFallback) { yield return PasteJsonInChunks(json); }
            else { Debug.LogError("[Julius] Paste fallback disabled; cannot deliver JSON."); }
#endif
        }
    }
    private IEnumerator PasteJsonInChunks(string json)
    {
        string lead = "I am sending JSON data in chunks. Combine them by order. When I send 'END_JSON', start processing.";
        yield return PasteMessage(lead);

        int total = json.Length, idx = 0, part = 1;
        while (idx < total)
        {
            int len = Math.Min(pasteChunkChars, total - idx);
            string chunk = json.Substring(idx, len);
            yield return PasteMessage($"[PART {part}] {chunk}");
            idx += len; part++;
            yield return new WaitForSeconds(pasteInterChunkDelay);
        }
        yield return PasteMessage("END_JSON");
        Debug.Log($"[Julius] Pasted JSON in {part - 1} chunk(s).");
    }
     private IEnumerator AttachJsonAsFile(string json, string fileName)
    {
        string b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        string js = $@"
            (function(){{
              try {{
                function allFileInputsDeep(root){{
                  const list=[], stack=[root||document];
                  while(stack.length){{
                    const n=stack.pop();
                    if(!n) continue;
                    if(n.querySelectorAll) n.querySelectorAll('input[type=""file""]').forEach(el=>list.push(el));
                    if(n.shadowRoot) stack.push(n.shadowRoot);
                    if(n.children) for(let i=0;i<n.children.length;i++) stack.push(n.children[i]);
                  }}
                  return list;
                }}

                var inputs = allFileInputsDeep();
                var input = document.getElementById('file') || inputs[0];
                if(!input) return 'FILE_INPUT_NOT_FOUND';

                var bin = atob('{b64}');
                var len = bin.length;
                var bytes = new Uint8Array(len);
                for (var i=0;i<len;i++) bytes[i] = bin.charCodeAt(i);

                var file = new File([bytes], '{EscapeJs(fileName)}', {{ type:'application/json' }});

                if (typeof DataTransfer === 'undefined') return 'ERROR:DataTransfer_unsupported';

                var dt = new DataTransfer();
                dt.items.add(file);

                try {{
                  input.files = dt.files; // works on Android/desktop; blocked on iOS/WKWebView
                }} catch(e) {{
                  return 'ERROR:assign_files_blocked';
                }}

                input.dispatchEvent(new Event('change', {{ bubbles:true }}));
                return 'FILE_ATTACHED(' + file.name + ', ' + (file.size||0) + ' bytes)';
              }} catch(e) {{
                return 'ERROR:' + (e && e.message || e);
              }}
            }})();
        ";

        yield return Eval(js);
    }
    private IEnumerator PasteMessage(string text)
    {
        string b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        string js = $@"
            (function(){{
              try {{
                var t = decodeURIComponent(escape(atob('{b64}')));
                var box = document.getElementById('chat-input-box') || document.querySelector('textarea,[contenteditable=""true""]');
                if (!box) return 'INPUT_NOT_FOUND';

                if (box.tagName === 'TEXTAREA') {{
                  box.focus(); box.value = t;
                  box.dispatchEvent(new InputEvent('input', {{ bubbles:true }}));
                }} else {{
                  box.focus(); box.innerText = t;
                  box.dispatchEvent(new InputEvent('input', {{ bubbles:true }}));
                }}

                var btn = document.querySelector('button[type=""submit""]');
                if (btn) {{ btn.click(); return 'SENT_BY_BUTTON'; }}
                box.dispatchEvent(new KeyboardEvent('keydown', {{ key:'Enter', bubbles:true }}));
                return 'SENT_BY_ENTER';
              }} catch(e) {{
                return 'ERROR:' + (e && e.message || e);
              }}
            }})();
        ";
        yield return Eval(js);
        if (!lastResult.StartsWith("SENT_"))
            Debug.LogWarning("[Julius] PasteMessage result: " + lastResult);
    }

    // ---- JS eval helper (newer UniWebView payload signature) ----
    private IEnumerator Eval(string js)
    {
        bool done = false; lastResult = "";

        try
        {
            webView.EvaluateJavaScript(js, (UniWebViewNativeResultPayload payload) => {
                lastResult = string.IsNullOrEmpty(payload.data) ? payload.resultCode.ToString() : payload.data;
                done = true;
            });
        }
        catch
        {
            // webView.EvaluateJavaScript(js, (string s) => {
            //     lastResult = s ?? "";
            //     done = true;
            // });
        }

        while (!done) yield return null;
        // Debug.Log("[Eval] " + lastResult);
    }

    private string EscapeJs(string s) => (s ?? "").Replace("\\", "\\\\").Replace("'", "\\'");
    private const string JS_PROBE_APP = @"
(function(){
  try {
    if (document.querySelector('#chat-input-box') ||
        document.querySelector('form[data-cy=""chat-input-form""]')) return 'APP_READY';
    var sel=['input[type=""email""]','input[type=""password""]','button[type=""submit""]',
             'input[name=""email""]','input[name=""password""]'];
    for (var i=0;i<sel.length;i++){ if (document.querySelector(sel[i])) return 'LOGIN_SCREEN'; }
    return 'UNKNOWN';
  } catch(e){ return 'ERROR:'+ (e&&e.message||e); }
})();";

    private const string JS_INSTALL_LOGIN_WATCHER = @"
(function(){
  try{
    if (window.__unityAuthWatchInstalled) return 'WATCH_EXISTS';
    function appReady(){
      return !!(document.querySelector('#chat-input-box') ||
                document.querySelector('form[data-cy=""chat-input-form""]'));
    }
    if (appReady()){
      location.href = 'uniwebview://auth-ready';
      return 'ALREADY_READY';
    }
    var observer = new MutationObserver(function(){
      if (appReady()){
        observer.disconnect();
        // Try scheme (may be blocked on some CSPs, polling will still catch it)
        try { location.href = 'uniwebview://auth-ready'; } catch(_){}
      }
    });
    observer.observe(document.documentElement,{childList:true,subtree:true});
    window.__unityAuthWatchInstalled = true;
    return 'WATCH_INSTALLED';
  }catch(e){ return 'ERROR:'+ (e&&e.message||e); }
})();";


    private const string JS_CLICK_PLUS = @"
        (function(){
          try{
            var btn = document.querySelector('[data-event=""chat_input_new_file""]');
            if (btn){ btn.click(); return 'PLUS_CLICKED'; }
            return 'PLUS_NOT_FOUND';
          }catch(e){ return 'ERROR:'+ (e&&e.message||e); }
        })();
    ";

    private const string JS_WAIT_FOR_FILE_INPUT = @"
        (function(){
          try{
            function allFileInputsDeep(root){
              const list=[], stack=[root||document];
              while(stack.length){
                const n=stack.pop();
                if(!n) continue;
                if(n.querySelectorAll) n.querySelectorAll('input[type=""file""]').forEach(el=>list.push(el));
                if(n.shadowRoot) stack.push(n.shadowRoot);
                if(n.children) for(let i=0;i<n.children.length;i++) stack.push(n.children[i]);
              }
              return list;
            }
            const found = allFileInputsDeep();
            if (found.length) return 'FILE_INPUT_FOUND_INITIAL:'+found.length;
            return new Promise(resolve=>{
              const timeout=setTimeout(()=>{ observer.disconnect(); resolve('FILE_INPUT_NOT_FOUND'); }, 5000);
              const observer=new MutationObserver(()=>{
                const f=allFileInputsDeep();
                if (f.length){ clearTimeout(timeout); observer.disconnect(); resolve('FILE_INPUT_FOUND_OBSERVER:'+f.length); }
              });
              observer.observe(document.documentElement,{childList:true,subtree:true});
            });
          }catch(e){ return 'ERROR:'+ (e&&e.message||e); }
        })();
    ";
   
    private void OnDestroy()
    {
        if (webView != null)
        {
            webView.OnPageFinished -= OnPageFinished;
            webView.OnMessageReceived -= OnMessageReceived;
        }
    }
    public void CallBackendAIAPI()
    {
        Thinking?.SetActive(true);
        sendButton.interactable = false;
        AppendMessage("You", userInputField.text);
        AddToHistory("user", userInputField.text);

        AIPromptBody body = new AIPromptBody() { prompt = userInputField.text };
        string json = JsonConvert.SerializeObject(body);
        APIHandler.instance.Post("UserReport/ProcessAiPrompt", json, onSuccess: (response) =>
        {
            AiPromptResponse aiResponse = JsonConvert.DeserializeObject<AiPromptResponse>(response);
            if (aiResponse.isSuccess)
            {
                Thinking?.SetActive(false);
                sendButton.interactable = true;
                if (!string.IsNullOrEmpty(aiResponse.result.result))
                {
                    AppendMessage("AI", aiResponse.result.result);
                    AddToHistory("assistant", aiResponse.result.result);
                }
                else if (!string.IsNullOrEmpty(aiResponse.result.pdf))
                {
                    GameObject go = Instantiate(GPTText, GPTText.transform.parent);
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
#else
                        string url = "file://" + path.Replace(" ", "%20");
                        GeneralStaticManager.OpenFile(path);
#endif
                    });
                    AddToHistory("assistant", "[[PDF_GENERATED]]");
                }
                else if (!string.IsNullOrEmpty(aiResponse.result.html))
                {
                    GameObject go = Instantiate(GPTText, GPTText.transform.parent);
                    GameObject go2 = go.transform.GetChild(1).gameObject;
                    go2.SetActive(true);
                    go.SetActive(true);
                    go.GetComponentInChildren<TMP_Text>().text = $"\n<b>AI:</b> <color=blue><u>Click to view HTML</u></color> \n";
                    Button viewButton = go.transform.GetChild(0).gameObject.AddComponent<Button>();
                    Button ShareButton = go2.AddComponent<Button>();
                    string path = Path.Combine(Application.persistentDataPath, $"AIreport{GeneralStaticManager.GenerateRandomName()}.html");
                    viewButton.onClick.AddListener(() =>
                    {
                        File.WriteAllTextAsync(path, aiResponse.result.html);
                        var webView = gameObject.AddComponent<UniWebView>();
                        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
                        webView.LoadHTMLString(aiResponse.result.html, "");
                        webView.EmbeddedToolbar.Show();
                        webView.Show();
                    });
                    ShareButton.onClick.AddListener(() =>
                    {
                        CSVManager.Export(path);
                    });
                    AddToHistory("assistant", "[[HTML_REPORT_GENERATED]]");
                }
                ScrollToBottom();
            }
            else
            {
                Thinking?.SetActive(false);
                sendButton.interactable = true;
                string reasons = "";
                foreach (var item in aiResponse.serviceErrors)
                    reasons += $"\n {item.code} {item.description}";

                ReferenceManager.instance.PopupManager.Show("AI Error!", $"Reasons are: {reasons}");
            }
        }, onError: (error) =>
        {
            Thinking?.SetActive(false);
            sendButton.interactable = true;
            ReferenceManager.instance.PopupManager.Show("Something Went Wrong!", $"Reasons are: {error}");
        }, true);
    }

    // ====== Data models / helpers ======

    [Serializable] public class Message { public string role; public string content; }
    [Serializable] public class ChatGPTResponse { public Choice[] choices; }
    [Serializable] public class Choice { public Message message; }
    public class AskReponse { public string url { get; set; } }

    [Serializable] public class AIPromptBody { public string prompt; }
    [Serializable] public class AiServiceError { public string code; public string description; }
    [Serializable] public class AiPromptResult { public string result; public string pdf; public string html; }
    [Serializable] public class AiPromptResponse { public bool isSuccess; public AiPromptResult result; public List<AiServiceError> serviceErrors; }

    // ===== SSE Download Handler =====
    private class SseDownloadHandler : DownloadHandlerScript
    {
        public ConcurrentQueue<string> Queue { get; }
        private readonly StringBuilder _buf = new StringBuilder();

        public SseDownloadHandler(ConcurrentQueue<string> queue) : base() { Queue = queue; }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0) return true;
            var chunk = Encoding.UTF8.GetString(data, 0, dataLength);
            _buf.Append(chunk);

            string s = _buf.ToString();
            int idx;
            while ((idx = IndexOfBoundary(s)) >= 0)
            {
                var block = s.Substring(0, idx);
                s = s.Substring(idx + 2);
                foreach (var line in block.Replace("\r", "").Split('\n'))
                {
                    var l = line;
                    if (string.IsNullOrEmpty(l)) continue;
                    if (l.StartsWith(":")) continue;
                    if (l.StartsWith("data:"))
                    {
                        var payload = l.Substring(5).Trim();
                        Queue.Enqueue(payload);
                    }
                }
            }
            _buf.Length = 0;
            _buf.Append(s);
            return true;
        }

        private static int IndexOfBoundary(string s)
        {
            var i = s.IndexOf("\n\n", StringComparison.Ordinal);
            if (i >= 0) return i;
            i = s.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            return i >= 0 ? i : -1;
        }

        protected override void CompleteContent() { }
        protected override float GetProgress() => 0f;
    }

    // ===================== Analysis packet (compact stats) =====================
    [Serializable]
    private class VarStats
    {
        public string name;
        public int count, missing;
        public double mean, sd, p5, p50, p95, min, max;
        public double trendSlopePerSec;
        public object minAt, maxAt;
    }
    [Serializable]
    private class AnalysisPacket
    {
        public int totalRows;
        public double timeMin, timeMax, duration;
        public string[] variables;
        public List<VarStats> stats;
        public List<Dictionary<string, object>> sampleRows;
    }

    static IEnumerable<float> GetSeries(List<TimeBasedReadingRequest> rows, System.Reflection.PropertyInfo p)
    {
        foreach (var r in rows)
        {
            var v = p.GetValue(r) as float?;
            if (v.HasValue) yield return v.Value;
        }
    }
    static double Mean(IList<double> a) => a.Count == 0 ? double.NaN : a.Average();
    static double SD(IList<double> a)
    {
        if (a.Count < 2) return double.NaN;
        var m = a.Average();
        double sum = 0; for (int i = 0; i < a.Count; i++) { var d = a[i] - m; sum += d * d; }
        return Math.Sqrt(sum / (a.Count - 1));
    }
    static double Percentile(IList<double> a, double p)
    {
        if (a.Count == 0) return double.NaN;
        var b = a.OrderBy(x => x).ToList();
        var pos = (p / 100.0) * (b.Count - 1);
        var lo = (int)Math.Floor(pos);
        var hi = (int)Math.Ceiling(pos);
        if (lo == hi) return b[lo];
        var frac = pos - lo;
        return b[lo] + frac * (b[hi] - b[lo]);
    }
    static (double slope, double intercept) LinReg(IList<double> x, IList<double> y)
    {
        int n = Math.Min(x.Count, y.Count);
        if (n < 2) return (double.NaN, double.NaN);
        double sx = 0, sy = 0, sxx = 0, sxy = 0;
        for (int i = 0; i < n; i++) { sx += x[i]; sy += y[i]; sxx += x[i] * x[i]; sxy += x[i] * y[i]; }
        var denom = n * sxx - sx * sx; if (Math.Abs(denom) < 1e-9) return (double.NaN, double.NaN);
        var slope = (n * sxy - sx * sy) / denom;
        var intercept = (sy - slope * sx) / n;
        return (slope, intercept);
    }
    static List<Dictionary<string, object>> StratifiedSample(List<TimeBasedReadingRequest> rows, int target)
    {
        var res = new List<Dictionary<string, object>>();
        if (rows == null || rows.Count == 0) return res;
        int n = rows.Count;
        target = Math.Min(target, n);
        for (int i = 0; i < target; i++)
        {
            int idx = (int)Math.Round(i * (n - 1.0) / Math.Max(1, target - 1));
            var r = rows[idx];
            res.Add(new Dictionary<string, object> {
                ["timeOfReading"] = r.TimeOfReading,
                ["pelvisAngle"] = r.PelvisAngle,
                ["varusValgusRight"] = r.VarusValgusRight,
                ["varusValgusLeft"] = r.VarusValgusLeft,
                ["hipLeftAbduction"] = r.HipLeftAbduction,
                ["hipRightAbduction"] = r.HipRightAbduction,
                ["ankleHipLeftAbductionDifference"] = r.AnkleHipLeftAbductionDifference,
                ["ankleHipRightAbductionDifference"] = r.AnkleHipRightAbductionDifference
            });
        }
        return res;
    }
    string BuildAnalysisPacketJson(List<TimeBasedReadingRequest> rows)
    {
        var pkt = new AnalysisPacket { stats = new List<VarStats>(), totalRows = rows?.Count ?? 0 };
        if (rows == null || rows.Count == 0) return JsonConvert.SerializeObject(pkt);

        var time = rows.Select(r => {
            double t; return double.TryParse(r.TimeOfReading, out t) ? t : double.NaN;
        }).ToList();
        var timeClean = time.Where(x => !double.IsNaN(x)).ToList();
        pkt.timeMin = timeClean.Count > 0 ? timeClean.Min() : double.NaN;
        pkt.timeMax = timeClean.Count > 0 ? timeClean.Max() : double.NaN;
        pkt.duration = (pkt.timeMax - pkt.timeMin);

        var pAll = typeof(TimeBasedReadingRequest)
            .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(float?))
            .ToList();

        pkt.variables = pAll.Select(p => p.Name).ToArray();

        foreach (var p in pAll)
        {
            var seriesF = GetSeries(rows, p).Select(v => (double)v).ToList();
            int count = seriesF.Count;
            int missing = rows.Count - count;

            var pairs = rows.Select((r, i) => {
                double t; var tOk = double.TryParse(r.TimeOfReading, out t);
                var fv = p.GetValue(r) as float?;
                return new { ok = tOk && fv.HasValue, t, v = fv.HasValue ? (double)fv.Value : double.NaN };
            }).Where(x => x.ok).ToList();

            var x = pairs.Select(z => z.t).ToList();
            var y = pairs.Select(z => z.v).ToList();
            var (slope, _) = LinReg(x.ToArray(), y.ToArray());

            double min = double.NaN, max = double.NaN; object minAt = null, maxAt = null;
            if (count > 0)
            {
                min = seriesF.Min(); max = seriesF.Max();
                for (int i = 0; i < rows.Count; i++)
                {
                    var fv = p.GetValue(rows[i]) as float?;
                    if (fv.HasValue && Math.Abs(fv.Value - min) < 1e-6) { minAt = rows[i].TimeOfReading; break; }
                }
                for (int i = 0; i < rows.Count; i++)
                {
                    var fv = p.GetValue(rows[i]) as float?;
                    if (fv.HasValue && Math.Abs(fv.Value - max) < 1e-6) { maxAt = rows[i].TimeOfReading; break; }
                }
            }

            pkt.stats.Add(new VarStats
            {
                name = p.Name,
                count = count,
                missing = missing,
                mean = Mean(seriesF),
                sd = SD(seriesF),
                p5 = Percentile(seriesF, 5),
                p50 = Percentile(seriesF, 50),
                p95 = Percentile(seriesF, 95),
                min = min,
                max = max,
                minAt = minAt,
                maxAt = maxAt,
                trendSlopePerSec = slope
            });
        }

        pkt.sampleRows = StratifiedSample(rows, 60);
        return JsonConvert.SerializeObject(pkt);
    }

    string BuildReportPrompt(string analysisJson, bool hasGait)
    {
        return $@"
You are a physiotherapist & data-visualization expert.

You are given an ANALYSIS_PACKET (JSON) that summarizes a gait dataset (stats, trends, percentiles, extremes) and includes a small sample of rows. Use it to:
- Diagnose asymmetries, instability, unusual ranges, fatigue/compensation, etc.
- Write a clear, clinical-style narrative and practical recommendations (strengthening, neuromuscular control, retraining).
- Populate metric cards (mean±SD, ranges, percentiles) from ANALYSIS_PACKET numbers (do not invent).
- Propose and render key charts (Chart.js) and a full table.

IMPORTANT:
- Do NOT inline large raw data; the page will load full data from window.__READINGS__ {(hasGait ? "and window.__GAIT__" : "")} via data.js.
- Include <script src=""data.js""></script> in the <head> if missing.
- Build all charts/tables by reading those globals at runtime.
- Return a complete, responsive HTML document (no markdown fences).

ANALYSIS_PACKET:
{analysisJson}
";
    }

    // ===== History helpers =====
    void AddToHistory(string role, string content, bool compressIfLarge = false)
    {
        if (string.IsNullOrEmpty(content)) content = "";

        string toStore = content;
        if (compressIfLarge && compressLargeAssistantReplies && content.Length > maxAssistantCharsForHistory)
        {
            int keep = maxAssistantCharsForHistory / 2;
            string head = content.Substring(0, keep);
            string tail = content.Substring(content.Length - keep);
            toStore =
                $"[Large assistant reply: {content.Length} chars. Stored head+tail only]\n" +
                $"---BEGIN HEAD---\n{head}\n---END HEAD---\n" +
                $"---BEGIN TAIL---\n{tail}\n---END TAIL---";
        }

        conversationHistory.Add(new Message { role = role, content = toStore });
        PruneHistory();
    }

    void PruneHistory()
    {
        int total = 0;
        for (int i = conversationHistory.Count - 1; i >= 0; i--)
        {
            total += conversationHistory[i].content?.Length ?? 0;
            if (total > maxHistoryChars)
            {
                if (i > 0) conversationHistory.RemoveRange(0, i);
                break;
            }
        }
    }

    // ===================== Domain notes =====================
    // public class TimeBasedReadingRequest { public string TimeOfReading; public float? PelvisAngle; public float? VarusValgusRight; ... }
    // public class GetGaitReportResponse { ... }
    // public class CustomDropDown { public List<UserReportFromDB> selectedUserReports; public List<DropdownItem> items; public List<DropdownItem> selectedItems; }
    // public class DropdownItem { public Toggle myToggle; }
    // public class UserReportFromDB { public List<TimeBasedReadingRequest> timeBasedReadings; public List<GetGaitReportResponse> gaitReports; }
    // public static class GeneralStaticManager { public static string ToJsonExcludingNulls(object o) { ... } public static string GenerateRandomName(){...} public static string FormatCsvAsTable(string s){...} public static void OpenFile(string path){...}}
    // public static class APIHandler { public static APIHandler instance; public void Post(string route, string json, Action<string> onSuccess, Action<string> onError, bool auth){} }
    // public static class CSVManager { public static void Export(string path){} }
    // public class UniWebView : MonoBehaviour { public Rect Frame; public void Load(string url){} public void Show(){} public void LoadHTMLString(string html, string baseUrl){} public UniWebViewToolbar EmbeddedToolbar => new UniWebViewToolbar(); public Color BackgroundColor{get;set;} public void SetOpenLinksInExternalBrowser(bool v){} public event Func<UniWebView, bool> OnShouldClose; }
    // public class UniWebViewToolbar { public void Show(){} }
    // public static class UniClipboard { public static void SetText(string s){} }
}
