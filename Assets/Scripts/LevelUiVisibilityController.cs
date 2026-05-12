using UnityEngine;

public class LevelUiVisibilityController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject codeUiPanel;
    [SerializeField] private GameObject mazeUiPanel;
    [SerializeField] private float codeUiStartX = -2f;
    [SerializeField] private float codeUiEndX = 25f;
    [SerializeField] private float mazeUiStartX = 49f;
    [SerializeField] private float mazeUiEndX = 70f;

    public void Configure(Transform playerTransform, GameObject codePanel, GameObject mazePanel)
    {
        player = playerTransform;
        codeUiPanel = codePanel;
        mazeUiPanel = mazePanel;
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        float x = player.position.x;
        if (codeUiPanel != null)
        {
            codeUiPanel.SetActive(x >= codeUiStartX && x <= codeUiEndX);
        }

        if (mazeUiPanel != null)
        {
            mazeUiPanel.SetActive(x >= mazeUiStartX && x <= mazeUiEndX);
        }
    }
}
