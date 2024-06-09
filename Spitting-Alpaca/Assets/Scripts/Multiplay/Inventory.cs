using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<Inventory>();
            }

            return m_instance;
        }
    }

    private static Inventory m_instance;
    private string gooditem = "";

    void Start()
    {
       
    }

    public void AddItem(GameObject item)
    {
        gooditem = item.name;
        Debug.Log("아이템 획득: " + item.name);
    }
    //굿아이템사용
    public string useGooditem()
    {
        Debug.Log("아이템사용");
        string realgooditem = gooditem;
        gooditem = "";
        return realgooditem;
    }
}
