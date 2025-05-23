using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;

public class OllamaChat : MonoBehaviour
{
    public TMP_InputField userInput;
    public Button submitButton;
    public TMP_Text responseText;

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);
    }

    void OnSubmit()
    {
        // Add instructions for concise answers here:
        string instructions = "Answer briefly and directly in 1-2 sentences only. No extra details.";
        string prompt = instructions + "\n\n" + userInput.text;

        StartCoroutine(SendToOllama(prompt));
    }

    IEnumerator SendToOllama(string prompt)
    {
        string json = $"{{\"model\": \"deepseek-r1:7b\", \"prompt\": \"{EscapeJson(prompt)}\", \"stream\": false}}";
        UnityWebRequest request = new UnityWebRequest("http://localhost:11434/api/generate", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string raw = request.downloadHandler.text;

            // Extract response from JSON by finding "response":"...".
            string marker = "\"response\":\"";
            int start = raw.IndexOf(marker);
            if (start >= 0)
            {
                start += marker.Length;
                int end = raw.IndexOf("\"", start);
                if (end >= 0)
                {
                    string response = raw.Substring(start, end - start);
                    // Unescape common escape sequences from JSON string
                    response = response.Replace("\\n", "\n").Replace("\\\"", "\"");
                    responseText.text = response;
                    yield break;
                }
            }

            responseText.text = "Error: Could not parse response.";
        }
        else
        {
            responseText.text = $"Error: {request.error}";
        }
    }

    // Helper method to escape special characters in JSON strings
    string EscapeJson(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }
}
