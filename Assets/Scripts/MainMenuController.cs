using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "SampleScene";

    public void Play()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
