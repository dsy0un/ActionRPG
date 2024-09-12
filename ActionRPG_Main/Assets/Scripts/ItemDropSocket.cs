using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropSocket : MonoBehaviour
{
    int itemNum;
    public Transform itemSocket;
    public int GetItemNum()
    {
        return itemNum;
    }
    public List<string> itemName = new();
    public string GetItemName()
    {
        return itemName[itemNum];
    }

    void Start()
    {
    }

    void Update()
    {
        
    }

    public void ItemNumberUpdate(int i)
    {
        itemNum = i;
    }
}
