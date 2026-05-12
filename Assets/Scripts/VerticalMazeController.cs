using UnityEngine;
using UnityEngine.UI;

public class VerticalMazeController : MonoBehaviour
{
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private Text timerText;
    [SerializeField] private GameObject avalancheObject;
    [SerializeField] private float avalancheEndY = 6f;
    [SerializeField] private float avalancheRiseSpeed = 0.6f;
    [SerializeField] private LevelRespawnManager respawnManager;

    private float timeLeft;
    private bool active = true;

    private void Start()
    {
        timeLeft = timeLimit;
        UpdateTimerText();
    }

    private void Update()
    {
        if (!active)
        {
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            active = false;
            respawnManager.Die();
        }

        UpdateTimerText();
        RiseAvalanche();
    }

    private void RiseAvalanche()
    {
        if (avalancheObject == null)
        {
            return;
        }

        Vector3 pos = avalancheObject.transform.position;
        float y = Mathf.MoveTowards(pos.y, avalancheEndY, avalancheRiseSpeed * Time.deltaTime);
        avalancheObject.transform.position = new Vector3(pos.x, y, pos.z);
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text = $"Лабиринт: {Mathf.CeilToInt(timeLeft)}";
    }

    public void Configure(Text timerOutput, GameObject avalanche, LevelRespawnManager manager)
    {
        timerText = timerOutput;
        avalancheObject = avalanche;
        respawnManager = manager;
    }
}
