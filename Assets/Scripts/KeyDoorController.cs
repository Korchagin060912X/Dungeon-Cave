using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KeyDoorController : MonoBehaviour
{
    [SerializeField] private int requiredKeys = 1;
    [SerializeField] private Text keyCounterText;
    [SerializeField] private Text completionText;
    [SerializeField] private string nextSceneName = "";

    private int collectedKeys;

    private void Start()
    {
        UpdateText();
    }

    public void AddKey()
    {
        collectedKeys++;
        UpdateText();
    }

    public bool CanOpenDoor()
    {
        return collectedKeys >= requiredKeys;
    }

    public void CompleteLevel()
    {
        if (!CanOpenDoor())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            if (completionText != null)
            {
                completionText.text = "Уровень пройден!";
                completionText.gameObject.SetActive(true);
            }

            Time.timeScale = 0f;
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void UpdateText()
    {
        if (keyCounterText == null)
        {
            return;
        }

        keyCounterText.text = $"Ключи: {collectedKeys}/{requiredKeys}";
    }

    public void Configure(Text keyText, Text completeText, int keyRequirement, string sceneName)
    {
        keyCounterText = keyText;
        completionText = completeText;
        requiredKeys = keyRequirement;
        nextSceneName = sceneName;
        UpdateText();
    }
}
