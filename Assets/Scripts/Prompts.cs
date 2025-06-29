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

    public static string BuildPrototypeMockupPrompt(string solutionIdeas, string lockedContext)
{
    string instructions =
        "Based on the following solution ideas, suggest 1–3 simple but specific mockup or prototype concepts. " +
        "For each, label as 'Mockup 1:', 'Mockup 2:', etc. " +
        "Describe what the prototype would look like, its main features, and how it would be presented to users. " +
        "Keep each mockup idea short, clear, and actionable. No introduction, no markdown, no lists, no hashtags, no asterisks, just the mockup descriptions.";
    return $"{instructions}\n\n{lockedContext}\n{solutionIdeas}\nAI:";
}


}

