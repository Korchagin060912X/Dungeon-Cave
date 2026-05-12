using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    [SerializeField] private LevelRespawnManager respawnManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        respawnManager.Die();
    }

    public void Configure(LevelRespawnManager manager)
    {
        respawnManager = manager;
    }
}
