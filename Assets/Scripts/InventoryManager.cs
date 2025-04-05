using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public List<string> items = new List<string>();

    public void AddItem(string oreType)
    {
        items.Add(oreType);
        Debug.Log($"Added {oreType} to inventory. Total items: {items.Count}");
    }
}
