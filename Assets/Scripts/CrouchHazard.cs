using UnityEngine;

public class CrouchHazard : MonoBehaviour
{
    [SerializeField] private LevelRespawnManager respawnManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player == null || !player.IsCrouching)
        {
            respawnManager.Die();
        }
    }

    public void Configure(LevelRespawnManager manager)
    {
        respawnManager = manager;
    }
}
