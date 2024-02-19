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
        //delete, already implemented
        return;
        buy.onClick.AddListener(Buy);
    }

    private void OnDisable()
    {
        buy.onClick.RemoveListener(Buy);
    }

    private void Buy()
    {
        buy.interactable = false;
        BoomDaoUtility.Instance.ExecuteAction(BUY_SNACKS_ICP, HandleBoughtSnacks);
    }

    private void HandleBoughtSnacks(List<ActionOutcome> _)
    {
        buy.interactable = true;
    }
}
