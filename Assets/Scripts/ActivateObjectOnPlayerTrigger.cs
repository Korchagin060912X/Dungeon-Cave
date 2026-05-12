using UnityEngine;

public class ActivateObjectOnPlayerTrigger : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private bool oneTime = true;

    public void Configure(GameObject target, bool oneShot = true)
    {
        targetObject = target;
        oneTime = oneShot;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || targetObject == null)
        {
            return;
        }

        targetObject.SetActive(true);
        if (oneTime)
        {
            gameObject.SetActive(false);
        }
    }
}
