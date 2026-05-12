using UnityEngine;
using UnityEngine.UI;

public class CodePuzzleController : MonoBehaviour
{
    [SerializeField] private string correctCode = "8303";
    [SerializeField] private InputField inputField;
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject doorToOpen;
    [SerializeField] private string lockedMessage = "Неверный код";
    [SerializeField] private string unlockedMessage = "Код принят. Проход открыт.";

    private bool unlocked;

    public void TrySubmitCode()
    {
        if (unlocked || inputField == null)
        {
            return;
        }

        string code = inputField.text.Trim();
        if (code == correctCode)
        {
            unlocked = true;
            if (doorToOpen != null)
            {
                doorToOpen.SetActive(false);
            }

            SetStatus(unlockedMessage, Color.green);
            return;
        }

        SetStatus(lockedMessage, Color.red);
    }

    private void SetStatus(string message, Color color)
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = message;
        statusText.color = color;
    }

    public void Configure(InputField field, Text status, GameObject doorObject)
    {
        inputField = field;
        statusText = status;
        doorToOpen = doorObject;
    }
}
