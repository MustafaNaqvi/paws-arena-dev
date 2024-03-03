using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetsManager : MonoBehaviour
{
    public static AssetsManager Instance;
    [SerializeField] private List<ItemSprite> items;
    
    private void Awake()
    {
        if (Instance==null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Sprite GetItemSprite(ItemType _type)
    {
        return items.Find(_item => _item.Type == _type).Sprite;
    }
}