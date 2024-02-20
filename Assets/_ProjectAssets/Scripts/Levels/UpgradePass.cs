using System.Collections.Generic;
using BoomDaoWrapper;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePass : MonoBehaviour
{
    private const string UPGRADE = "buyPremiumPassIcp";
    [SerializeField] private Button upgrade;

    private void OnEnable()
    {
        upgrade.onClick.AddListener(Upgrade);
        upgrade.interactable = !DataManager.Instance.PlayerData.HasPass;
    }

    private void OnDisable()
    {
        upgrade.onClick.RemoveListener(Upgrade);
    }

    private void Upgrade()
    {
        upgrade.interactable = false;
        BoomDaoUtility.Instance.ExecuteAction(UPGRADE, HandleUpgrade);
    }

    private void HandleUpgrade(List<ActionOutcome> _outcomes)
    {
        upgrade.interactable = true;
        if (_outcomes==default || _outcomes.Count==0)
        {
            return;
        }

        upgrade.interactable = false;
    }
}
