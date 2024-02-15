using System.Collections.Generic;
using BoomDaoWrapper;
using UnityEngine;
using UnityEngine.UI;

public class BuySnacks : MonoBehaviour
{
    private const string BUY_SNACKS_ICP = "buySnackIcp";
    [SerializeField] private Button buy;

    private void OnEnable()
    {
        buy.onClick.AddListener(Buy);
    }

    private void OnDisable()
    {
        buy.onClick.RemoveListener(Buy);
    }

    private void Buy()
    {
        BoomDaoUtility.Instance.ExecuteAction(BUY_SNACKS_ICP, HandleBoughtSnacks);
    }

    private void HandleBoughtSnacks(List<ActionOutcome> _outcomes)
    {
        if (_outcomes==default || _outcomes.Count==0)
        {
            return;
        }
        
        Debug.Log("---- Bought snacks");
        foreach (var _outcome in _outcomes)
        {
            Debug.Log($"---- {_outcome.Name} : {_outcome.Value}");
        }
    }
}
