
using UnityEngine;
using System.Collections.Generic;

public class ButtonLogic : MonoBehaviour
{
    public GameObject Starter;
    public GameObject hexagonsPanel;
    public GameObject empathizePanel;
    public GameObject definePanel;
    public GameObject ideatePanel;
    public GameObject prototypePanel;
    public GameObject testPanel;

    public OllamaScript ollamaScript; // Assign in Inspector

    public GameObject fullSubmitButton;           // Assign in Inspector
    public GameObject regenerateUnlockedButton;    // Assign in Inspector

    private Stack<GameObject> history = new Stack<GameObject>();
    private GameObject currentPanel;

    void Start()
    {
        currentPanel = Starter;
        currentPanel.SetActive(true);

        empathizePanel.SetActive(false);
        definePanel.SetActive(false);
        ideatePanel.SetActive(false);
        prototypePanel.SetActive(false);
        testPanel.SetActive(false);

        if (ollamaScript != null)
            ollamaScript.currentStep = DesignStep.None;

        if (fullSubmitButton != null)
            fullSubmitButton.SetActive(true); // Starter is active at launch

        if (regenerateUnlockedButton != null)
            regenerateUnlockedButton.SetActive(false); // Not visible on Starter at launch
    }

    private void EnablePanel(GameObject panel, DesignStep step)
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            history.Push(currentPanel);
        }
        panel.SetActive(true);
        currentPanel = panel;
        if (ollamaScript != null)
            ollamaScript.currentStep = step;

        // Show the Full Submit button ONLY on the Starter panel
        if (fullSubmitButton != null)
            fullSubmitButton.SetActive(panel == Starter);

        // Show the Regenerate Unlocked Steps button ONLY when NOT on the Starter panel
        if (regenerateUnlockedButton != null)
            regenerateUnlockedButton.SetActive(panel != Starter);
    }

    public void GoToHexagons()
    {
        if (ollamaScript != null)
            ollamaScript.AnalyzeAndDistribute();

        EnablePanel(hexagonsPanel, DesignStep.None);
    }

    public void GoToEmpathize()
    {
        EnablePanel(empathizePanel, DesignStep.Empathize);
    }

    public void GoToDefine()
    {
        EnablePanel(definePanel, DesignStep.Define);
    }

    public void GoToIdeate()
    {
        EnablePanel(ideatePanel, DesignStep.Ideate);
    }

    public void GoToPrototype()
    {
        EnablePanel(prototypePanel, DesignStep.Prototype);
    }

    public void GoToTest()
    {
        EnablePanel(testPanel, DesignStep.Test);
    }

    public void Back()
    {
        if (history.Count > 0)
        {
            Debug.Log("Going back from: " + currentPanel.name + " to: " + history.Peek().name);
            currentPanel.SetActive(false);
            currentPanel = history.Pop();
            currentPanel.SetActive(true);

            // Set currentStep based on which panel is now active
            if (ollamaScript != null)
            {
                if (currentPanel == empathizePanel) ollamaScript.currentStep = DesignStep.Empathize;
                else if (currentPanel == definePanel) ollamaScript.currentStep = DesignStep.Define;
                else if (currentPanel == ideatePanel) ollamaScript.currentStep = DesignStep.Ideate;
                else if (currentPanel == prototypePanel) ollamaScript.currentStep = DesignStep.Prototype;
                else if (currentPanel == testPanel) ollamaScript.currentStep = DesignStep.Test;
                else ollamaScript.currentStep = DesignStep.None;
            }

            // Show/hide the Full Submit button
            if (fullSubmitButton != null)
                fullSubmitButton.SetActive(currentPanel == Starter);

            // Show/hide the Regenerate Unlocked Steps button
            if (regenerateUnlockedButton != null)
                regenerateUnlockedButton.SetActive(currentPanel != Starter);
        }
        else
        {
            Debug.Log("No previous panel in history.");
        }
    }

    // --- Lock/Unlock Button Methods ---
    public void ToggleLockEmpathize() { ollamaScript.ToggleStepLocked(DesignStep.Empathize); }
    public void ToggleLockDefine()    { ollamaScript.ToggleStepLocked(DesignStep.Define); }
    public void ToggleLockIdeate()    { ollamaScript.ToggleStepLocked(DesignStep.Ideate); }
    public void ToggleLockPrototype() { ollamaScript.ToggleStepLocked(DesignStep.Prototype); }
    public void ToggleLockTest()      { ollamaScript.ToggleStepLocked(DesignStep.Test); }

    // --- Full Submit Button (Starter only) ---
    public void FullSubmitRespectingLocks()
    {
        Debug.Log("FullSubmitRespectingLocks button pressed.");
        if (ollamaScript != null)
            ollamaScript.AnalyzeAndDistributeRespectingLocks();
    }

    // --- Regenerate Unlocked Steps Button (Not Starter) ---
    public void RegenerateUnlockedSteps()
    {
        Debug.Log("RegenerateUnlockedSteps button pressed.");
        if (ollamaScript != null)
            ollamaScript.AnalyzeAndDistributeRespectingLocks();
    }
}
