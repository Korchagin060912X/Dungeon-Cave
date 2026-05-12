using UnityEngine;
using UnityEngine.UI;

public class DigitCollectible : MonoBehaviour
{
    [SerializeField] private string digit = "0";
    [SerializeField] private Text targetText;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (targetText != null)
        {
            targetText.text += digit;
        }

        Destroy(gameObject);
    }

    public void Configure(string value, Text outputText)
    {
        digit = value;
        targetText = outputText;
    }
}
