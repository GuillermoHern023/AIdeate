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
        }
        else
        {
            Debug.Log("No previous panel in history.");
        }
    }
}
