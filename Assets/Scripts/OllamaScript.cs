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
    public string modelName = "deepseek-r1:7b";

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);
    }

    void OnSubmit()
    {
        string prompt = Prompts.BuildDefaultPrompt(userInput.text);
        StartCoroutine(SendToOllama(prompt));
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

                responseText.text = output.Trim();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Parsing error: {e.Message}\nRaw response: {request.downloadHandler.text}");
                responseText.text = "Error: Could not parse JSON response.";
            }
        }
        else
        {
            responseText.text = $"Error: {request.error}";
        }
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
