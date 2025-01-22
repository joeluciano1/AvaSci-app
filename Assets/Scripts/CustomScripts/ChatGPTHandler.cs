using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LightBuzz.AvaSci.Csv;
using Newtonsoft.Json;

public class ChatGPTHandler : MonoBehaviour
{
    private string apiKey;
    private string openAIEndpoint = "https://api.openai.com/v1/chat/completions";

    public void AnalyzeCSV(List<string> csvDatas)
    {
        
        string prompt = $"Analyze the following CSV datas and generate an insightful HTML report. " +
                        $"Provide key observations, statistical insights, and include graph plots using Chart.js for visualization. The plots should be w.r.t the time column values in the csvs " +
                        $"Ensure the report is well-formatted and responsive for web viewing:\n\n{csvDatas[0]} and {csvDatas[1]}";



        StartCoroutine(SendRequestToChatGPT(prompt));
    }

    IEnumerator SendRequestToChatGPT(string prompt)
    {
        var requestData = new
        {
            model = "gpt-4-turbo",  // Use GPT-4 turbo for faster responses
            messages = new[]
            {
                new { role = "system", content = "You are an expert data analyst and HTML report generator." },
                new { role = "user", content = prompt }
            },
            max_tokens = 4000,
            temperature = 0.7
        };

        string jsonBody = JsonConvert.SerializeObject(requestData);

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
        CSVManager.Export(path);
        Debug.Log("PDF Report saved at: " + path);
    }

    [System.Serializable]
    private class ChatGPTResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }

    [System.Serializable]
    private class Message
    {
        public string content;
    }
}
