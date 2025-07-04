using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum DesignStep { None, Empathize, Define, Ideate, Prototype, Test }

public class OllamaScript : MonoBehaviour
{
    [Header("General Chat UI References")]
    public TMP_InputField userInput;
    public Button submitButton;
    public ScrollRect chatScrollRect;
    public RectTransform chatContent;
    public GameObject userMessagePrefab;
    public GameObject aiMessagePrefab;

    [Header("Step Content Parents")]
    public RectTransform empathizeContent;
    public RectTransform defineContent;
    public RectTransform ideateContent;
    public RectTransform prototypeContent;
    public RectTransform testContent;

    [Header("Ollama Settings")]
    public string modelName = "deepseek-r1:7b";
    public string GetEmpathizeContext()
    {
        // You can return the main project idea, or combine history, etc.
        return string.Join("\n", mainConversationHistory);
    }
    public string GetStepMessages(DesignStep step)
    {
        if (stepConversations.ContainsKey(step))
            return string.Join("\n", stepConversations[step]);
        return "";
    }

    // And add:
    public void SendCustomPromptToDefine(string prompt, System.Action<string> onComplete = null)
    {
        StartCoroutine(SendCustomPromptCoroutine(prompt, DesignStep.Define, onComplete));
    }
    public void SendCustomPromptToIdeate(string prompt, System.Action<string> onComplete = null)
    {
        StartCoroutine(SendCustomPromptCoroutine(prompt, DesignStep.Ideate, onComplete));
    }
    public void SendCustomPromptToPrototype(string prompt, System.Action<string> onComplete = null)
{
    StartCoroutine(SendCustomPromptCoroutine(prompt, DesignStep.Prototype, onComplete));
}





    [HideInInspector]
    public DesignStep currentStep = DesignStep.None; // Set by ButtonLogic

    // Store main and step-specific conversation histories
    private List<string> mainConversationHistory = new List<string>();
    private Dictionary<DesignStep, List<string>> stepConversations = new Dictionary<DesignStep, List<string>>()
    {
        { DesignStep.Empathize, new List<string>() },
        { DesignStep.Define, new List<string>() },
        { DesignStep.Ideate, new List<string>() },
        { DesignStep.Prototype, new List<string>() },
        { DesignStep.Test, new List<string>() }
    };

    // Lock state for each step
    private Dictionary<DesignStep, bool> stepLocked = new Dictionary<DesignStep, bool>()
    {
        { DesignStep.Empathize, false },
        { DesignStep.Define, false },
        { DesignStep.Ideate, false },
        { DesignStep.Prototype, false },
        { DesignStep.Test, false }
    };

    // --- Lock/Unlock Methods ---
    public void SetStepLocked(DesignStep step, bool locked)
    {
        stepLocked[step] = locked;
        Debug.Log($"SetStepLocked: {step} is now {(locked ? "LOCKED" : "UNLOCKED")}");
    }

    public bool IsStepLocked(DesignStep step)
    {
        return stepLocked.ContainsKey(step) && stepLocked[step];
    }

    public void ToggleStepLocked(DesignStep step)
    {
        stepLocked[step] = !stepLocked[step];
        Debug.Log($"ToggleStepLocked: {step} is now {(stepLocked[step] ? "LOCKED" : "UNLOCKED")}");
    }

    // --- Chat Submission ---
    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);
    }

    void OnSubmit()
    {
        string userMessage = userInput.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        if (currentStep == DesignStep.None)
        {
            mainConversationHistory.Add($"User: {userMessage}");
            AddMessage(userMessage, true); // Main chat
            string conversation = string.Join("\n", mainConversationHistory) + "\nAI:";
            // You can use a default prompt here if you wish
            string prompt = Prompts.BuildEmpathizePrompt(conversation, "");
            StartCoroutine(SendToOllama(prompt, DesignStep.None));
        }
        else
        {
            stepConversations[currentStep].Add($"User: {userMessage}");
            AddStepMessage(userMessage, true, currentStep); // Step chat

            // Combine main chat and step chat for context
            string conversation = string.Join("\n", mainConversationHistory) + "\n" +
                                  string.Join("\n", stepConversations[currentStep]) + "\nAI:";
            string prompt = "";
            switch (currentStep)
            {
                case DesignStep.Empathize:
                    prompt = Prompts.BuildEmpathizePrompt(conversation, "");
                    break;
                case DesignStep.Define:
                    prompt = Prompts.BuildDefinePrompt(conversation, "");
                    break;
                case DesignStep.Ideate:
                    prompt = Prompts.BuildIdeatePrompt(conversation, "");
                    break;
                case DesignStep.Prototype:
                    prompt = Prompts.BuildPrototypePrompt(conversation, "");
                    break;
                case DesignStep.Test:
                    prompt = Prompts.BuildTestPrompt(conversation, "");
                    break;
            }
            StartCoroutine(SendToOllama(prompt, currentStep));
        }
        userInput.text = "";
    }

    IEnumerator SendToOllama(string prompt, DesignStep step)
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

                if (step == DesignStep.None)
                {
                    mainConversationHistory.Add($"AI: {output.Trim()}");
                    AddMessage(output.Trim(), false); // Main chat
                }
                else
                {
                    stepConversations[step].Add($"AI: {output.Trim()}");
                    // Split into sentences for step chat
                    string[] sentences = output.Split(new[] { ". " }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var sentence in sentences)
                    {
                        string cleanSentence = sentence.Trim();
                        if (!cleanSentence.EndsWith(".")) cleanSentence += ".";
                        if (!string.IsNullOrWhiteSpace(cleanSentence))
                            AddStepMessage(cleanSentence, false, step);
                    }
                }
            }
            catch (System.Exception e)
            {
                if (step == DesignStep.None)
                    AddMessage("Error: Could not parse JSON response.", false);
                else
                    AddStepMessage("Error: Could not parse JSON response.", false, step);
                Debug.LogError($"Parsing error: {e.Message}\nRaw response: {request.downloadHandler.text}");
            }
        }
        else
        {
            if (step == DesignStep.None)
                AddMessage($"Error: {request.error}", false);
            else
                AddStepMessage($"Error: {request.error}", false, step);
        }
    }

    // General chat message (main chat window)
    void AddMessage(string message, bool isUser)
    {
        GameObject prefab = isUser ? userMessagePrefab : aiMessagePrefab;
        GameObject msgObj = Instantiate(prefab, chatContent);
        TMP_Text msgText = msgObj.GetComponentInChildren<TMP_Text>();
        msgText.text = message;

        Canvas.ForceUpdateCanvases();
        StartCoroutine(ScrollToBottomNextFrame());
    }

    IEnumerator ScrollToBottomNextFrame()
    {
        yield return null; // Wait one frame
        chatScrollRect.verticalNormalizedPosition = 0f;
    }

    // Add message to a specific step's scroll view
    public void AddStepMessage(string message, bool isUser, DesignStep step)
    {
        GameObject prefab = isUser ? userMessagePrefab : aiMessagePrefab;
        RectTransform parent = GetStepContent(step);

        if (parent != null && !string.IsNullOrWhiteSpace(message))
        {
            GameObject msgObj = Instantiate(prefab, parent);
            TMP_Text msgText = msgObj.GetComponentInChildren<TMP_Text>();
            msgText.text = message;
        }
    }

    // --- Full Submit (Regenerate Unlocked Steps) ---
    public void AnalyzeAndDistributeRespectingLocks()
    {
        // Gather locked step conversations as context
        string lockedContext = "";
        foreach (var step in stepLocked.Keys)
        {
            if (stepLocked[step])
            {
                Debug.Log($"Step {step} is LOCKED and will NOT be regenerated.");
                lockedContext += $"[{step} Conversation]\n" + string.Join("\n", stepConversations[step]) + "\n";
            }
        }

        // For each step, regenerate if not locked
        foreach (var step in stepConversations.Keys)
        {
            if (stepLocked[step])
            {
                Debug.Log($"Skipping regeneration for {step} (LOCKED).");
                continue; // Skip locked steps
            }

            // --- CLEAR CONTENT and HISTORY for this step ---
            RectTransform content = GetStepContent(step);
            if (content != null) ClearStepContent(content);
            stepConversations[step].Clear();

            Debug.Log($"Regenerating {step} (UNLOCKED).");
            string mainHistory = string.Join("\n", mainConversationHistory);

            string prompt = "";
            switch (step)
            {
                case DesignStep.Empathize:
                    prompt = Prompts.BuildEmpathizePrompt(mainHistory, lockedContext);
                    break;
                case DesignStep.Define:
                    prompt = Prompts.BuildDefinePrompt(mainHistory, lockedContext);
                    break;
                case DesignStep.Ideate:
                    prompt = Prompts.BuildIdeatePrompt(mainHistory, lockedContext);
                    break;
                case DesignStep.Prototype:
                    prompt = Prompts.BuildPrototypePrompt(mainHistory, lockedContext);
                    break;
                case DesignStep.Test:
                    prompt = Prompts.BuildTestPrompt(mainHistory, lockedContext);
                    break;
            }

            StartCoroutine(SendToOllama(prompt, step));
        }
    }

    // --- (Optional) Old AnalyzeAndDistribute for hexagonsPanel ---
    public void AnalyzeAndDistribute()
    {
        string conversation = string.Join("\n", mainConversationHistory);

        string prompt =
            "You're an AI assistant supporting a project manager using Design Thinking.\n\n" +
            "Analyze the following project idea or discussion and break it down into the five Design Thinking stages:\n" +
            "1. Empathize – Highlight the user's needs, motivations, frustrations, or context.\n" +
            "2. Define – Clearly articulate the core user problem based on the empathy insights.\n" +
            "3. Ideate – Suggest creative and practical solution directions or opportunities.\n" +
            "4. Prototype – Describe a simple version or mockup of the solution to test.\n" +
            "5. Test – Propose how to validate the solution with users and what feedback to look for.\n\n" +
            "Output each stage as a short paragraph, clearly labeled, written in a tone suitable for project managers. " +
            "Label each section as either Empathize:, Define:, Ideate:, Prototype:, or Test: (with or without bold/markdown). " +
            "Do NOT use numbers, just the label and a colon.\n\n" +
            "Input:\n" +
            conversation;

        Debug.Log("Starting analysis and distribution...");
        StartCoroutine(AnalyzeAndDistributeCoroutine(prompt));
    }

    IEnumerator AnalyzeAndDistributeCoroutine(string prompt)
    {
        Debug.Log("Sending analysis request to AI...");
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

                Debug.Log("Raw AI response:\n" + res.response);

                ParseAndPopulateSteps(res.response);

                Debug.Log("Finished parsing and populating steps.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Parsing error: {e.Message}\nRaw response: {request.downloadHandler.text}");
            }
        }
        else
        {
            Debug.LogError($"Error: {request.error}");
        }
    }

    void ParseAndPopulateSteps(string aiResponse)
    {
        // Clear existing messages in each step (optional, for clean output)
        ClearStepContent(empathizeContent);
        ClearStepContent(defineContent);
        ClearStepContent(ideateContent);
        ClearStepContent(prototypeContent);
        ClearStepContent(testContent);

        // Regex for both bolded and plain labels
        string[] stepNames = { "Empathize", "Define", "Ideate", "Prototype", "Test" };
        Dictionary<DesignStep, string> stepTexts = new Dictionary<DesignStep, string>();

        foreach (var stepName in stepNames)
        {
            // Regex: matches **Empathize:** or Empathize: (optionally bold), captures until next label or end
            var match = Regex.Match(
                aiResponse,
                @"(?:\*\*)?" + stepName + @":(?:\*\*)?\s*(.*?)(?=(?:\*\*[A-Za-z]+:\*\*|[A-Za-z]+:|$))",
                RegexOptions.Singleline);

            if (match.Success)
            {
                var content = match.Groups[1].Value.Trim();
                Debug.Log($"Parsed content for {stepName}: {content}");
                stepTexts[(DesignStep)System.Enum.Parse(typeof(DesignStep), stepName)] = content;
            }
            else
            {
                Debug.LogWarning($"Step label '{stepName}' not found in response.");
            }
        }

        // Now populate each step's content
        foreach (var kvp in stepTexts)
        {
            Debug.Log($"Pasting to {kvp.Key}: {kvp.Value}");
            // Split into sentences for step chat
            string[] sentences = kvp.Value.Split(new[] { ". " }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var sentence in sentences)
            {
                string cleanSentence = sentence.Trim();
                if (!cleanSentence.EndsWith(".")) cleanSentence += ".";
                if (!string.IsNullOrWhiteSpace(cleanSentence))
                    AddStepMessage(cleanSentence, false, kvp.Key);
            }
        }
    }

    void ClearStepContent(RectTransform content)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    string EscapeJson(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    // Helper to get the correct RectTransform for a step
    RectTransform GetStepContent(DesignStep step)
    {
        switch (step)
        {
            case DesignStep.Empathize: return empathizeContent;
            case DesignStep.Define: return defineContent;
            case DesignStep.Ideate: return ideateContent;
            case DesignStep.Prototype: return prototypeContent;
            case DesignStep.Test: return testContent;
            default: return null;
        }
    }

    [System.Serializable]
    private class OllamaResponse
    {
        public string response;
    }

    public void SendCustomPromptToEmpathize(string prompt, System.Action<string> onComplete = null)
    {
        StartCoroutine(SendCustomPromptCoroutine(prompt, DesignStep.Empathize, onComplete));
    }

    private IEnumerator SendCustomPromptCoroutine(string prompt, DesignStep step, System.Action<string> onComplete)
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
            string raw = request.downloadHandler.text;
            OllamaResponse res = JsonUtility.FromJson<OllamaResponse>(raw);

            string output = res.response.Trim();
            // Add to history if you want
            stepConversations[step].Add($"AI: {output}");

            // Display in UI or callback
            onComplete?.Invoke(output);
        }
        else
        {
            onComplete?.Invoke("Error: " + request.error);
        }
    }
    




}

