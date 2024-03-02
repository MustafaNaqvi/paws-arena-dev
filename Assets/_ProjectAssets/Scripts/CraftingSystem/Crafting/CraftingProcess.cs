using System;
using UnityEngine;

[Serializable]
public class CraftingProcess
{
    public static Action<ItemType> OnFinishedCrafting;
    
    public DateTime EndDate;
    public ItemType EndProduct;


    public string GetFinishTime()
    {
        TimeSpan _endTime = EndDate - DateTime.UtcNow;
        if (_endTime.TotalSeconds < 0)
        {
            OnFinishedCrafting?.Invoke(EndProduct);
            return "Craft";
        }

        float _seconds = _endTime.Seconds;
        float _minutes = _endTime.Minutes;
        float _hours = _endTime.Hours;

        string _finishText = string.Empty;
        _finishText += _hours < 10 ? "0" + _hours : _hours;
        _finishText += ":";
        _finishText += _minutes < 10 ? "0" + _minutes : _minutes;
        _finishText += ":";
        _finishText += _seconds < 10 ? "0" + _seconds : _seconds;

        return _finishText;
    }

}
