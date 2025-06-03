public static class Prompts
{
    public static string BuildDefaultPrompt(string userInput)
    {
        string instructions = "Answer briefly and directly in 1 sentece. Do not include <think> or internal thoughts.";
        return $"{instructions}\n\n{userInput}";
    }

    // Add more prompt builders here as needed
}
