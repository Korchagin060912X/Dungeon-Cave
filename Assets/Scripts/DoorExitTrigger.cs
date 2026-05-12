using UnityEngine;

public class DoorExitTrigger : MonoBehaviour
{
    [SerializeField] private KeyDoorController doorController;
    [SerializeField] private GameObject doorClosedVisual;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (doorController.CanOpenDoor())
        {
            if (doorClosedVisual != null)
            {
                doorClosedVisual.SetActive(false);
            }

            doorController.CompleteLevel();
        }
    }

    public void Configure(KeyDoorController controller, GameObject doorVisual)
    {
        doorController = controller;
        doorClosedVisual = doorVisual;
    }
}
