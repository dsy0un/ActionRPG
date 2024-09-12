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
    public GameObject itemDrop; // ��� ������ ������

    public Player player;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    /// <summary>
    /// �������� ����ϴ� �Լ�
    /// </summary>
    /// <param name="pos">������ ��ġ</param>
    public void ItemDropStart(Vector3 pos)
    {
        itemNum = Random.Range(0, items.Count); // 0���� ������ �������� �������� ���� ���� (0 ~ ��ü ������ ���� - 1)
        // ��� ������ ����(����, ��ġ, ȸ��, �θ�)
        GameObject dropItem = Instantiate(itemDrop, pos, Quaternion.identity, transform);
        // ������ ����
        GameObject item = Instantiate(items[itemNum], dropItem.GetComponent<ItemDropSocket>().itemSocket.position, dropItem.GetComponent<ItemDropSocket>().itemSocket.rotation, dropItem.GetComponent<ItemDropSocket>().itemSocket);
        dropItem.GetComponent<ItemDropSocket>().ItemNumberUpdate(itemNum);
    }

    public void PlayerItemEquip(int i)
    {
        player.ItemEquip(items[i]);
    }
}
