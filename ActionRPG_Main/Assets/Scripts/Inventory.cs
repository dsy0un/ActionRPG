using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public GameObject inventory;
    public List<Button> weaponButton = new();
    public GameObject itemUI;
    public ItemDropManager itemDropManager;
    public Transform itemWeaponPos;
    int itemNum;

    public Transform itemSocket; // 아이템 소켓
    public GameObject itemDropSocket;

    public GameObject itemCam;
    public List<RenderTexture> rtxList = new();
    public List<ItemCamera> itemCamList = new();

    public Transform jointItem;
    GameObject jointWeapon;

    GameObject handItem;

    void Start()
    {
        for (int i = 0; i < weaponButton.Count; i++)
        {
            weaponButton[i].GetComponent<ItemButton>().ItemButtonNumberUpdate(i);
        }
    }

    void Update()
    {

    }

    public void InventoryOpen(bool b)
    {
        inventory.SetActive(b);
    }

    public void InventoryUpdate(GameObject g)
    {
        GameObject itemCamera = Instantiate(itemCam, new Vector3(itemWeaponPos.position.x, itemWeaponPos.position.y - (itemNum * 20), itemWeaponPos.position.z), itemWeaponPos.rotation);
        itemCamera.GetComponent<ItemCamera>().itemCam.targetTexture = rtxList[itemNum];
        itemCamList.Add(itemCamera.GetComponent<ItemCamera>());

        // 게임 아이템 렌더 텍스처 등록
        GameObject item = Instantiate(g, itemCamera.GetComponent<ItemCamera>().itemPos.position, itemCamera.GetComponent<ItemCamera>().itemPos.rotation, itemCamera.GetComponent<ItemCamera>().itemPos);

        while (weaponButton[itemNum].transform.childCount == 0)
        {
            GameObject itemButton = Instantiate(itemUI, weaponButton[itemNum].transform.position, weaponButton[itemNum].transform.rotation, weaponButton[itemNum].transform);
            itemButton.GetComponent<RawImage>().texture = rtxList[itemNum];
            weaponButton[itemNum].GetComponent<ItemButton>().ItemNumberUpdate(itemDropSocket.GetComponent<ItemDropSocket>().GetItemNum());
        }
        itemNum++;
    }

    public void ItemEquipButtonOn(int i, int j)
    {
        if (handItem)
        {
            Destroy(handItem);
            handItem = null;
        }
        GameObject itemButton = Instantiate(itemUI, itemSocket.position, itemSocket.rotation, itemSocket);
        itemButton.GetComponent<RawImage>().texture = rtxList[i];
        itemButton.transform.localScale = itemButton.transform.localScale * 2;
        handItem = itemButton;

        if (jointWeapon)
        {
            Destroy(jointWeapon);
            jointWeapon = null;
        }
        GameObject weapon = Instantiate(itemDropManager.items[j], jointItem.position, jointItem.rotation, jointItem);
        weapon.layer = 9;
        jointWeapon = weapon;

        itemDropManager.PlayerItemEquip(j);
    }
}
