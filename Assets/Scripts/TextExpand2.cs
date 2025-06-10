using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class AutoExpandInputField : MonoBehaviour
{
    public float minHeight = 40f;
    public float maxHeight = 200f;
    public float padding = 10f;

    private TMP_InputField inputField;
    private RectTransform rectTransform;
    private TMP_Text textComponent;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        rectTransform = GetComponent<RectTransform>();
        textComponent = inputField.textComponent;

        inputField.onValueChanged.AddListener(OnTextChanged);
        OnTextChanged(inputField.text);
    }

    void OnTextChanged(string text)
    {
        textComponent.ForceMeshUpdate();

        float preferredHeight = textComponent.textBounds.size.y;
        float newHeight = Mathf.Clamp(preferredHeight + padding, minHeight, maxHeight);

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
    }
}
