using System.Globalization;
using TMPro;
using UnityEngine;

public class CrystalsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI amountDisplay;
    [SerializeField] private LuckyWheelRewardType type;

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
        amountDisplay.text = DataManager.Instance.PlayerData.GetAmountOfCrystals(type).ToString(CultureInfo.InvariantCulture);
    }
}
