using UnityEngine;

public class MinimapMarker2D : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform marker;
    [SerializeField] private Vector2 worldMin = new Vector2(-15f, -15f);
    [SerializeField] private Vector2 worldMax = new Vector2(15f, 15f);
    [SerializeField] private Vector2 mapMin = new Vector2(-80f, -80f);
    [SerializeField] private Vector2 mapMax = new Vector2(80f, 80f);

    private void Update()
    {
        if (player == null || marker == null)
        {
            return;
        }

        float tX = Mathf.InverseLerp(worldMin.x, worldMax.x, player.position.x);
        float tY = Mathf.InverseLerp(worldMin.y, worldMax.y, player.position.y);
        marker.anchoredPosition = new Vector2(
            Mathf.Lerp(mapMin.x, mapMax.x, tX),
            Mathf.Lerp(mapMin.y, mapMax.y, tY)
        );
    }

    public void Configure(Transform targetPlayer, RectTransform targetMarker, Vector2 wMin, Vector2 wMax, Vector2 mMin, Vector2 mMax)
    {
        player = targetPlayer;
        marker = targetMarker;
        worldMin = wMin;
        worldMax = wMax;
        mapMin = mMin;
        mapMax = mMax;
    }
}
