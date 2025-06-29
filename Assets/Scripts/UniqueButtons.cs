using UnityEngine;
using System.Text.RegularExpressions;

public class UniqueButtons : MonoBehaviour
{
    // Reference to your main OllamaScript (assign in Inspector)
    public OllamaScript ollamaScript;

    // Call this from your Empathize button's OnClick event
    public void OnGenerateUserPersonas()
    {
        if (ollamaScript == null)
        {
            Debug.LogWarning("OllamaScript reference missing!");
            return;
        }

        // Get context for personas (e.g., project idea/history)
        string context = ollamaScript.GetEmpathizeContext();

        // Build the prompt for personas
        string prompt =
            "Based on the following project idea/context, generate 2–3 detailed user personas. " +
            "For each persona, include: demographics (age, gender, occupation, etc.), goals, pain points, and a representative quote. " +
            "Label each persona as 'Persona 1:', 'Persona 2:', etc. " +
            "No markdown, no lists, no hashtags, no asterisks, no dashes, no introduction or summary, just the personas. " +
            "Context:\n" + context;

        // Send the prompt to the AI for the Empathize step
        ollamaScript.SendCustomPromptToEmpathize(prompt, OnPersonasGenerated);
    }

    // Callback: Split and display each persona as a unique message
    private void OnPersonasGenerated(string aiResponse)
    {
        if (ollamaScript != null)
        {
            // 1. Remove <think>...</think> blocks
            string cleaned = Regex.Replace(
                aiResponse, @"<think>.*?</think>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // 2. Remove markdown and formatting symbols
            cleaned = cleaned.Replace("**", "")
                             .Replace("*", "")
                             .Replace("---", "")
                             .Replace("```", "")
                             .Replace("#", "");

            // 3. Remove any intro text before the first "Persona"
            int firstPersona = Regex.Match(cleaned, @"Persona\s*\d*[:：]", RegexOptions.IgnoreCase).Index;
            if (firstPersona > 0)
                cleaned = cleaned.Substring(firstPersona);

            // 4. Split by Persona label (handles Persona 1:, Persona1:, Persona 2:, etc.)
            var matches = Regex.Split(cleaned, @"(?=Persona\s*\d*[:：])", RegexOptions.IgnoreCase);

            foreach (var persona in matches)
            {
                string message = persona.Trim();
                // Only add if it looks like a persona (starts with "Persona" and is not empty)
                if (!string.IsNullOrEmpty(message) && message.ToLower().StartsWith("persona"))
                {
                    ollamaScript.AddStepMessage(message, false, DesignStep.Empathize);
                }
            }
        }
        else
        {
            Debug.Log("User Personas:\n" + aiResponse);
        }
    }

    // Call this from your Define button's OnClick event
    public void OnGenerateInsightsAndProblem()
    {
        if (ollamaScript == null)
        {
            Debug.LogWarning("OllamaScript reference missing!");
            return;
        }

        // Gather main conversation and empathize messages
        string mainContext = ollamaScript.GetEmpathizeContext();
        string empathizeData = ollamaScript.GetStepMessages(DesignStep.Empathize);

        // Build the prompt
        string prompt =
            "Analyze the following project idea and user persona data. " +
            "ONLY output 1–3 short, clear user insights (one per line, no explanations, no introduction, no markdown, no lists, no asterisks, no dashes, no quotes, no 'think' sections, just plain sentences). " +
            "After the insights, output a single concise problem statement, starting with 'Problem statement:' and nothing else.\n\n" +
            "Project idea and conversation:\n" + mainContext + "\n\n" +
            "User personas and empathy data:\n" + empathizeData + "\n\n" +
            "Output format:\n" +
            "Insight 1.\nInsight 2.\nInsight 3.\n\nProblem statement: [one sentence]";

        ollamaScript.SendCustomPromptToDefine(prompt, OnInsightsAndProblemGenerated);
    }

    private void OnInsightsAndProblemGenerated(string aiResponse)
    {
        if (ollamaScript != null)
        {
            // 1. Remove <think>...</think> blocks
            string cleaned = Regex.Replace(
                aiResponse, @"<think>.*?</think>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // 2. Remove markdown/formatting
            cleaned = cleaned.Replace("**", "")
                             .Replace("*", "")
                             .Replace("---", "")
                             .Replace("```", "")
                             .Replace("#", "")
                             .Replace("\"", "");

            // 3. Split by lines
            string[] lines = cleaned.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            int insightCount = 0;
            foreach (var line in lines)
            {
                string msg = line.Trim();
                // Only add lines that look like insights or the problem statement
                if (!string.IsNullOrEmpty(msg))
                {
                    if (msg.ToLower().StartsWith("insight") && insightCount < 5)
                    {
                        ollamaScript.AddStepMessage(msg, false, DesignStep.Define);
                        insightCount++;
                    }
                    else if (msg.ToLower().StartsWith("problem statement"))
                    {
                        ollamaScript.AddStepMessage(msg, false, DesignStep.Define);
                        break; // Only one problem statement
                    }
                }
            }
        }
        else
        {
            Debug.Log("Insights and Problem Statement:\n" + aiResponse);
        }
    }
    public void OnRunIdeaSprint()
    {
        if (ollamaScript == null)
        {
            Debug.LogWarning("OllamaScript reference missing!");
            return;
        }

        // Get the latest problem statement from the Define step
        string defineData = ollamaScript.GetStepMessages(DesignStep.Define);
        string problemStatement = "";
        foreach (var line in defineData.Split('\n'))
        {
            if (line.Trim().ToLower().StartsWith("problem statement"))
            {
                problemStatement = line.Trim();
                break;
            }
        }
        if (string.IsNullOrEmpty(problemStatement))
        {
            Debug.LogWarning("No problem statement found in Define step.");
            return;
        }

        // Build the prompt
        string ideateHistory = ollamaScript.GetStepMessages(DesignStep.Ideate);

        string prompt =
            "Based on the following problem statement and previous brainstorming conversation, use a mix of ideation techniques (such as SCAMPER, Worst Idea, and Brainstorm) to generate 1–5 creative solution ideas. " +
            "For each idea, label it as 'Idea 1:', 'Idea 2:', etc. " +
            "Keep each idea short, clear, and actionable. " +
            "Do not include any introduction, explanation, or markdown—just the ideas.\n\n" +
            "Problem statement:\n" + problemStatement + "\n\n" +
            "Previous ideation conversation:\n" + ideateHistory + "\n\n" +
            "Output format:\n" +
            "Idea 1: [solution idea]\nIdea 2: [solution idea]";


        ollamaScript.SendCustomPromptToIdeate(prompt, OnIdeasGenerated);
    }

    private void OnIdeasGenerated(string aiResponse)
    {
        if (ollamaScript != null)
        {
            // Remove <think>...</think> blocks and formatting
            string cleaned = Regex.Replace(aiResponse, @"<think>.*?</think>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase)
                                 .Replace("**", "")
                                 .Replace("*", "")
                                 .Replace("---", "")
                                 .Replace("```", "")
                                 .Replace("#", "");

            // Split by "Idea" label
            var matches = Regex.Split(cleaned, @"(?=Idea\s*\d*[:：])", RegexOptions.IgnoreCase);

            foreach (var idea in matches)
            {
                string message = idea.Trim();
                if (!string.IsNullOrEmpty(message) && message.ToLower().StartsWith("idea"))
                {
                    ollamaScript.AddStepMessage(message, false, DesignStep.Ideate);
                }
            }
        }
        else
        {
            Debug.Log("Ideas:\n" + aiResponse);
        }
    }

// Callback to split and display each mockup idea
private void OnPrototypeIdeasGenerated(string aiResponse)
{
    if (ollamaScript != null)
    {
        // Remove <think>...</think> blocks and formatting
        string cleaned = Regex.Replace(aiResponse, @"<think>.*?</think>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase)
                             .Replace("**", "")
                             .Replace("*", "")
                             .Replace("---", "")
                             .Replace("```", "")
                             .Replace("#", "");

        // Split by "Mockup" label
        var matches = Regex.Split(cleaned, @"(?=Mockup\s*\d*[:：])", RegexOptions.IgnoreCase);

        foreach (var mockup in matches)
        {
            string message = mockup.Trim();
            if (!string.IsNullOrEmpty(message) && message.ToLower().StartsWith("mockup"))
            {
                ollamaScript.AddStepMessage(message, false, DesignStep.Prototype);
            }
        }
    }
    else
    {
        Debug.Log("Prototype Mockups:\n" + aiResponse);
    }
}



}

