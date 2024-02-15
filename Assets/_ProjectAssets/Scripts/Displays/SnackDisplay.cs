using UnityEngine;
using TMPro;

public class SnackDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI snackAmountDisplay;

    private void OnEnable()
    {
        PlayerData.OnUpdatedSnacks += ShowSnacks;
        ShowSnacks();
    }

    private void OnDisable()
    {
        PlayerData.OnUpdatedSnacks -= ShowSnacks;
    }

    private void ShowSnacks()
    {
        snackAmountDisplay.text = DataManager.Instance.PlayerData.Snacks.ToString();
    }
}
