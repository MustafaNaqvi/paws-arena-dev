using Newtonsoft.Json;
using System;

[Serializable]
public class CraftingProcess
{
    public LuckyWheelRewardType Ingridiant;
    public DateTime DateStarted;

    [JsonIgnore] public static Action OnFinishedCrafting;

    public string GetFinishTime()
    {
        CraftingRecepieSO _recepie = CraftingRecepieSO.Get(Ingridiant);
        DateTime _endDate = DateStarted.AddSeconds(_recepie.FusionTime);

        TimeSpan _endTime = _endDate - DateTime.UtcNow;

        if (_endTime.TotalSeconds < 0)
        {
            EndProduction();
            return "Craft";
        }

        float _secounds = _endTime.Seconds;
        float _minutes = _endTime.Minutes;
        float _hours = _endTime.Hours;

        string _finishText = string.Empty;
        _finishText += _hours < 10 ? "0" + _hours : _hours;
        _finishText += ":";
        _finishText += _minutes < 10 ? "0" + _minutes : _minutes;
        _finishText += ":";
        _finishText += _secounds < 10 ? "0" + _secounds : _secounds;

        return _finishText;
    }

    private void EndProduction()
    {
        CraftingRecepieSO _recepie = CraftingRecepieSO.Get(Ingridiant);
        DataManager.Instance.PlayerData.CraftingProcess = null;
        //todo increase amount of crystals? 
        OnFinishedCrafting?.Invoke();
    }
}
