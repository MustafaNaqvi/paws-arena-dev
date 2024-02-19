using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BoomDaoWrapper;

public class BuyMilk : MonoBehaviour
{
    private const string BUY_MILK_BOTTLE = "buyMilkBottleIcp";
    private const string BUY_MILK_GLASS = "buyMilkGlassIcp";
    
    [SerializeField] private Button doneButton;
    [SerializeField] private Button buyJugOfMilkButton;
    [SerializeField] private Button buyGlassOfMilkButton;

    [SerializeField] private TextMeshProUGUI jugOfMilkDisplay;
    [SerializeField] private TextMeshProUGUI glassOfMilkDisplay;

    [SerializeField] private Color normalAmountColor;
    [SerializeField] private Color zeroAmountColor;

    [SerializeField] private TextMeshProUGUI glassOfMilkPriceDisplay;
    [SerializeField] private TextMeshProUGUI jugOfMilkPriceDisplay;

    public void Setup()
    {
        ShowGlassOfMilk();
        ShowJugOfMilk();

        doneButton.onClick.AddListener(Done);
        buyJugOfMilkButton.onClick.AddListener(BuyJugOfMilk);
        buyGlassOfMilkButton.onClick.AddListener(BuyGlassOfMIlk);

        PlayerData.OnUpdatedJugOfMilk += ShowJugOfMilk;
        PlayerData.OnUpdatedGlassOfMilk += ShowGlassOfMilk;

        glassOfMilkPriceDisplay.text = DataManager.Instance.GameData.GlassOfMilkPrice.ToString();
        jugOfMilkPriceDisplay.text = DataManager.Instance.GameData.JugOfMilkPrice.ToString();

        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        doneButton.onClick.AddListener(Done);
        buyJugOfMilkButton.onClick.AddListener(BuyJugOfMilk);
        buyGlassOfMilkButton.onClick.AddListener(BuyGlassOfMIlk);

        PlayerData.OnUpdatedJugOfMilk -= ShowJugOfMilk;
        PlayerData.OnUpdatedGlassOfMilk -= ShowGlassOfMilk;
    }

    private void ShowJugOfMilk()
    {
        jugOfMilkDisplay.text = DataManager.Instance.PlayerData.JugOfMilk.ToString();
        jugOfMilkDisplay.color = DataManager.Instance.PlayerData.JugOfMilk == 0 ? zeroAmountColor : normalAmountColor;
    }

    private void ShowGlassOfMilk()
    {
        glassOfMilkDisplay.text = DataManager.Instance.PlayerData.GlassOfMilk.ToString();
        glassOfMilkDisplay.color = DataManager.Instance.PlayerData.GlassOfMilk == 0 ? zeroAmountColor : normalAmountColor;
    }

    private void BuyJugOfMilk()
    {
        ManageInteractables(false);
        BoomDaoUtility.Instance.ExecuteAction(BUY_MILK_BOTTLE, HandleBuyOutcome);
    }

    private void BuyGlassOfMIlk()
    {
        ManageInteractables(false);

        BoomDaoUtility.Instance.ExecuteAction(BUY_MILK_GLASS, HandleBuyOutcome);
    }

    private void HandleBuyOutcome(List<ActionOutcome> _outcomes)
    {
        ManageInteractables(true);
    }

    private void Done()
    {
        gameObject.SetActive(false);
    }

    private void ManageInteractables(bool _status)
    {
        buyJugOfMilkButton.interactable = _status;
        buyGlassOfMilkButton.interactable = _status;
    }
}
