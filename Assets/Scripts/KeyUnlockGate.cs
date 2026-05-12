using UnityEngine;

public class KeyUnlockGate : MonoBehaviour
{
    [SerializeField] private KeyDoorController keyController;
    [SerializeField] private GameObject gateObject;

    public void Configure(KeyDoorController controller, GameObject gate)
    {
        keyController = controller;
        gateObject = gate;
    }

    private void Update()
    {
        if (keyController == null || gateObject == null)
        {
            return;
        }

        if (keyController.CanOpenDoor())
        {
            gateObject.SetActive(false);
            enabled = false;
        }
    }
}
