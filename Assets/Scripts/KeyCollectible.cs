using UnityEngine;

public class KeyCollectible : MonoBehaviour
{
    [SerializeField] private KeyDoorController keyDoorController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        keyDoorController.AddKey();
        Destroy(gameObject);
    }

    public void Configure(KeyDoorController controller)
    {
        keyDoorController = controller;
    }
}
