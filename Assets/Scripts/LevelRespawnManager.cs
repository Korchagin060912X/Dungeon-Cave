using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelRespawnManager : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "SampleScene";

    public void RestartLevel()
    {
        string sceneToLoad = string.IsNullOrWhiteSpace(gameplaySceneName)
            ? SceneManager.GetActiveScene().name
            : gameplaySceneName;

        SceneManager.LoadScene(sceneToLoad);
    }

    public void Die()
    {
        RestartLevel();
    }
}
