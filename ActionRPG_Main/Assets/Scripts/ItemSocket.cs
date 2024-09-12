using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSocket : MonoBehaviour
{
    public ItemDropSocket itemDropSocket;

    void Start()
    {
        
    }

    void Update()
    {

    }

    public string ItemView()
    {
        return itemDropSocket.GetItemName();
    }
}
