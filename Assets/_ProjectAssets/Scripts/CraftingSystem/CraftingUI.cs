using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] private Button commonButton;
    [SerializeField] private Button uncommonButton;
    [SerializeField] private Button rareButton;
    [SerializeField] private Button epicButton;
    [SerializeField] private Button legendaryButton;

    //top frame
    [SerializeField] private GameObject topHolder;
    [SerializeField] private Image ingridiantImage;
    [SerializeField] private TextMeshProUGUI craftText;
    [SerializeField] private TextMeshProUGUI craftAmountDisplay;
    [SerializeField] private Image endResultImage;
    [SerializeField] private Button craftCrystalButton;
    [SerializeField] private TextMeshProUGUI craftButtonText;
    [SerializeField] private Image shardBackground;
    [SerializeField] private GameObject messageDisplay;

    //bot frame
    [SerializeField] private Image botFrameImage;
    [SerializeField] private TextMeshProUGUI botFrameText;
    [SerializeField] private TextMeshProUGUI botAmountDisplay;
    [SerializeField] private Button botCraftItemButton;

    [Space]
    [Space]
    [SerializeField]
    private EquipmentsConfig equipments;
    [SerializeField] private CraftedItemDisplay itemDisplay;
    [SerializeField] private CraftFinishedDisplay craftingFinished;
    private CraftingRecepieSO showingRecepie;
    
    public void Setup()
    {
        CraftingProcess.OnFinishedCrafting += FinishedCrafting;

        commonButton.onClick.AddListener(() => ShowRecepie(LuckyWheelRewardType.Common));
        uncommonButton.onClick.AddListener(() => ShowRecepie(LuckyWheelRewardType.Uncommon));
        rareButton.onClick.AddListener(() => ShowRecepie(LuckyWheelRewardType.Rare));
        epicButton.onClick.AddListener(() => ShowRecepie(LuckyWheelRewardType.Epic));
        legendaryButton.onClick.AddListener(() => ShowRecepie(LuckyWheelRewardType.Legendary));
        craftCrystalButton.onClick.AddListener(CraftCrystal);
        botCraftItemButton.onClick.AddListener(CraftItem);

        ShowRecepie(LuckyWheelRewardType.Common);
        ShowRecepie();
        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        CraftingProcess.OnFinishedCrafting -= FinishedCrafting;

        craftCrystalButton.onClick.RemoveListener(CraftCrystal);
        commonButton.onClick.RemoveAllListeners();
        uncommonButton.onClick.RemoveAllListeners();
        rareButton.onClick.RemoveAllListeners();
        epicButton.onClick.RemoveAllListeners();
        legendaryButton.onClick.RemoveAllListeners();
        botCraftItemButton.onClick.RemoveListener(CraftItem);
    }

    private void ShowRecepie()
    {
        ShowRecepie(showingRecepie.Inggrdiant);
    }

    private void ShowRecepie(LuckyWheelRewardType _ingridiant)
    {
        showingRecepie = CraftingRecepieSO.Get(_ingridiant);

        if (_ingridiant == LuckyWheelRewardType.Common)
        {
            messageDisplay.SetActive(true);
            ShowBotFrame(_ingridiant);
            topHolder.SetActive(false);
            return;
        }
        
        messageDisplay.SetActive(false);

        topHolder.SetActive(true);

        CraftingRecepieSO _topRecepie = CraftingRecepieSO.Get((LuckyWheelRewardType)((int)_ingridiant-1));
        ingridiantImage.sprite = _topRecepie.EndProductSprite;
        craftText.text = $"Get 1 <color={_topRecepie.EndProductColor}>{_topRecepie.EndProduct}</color> shard by\ncombining {_topRecepie.AmountNeeded} <color={_topRecepie.IngridiantColor}>{_topRecepie.Inggrdiant}</color> shards";
        double _amountOfIngridiants = DataManager.Instance.PlayerData.GetAmountOfCrystals(_topRecepie.Inggrdiant);
        if (_amountOfIngridiants >= showingRecepie.AmountNeeded)
        {
            craftAmountDisplay.text = $"<color=#00ff00>{_amountOfIngridiants}</color>/<color={_topRecepie.EndProductColor}>{_topRecepie.AmountNeeded}</color>";
            craftCrystalButton.interactable = true;
        }
        else
        {
            craftAmountDisplay.text = $"<color=#ff0000>{_amountOfIngridiants}</color>/<color={_topRecepie.EndProductColor}>{_topRecepie.AmountNeeded}</color>";
            craftCrystalButton.interactable = false;
        }

        endResultImage.sprite = _topRecepie.IngridiantSprite;
        ingridiantImage.SetNativeSize();
        endResultImage.SetNativeSize();
        craftButtonText.text = "Craft";
        shardBackground.sprite = showingRecepie.TopOfferBackground;

        if (DataManager.Instance.PlayerData.CraftingProcess != null)
        {
            craftCrystalButton.interactable = false;
        }

        ShowBotFrame(_ingridiant);
    }

    private void ShowBotFrame(LuckyWheelRewardType _ingridiant)
    {
        CraftingRecepieSO _recepie = CraftingRecepieSO.Get(_ingridiant);
        botFrameText.text = $"Combine {_recepie.BotAmountNeeded} <color={_recepie.IngridiantColor}>{_recepie.Inggrdiant}</color> shards\nto get 1 <color={_recepie.IngridiantColor}>{_recepie.Inggrdiant}</color> item";
        double _amountGot = DataManager.Instance.PlayerData.GetAmountOfCrystals(_ingridiant);
        botAmountDisplay.text = $"<color={_recepie.IngridiantColor}>{_amountGot}</color>/<color={showingRecepie.IngridiantColor}>{showingRecepie.BotAmountNeeded}</color>";
        if (_amountGot >= _recepie.BotAmountNeeded)
        {
            botCraftItemButton.interactable = true;
        }
        else
        {
            botCraftItemButton.interactable = false;
        }

        botFrameImage.sprite = _recepie.BottomOfferBackground;
    }

    private void CraftCrystal()
    {
        CraftingRecepieSO _topRecepie = CraftingRecepieSO.Get((LuckyWheelRewardType)((int)showingRecepie.Inggrdiant-1));

        CraftingProcess _craftingProcess = new CraftingProcess();
        _craftingProcess.DateStarted = DateTime.UtcNow;
        _craftingProcess.Ingridiant = _topRecepie.Inggrdiant;

        DataManager.Instance.PlayerData.CraftingProcess = _craftingProcess;
        
        //todo deduce the _topRecepie.Inggrdiant type of crystal

        ShowRecepie(showingRecepie.Inggrdiant);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void FinishedCrafting()
    {
        craftingFinished.Setup($"Congratulations, you just crafted a {showingRecepie.EndProduct} shard");
        ShowRecepie();
        ShowRecepie(showingRecepie.Inggrdiant);
        craftButtonText.text = "Craft";
        EventsManager.OnCraftedCrystal?.Invoke();
    }

    private void CraftItem()
    {
        EquipmentData _equipmentData = equipments.CraftItem(showingRecepie);
        ShowItem(_equipmentData);
        DataManager.Instance.PlayerData.AddOwnedEquipment(_equipmentData.Id);
        //todo decrease amount of showingRecepie.Inggrdiant type cristals
        EventsManager.OnCraftedItem?.Invoke();
    }

    private void ShowItem(EquipmentData _equipmentData)
    {
        itemDisplay.Setup(_equipmentData);
    }

    private void Update()
    {
        if (DataManager.Instance.PlayerData.CraftingProcess != null)
        {
            craftButtonText.text = DataManager.Instance.PlayerData.CraftingProcess.GetFinishTime();
        }
    }
}
