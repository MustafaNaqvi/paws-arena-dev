using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

        commonButton.onClick.AddListener(() => ShowRecipe(ItemType.CommonShard));
        uncommonButton.onClick.AddListener(() => ShowRecipe(ItemType.UncommonShard));
        rareButton.onClick.AddListener(() => ShowRecipe(ItemType.RareShard));
        epicButton.onClick.AddListener(() => ShowRecipe(ItemType.EpicShard));
        legendaryButton.onClick.AddListener(() => ShowRecipe(ItemType.LegendaryShard));
        craftCrystalButton.onClick.AddListener(CraftCrystal);
        botCraftItemButton.onClick.AddListener(CraftItem);

        ShowRecipe(ItemType.CommonShard);
        gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        CraftingProcess.OnFinishedCrafting -= FinishedCrafting;

        commonButton.onClick.RemoveAllListeners();
        uncommonButton.onClick.RemoveAllListeners();
        rareButton.onClick.RemoveAllListeners();
        epicButton.onClick.RemoveAllListeners();
        legendaryButton.onClick.RemoveAllListeners();
        craftCrystalButton.onClick.RemoveListener(CraftCrystal);
        botCraftItemButton.onClick.RemoveListener(CraftItem);
    }

    private void ShowRecipe(ItemType _ingredient)
    {
        showingRecepie = CraftingRecepieSO.Get(_ingredient);

        if (_ingredient == ItemType.CommonShard)
        {
            messageDisplay.SetActive(true);
            ShowBotFrame(_ingredient);
            topHolder.SetActive(false);
            return;
        }
        
        messageDisplay.SetActive(false);
        topHolder.SetActive(true);

        CraftingRecepieSO _topRecipe = CraftingRecepieSO.Get((ItemType)((int)_ingredient-1));
        ingridiantImage.sprite = _topRecipe.EndProductSprite;
        craftText.text = $"Get 1 <color={_topRecipe.EndProductColor}>{_topRecipe.EndProduct}</color> shard by\ncombining {_topRecipe.AmountNeeded} <color={_topRecipe.IngridiantColor}>{_topRecipe.Inggrdiant}</color> shards";
        double _amountOfIngredients = DataManager.Instance.PlayerData.GetAmountOfCrystals(_topRecipe.Inggrdiant);
        if (_amountOfIngredients >= showingRecepie.AmountNeeded)
        {
            craftAmountDisplay.text = $"<color=#00ff00>{_amountOfIngredients}</color>/<color={_topRecipe.EndProductColor}>{_topRecipe.AmountNeeded}</color>";
            craftCrystalButton.interactable = true;
        }
        else
        {
            craftAmountDisplay.text = $"<color=#ff0000>{_amountOfIngredients}</color>/<color={_topRecipe.EndProductColor}>{_topRecipe.AmountNeeded}</color>";
            craftCrystalButton.interactable = false;
        }

        endResultImage.sprite = _topRecipe.IngridiantSprite;
        ingridiantImage.SetNativeSize();
        endResultImage.SetNativeSize();
        craftButtonText.text = "Craft";
        shardBackground.sprite = showingRecepie.TopOfferBackground;

        if (DataManager.Instance.PlayerData.CraftingProcess != null)
        {
            craftCrystalButton.interactable = false;
        }

        ShowBotFrame(_ingredient);
    }

    private void ShowBotFrame(ItemType _ingredient)
    {
        CraftingRecepieSO _recipe = CraftingRecepieSO.Get(_ingredient);
        botFrameText.text = $"Combine {_recipe.BotAmountNeeded} <color={_recipe.IngridiantColor}>{_recipe.Inggrdiant}</color> shards\nto get 1 <color={_recipe.IngridiantColor}>{_recipe.Inggrdiant}</color> item";
        double _amountGot = DataManager.Instance.PlayerData.GetAmountOfCrystals(_ingredient);
        botAmountDisplay.text = $"<color={_recipe.IngridiantColor}>{_amountGot}</color>/<color={showingRecepie.IngridiantColor}>{showingRecepie.BotAmountNeeded}</color>";
        if (_amountGot >= _recipe.BotAmountNeeded)
        {
            botCraftItemButton.interactable = true;
        }
        else
        {
            botCraftItemButton.interactable = false;
        }

        botFrameImage.sprite = _recipe.BottomOfferBackground;
    }

    private void CraftCrystal()
    {
        // CraftingRecepieSO _topRecipe = CraftingRecepieSO.Get((ItemType)((int)showingRecepie.Inggrdiant-1));
        //
        // CraftingProcess _craftingProcess = new CraftingProcess();
        // _craftingProcess.EndDate = DateTime.UtcNow;
        // _craftingProcess.EndProduct = _topRecipe.EndProduct;
        
        //todo call action on boomdao ui

        ShowRecipe(showingRecepie.Inggrdiant);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void FinishedCrafting(ItemType _endProduct)
    {
        craftingFinished.Setup($"Congratulations, you just crafted a {Utilities.GetItemName(_endProduct)} shard");
        ShowRecipe(showingRecepie.Inggrdiant);
        craftButtonText.text = "Craft";
        EventsManager.OnCraftedCrystal?.Invoke();
    }

    private void CraftItem()
    {
        return;
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
        CraftingProcess _process = DataManager.Instance.PlayerData.CraftingProcess;
        if (_process != default)
        {
            craftButtonText.text = _process.GetFinishTime();
        }
    }
}
