using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaUI : MonoBehaviour
{
    [SerializeField] private PlayerController2D player;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Text stateText;

    public void Configure(PlayerController2D playerController, Slider slider, Text status)
    {
        player = playerController;
        staminaSlider = slider;
        stateText = status;
    }

    private void Update()
    {
        if (player == null || staminaSlider == null)
        {
            return;
        }

        staminaSlider.value = player.Stamina01;
        if (stateText != null)
        {
            int value = Mathf.RoundToInt(player.Stamina01 * 100f);
            stateText.text = player.IsTired ? $"Устал {value}%" : $"Бег {value}%";
            stateText.color = player.IsTired ? Color.red : Color.white;
        }
    }
}
