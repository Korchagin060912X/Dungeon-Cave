using UnityEngine;
using UnityEngine.UI;

public class MinecartRideController : MonoBehaviour
{
    [SerializeField] private Transform[] routePoints;
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool autoStartOnPlayerEnter = true;

    private int targetIndex;
    private bool isMoving;
    private bool hasStartedRide;
    private Transform rider;
    private Vector3 lastPosition;
    private GameObject ridePanel;
    private Button rideButton;
    private GameObject actionPanel;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (!isMoving || routePoints == null || routePoints.Length == 0)
        {
            lastPosition = transform.position;
            return;
        }

        Transform target = routePoints[targetIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;

        if (rider != null)
        {
            rider.position += new Vector3(delta.x, 0f, 0f);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            targetIndex++;
            if (targetIndex >= routePoints.Length)
            {
                isMoving = false;
                rider = null;
                SetActionPanelVisible(false);
            }
        }
    }

    public void StartRide()
    {
        if (routePoints == null || routePoints.Length == 0)
        {
            return;
        }

        if (isMoving)
        {
            return;
        }

        if (rider == null)
        {
            return;
        }

        targetIndex = 1;
        isMoving = true;
        hasStartedRide = true;
        lastPosition = transform.position;
        SetRideButtonVisible(false);
        SetActionPanelVisible(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || hasStartedRide)
        {
            return;
        }

        rider = other.transform;
        if (autoStartOnPlayerEnter)
        {
            StartRide();
        }
        else
        {
            SetRideButtonVisible(true);
            SetActionPanelVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (hasStartedRide)
        {
            return;
        }

        if (rider != null && other.transform == rider)
        {
            rider = null;
            SetRideButtonVisible(false);
            SetActionPanelVisible(false);
        }
    }

    public void Configure(Transform[] points, float moveSpeed, bool startAutomatically)
    {
        routePoints = points;
        speed = moveSpeed;
        autoStartOnPlayerEnter = startAutomatically;
        hasStartedRide = false;
    }

    public void ConfigureRideButton(GameObject panel, Button button)
    {
        ridePanel = panel;
        rideButton = button;
        if (rideButton != null)
        {
            rideButton.onClick.RemoveAllListeners();
            rideButton.onClick.AddListener(StartRide);
        }

        SetRideButtonVisible(false);
    }

    public void ConfigureActionPanel(GameObject panel)
    {
        actionPanel = panel;
        SetActionPanelVisible(false);
    }

    private void SetRideButtonVisible(bool visible)
    {
        if (ridePanel != null)
        {
            ridePanel.SetActive(visible && !hasStartedRide);
        }
    }

    private void SetActionPanelVisible(bool visible)
    {
        if (actionPanel != null)
        {
            actionPanel.SetActive(visible);
        }
    }
}
