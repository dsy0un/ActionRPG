using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropManager : MonoBehaviour
{
    public List<GameObject> items = new();
    int itemNum;
    public int GetItemNum()
    {
        return itemNum;
    }
    public GameObject itemDrop; // 드롭 아이템 프리팹

    public Player player;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    /// <summary>
    /// 아이템을 드랍하는 함수
    /// </summary>
    /// <param name="pos">몬스터의 위치</param>
    public void ItemDropStart(Vector3 pos)
    {
        itemNum = Random.Range(0, items.Count); // 0부터 아이템 개수까지 무작위로 숫자 선출 (0 ~ 전체 아이템 개수 - 1)
        // 드롭 아이템 생성(원본, 위치, 회전, 부모)
        GameObject dropItem = Instantiate(itemDrop, pos, Quaternion.identity, transform);
        // 아이템 생성
        GameObject item = Instantiate(items[itemNum], dropItem.GetComponent<ItemDropSocket>().itemSocket.position, dropItem.GetComponent<ItemDropSocket>().itemSocket.rotation, dropItem.GetComponent<ItemDropSocket>().itemSocket);
        dropItem.GetComponent<ItemDropSocket>().ItemNumberUpdate(itemNum);
    }

    public void PlayerItemEquip(int i)
    {
        player.ItemEquip(items[i]);
    }
}
