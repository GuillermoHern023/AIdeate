public static class Prompts
{
    public static string BuildEmpathizePrompt(string mainHistory, string lockedContext)
    {
        string instructions =
            "Summarize the user's needs, motivations, and frustrations in 2-3 sentences. " +
            "Be empathetic and clear. No markdown, no lists, no hashtags.";
        return $"{instructions}\n\n{lockedContext}\n{mainHistory}\nAI:";
    }

    public static string BuildDefinePrompt(string mainHistory, string lockedContext)
    {
        string instructions =
            "Clearly state the core user problem in 1-2 sentences. " +
            "Be concise and specific. No markdown, no lists, no hashtags.";
        return $"{instructions}\n\n{lockedContext}\n{mainHistory}\nAI:";
    }

    public static string BuildIdeatePrompt(string mainHistory, string lockedContext)
    {
        string instructions =
            "Suggest one or two creative, practical solution directions in 2-3 sentences. " +
            "Be inspiring but realistic. No markdown, no lists, no hashtags.";
        return $"{instructions}\n\n{lockedContext}\n{mainHistory}\nAI:";
    }

    public static string BuildPrototypePrompt(string mainHistory, string lockedContext)
    {
        string instructions =
            "Describe a simple prototype or mockup for the solution in 2 sentences. " +
            "Focus on clarity and feasibility. No markdown, no lists, no hashtags.";
        return $"{instructions}\n\n{lockedContext}\n{mainHistory}\nAI:";
    }

    public static string BuildTestPrompt(string mainHistory, string lockedContext)
    {
        string instructions =
            "Explain how you would test the solution with users and what feedback to seek, in 2 sentences. " +
            "Be practical and user-focused. No markdown, no lists, no hashtags.";
        return $"{instructions}\n\n{lockedContext}\n{mainHistory}\nAI:";
    }
}

