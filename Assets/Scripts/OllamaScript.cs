using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;
using System.Collections.Generic;

public class OllamaScript : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField userInput;
    public Button submitButton;
    public ScrollRect chatScrollRect;
    public RectTransform chatContent;
    public GameObject userMessagePrefab;
    public GameObject aiMessagePrefab;

    [Header("Ollama Settings")]
    public string modelName = "deepseek-r1:7b";

    private List<string> conversationHistory = new List<string>();

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);
    }

    void OnSubmit()
    {
        string userMessage = userInput.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        // Add user message to history and UI
        conversationHistory.Add($"User: {userMessage}");
        AddMessage(userMessage, true); // Show user message bubble

        // Build conversation string for prompt
        string conversation = string.Join("\n", conversationHistory) + "\nAI:";
        string prompt = Prompts.BuildDefaultPrompt(conversation);

        StartCoroutine(SendToOllama(prompt));
        userInput.text = "";
    }

    IEnumerator SendToOllama(string prompt)
    {
        string json = $"{{\"model\": \"{modelName}\", \"prompt\": \"{EscapeJson(prompt)}\", \"stream\": false}}";
        UnityWebRequest request = new UnityWebRequest("http://localhost:11434/api/generate", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string raw = request.downloadHandler.text;
                OllamaResponse res = JsonUtility.FromJson<OllamaResponse>(raw);

                string output = res.response;

                // Strip <think>...</think> blocks
                while (true)
                {
                    int thinkStart = output.IndexOf("<think>");
                    int thinkEnd = output.IndexOf("</think>");
                    if (thinkStart >= 0 && thinkEnd > thinkStart)
                    {
                        output = output.Remove(thinkStart, (thinkEnd + 8) - thinkStart);
                    }
                    else break;
                }

                // Add AI response to history and UI
                conversationHistory.Add($"AI: {output.Trim()}");
                AddMessage(output.Trim(), false); // Show AI message bubble
            }
            catch (System.Exception e)
            {
                AddMessage("Error: Could not parse JSON response.", false);
                Debug.LogError($"Parsing error: {e.Message}\nRaw response: {request.downloadHandler.text}");
            }
        }
        else
        {
            AddMessage($"Error: {request.error}", false);
        }
    }

    void AddMessage(string message, bool isUser)
    {
        GameObject prefab = isUser ? userMessagePrefab : aiMessagePrefab;
        GameObject msgObj = Instantiate(prefab, chatContent);
        TMP_Text msgText = msgObj.GetComponentInChildren<TMP_Text>();
        msgText.text = message;

        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    string EscapeJson(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    [System.Serializable]
    private class OllamaResponse
    {
        public string response;
    }
}
