using System.Globalization;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class CrystalsTotalDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private TextMeshProUGUI glowDisplay;

    private void OnEnable()
    {
        PlayerData.OnUpdatedShards += Show;

        Show();
    }

    private void OnDisable()
    {
        PlayerData.OnUpdatedShards -= Show;
    }

    private void Show()
    {
        display.text = DataManager.Instance.PlayerData.TotalCrystals.ToString(CultureInfo.InvariantCulture);
        glowDisplay.text = DataManager.Instance.PlayerData.TotalCrystals.ToString(CultureInfo.InvariantCulture);
    }

    public void OnPointerEnter(PointerEventData _eventData)
    {
        glowDisplay.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData _eventData)
    {
        glowDisplay.gameObject.SetActive(false);
    }
}
