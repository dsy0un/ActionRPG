using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemButton : MonoBehaviour
{
    public Inventory inventory;
    public int buttonNum;
    public int itemNum;
    public int GetItemNum()
    {
        return itemNum;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ItemButtonNumberUpdate(int i)
    {
        buttonNum = i;
    }

    public void ItemNumberUpdate(int i)
    {
        itemNum = i;
    }

    public void ItemEquipButtonOn()
    {
        // 자식 객체가 있을 때 -> 아이템이 있을 때
        if (gameObject.transform.childCount > 0)
        {
            inventory.ItemEquipButtonOn(buttonNum, itemNum);
        }
    }
}
