using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuantumContainer : MonoBehaviour
{
    public List<QuantumInventory.Slot> inventory;
    public int maxSlots;
    public AudioClip open, close, lockState;
    public string locked;

    private void Start()
    {
        if (maxSlots > 20) maxSlots = 20;
        if (inventory.Count > 20) while (inventory.Count > 20) inventory.RemoveAt(inventory.Count - 1);
    }

    public void Gather(QuantumInventory.Slot item)
    {
        if (inventory.Count >= maxSlots)
            return;
        QuantumInventory.Slot slot = FindSlot(item.item);
        if (slot == null || !slot.stackable)
            inventory.Add(new QuantumInventory.Slot(item.item, item.type, item.quantity, item.icon, item.stackable, item.metaData));
        else if (slot != null && slot.stackable)
            slot.quantity += item.quantity;
    }

    public QuantumInventory.Slot FindSlot(string item)
    {
        foreach (QuantumInventory.Slot slot in inventory)
            if (slot.item == item)
                return slot;
        return null;
    }

    public void RemoveSlot(string item)
    {
        foreach (QuantumInventory.Slot slot in inventory)
            if (slot.item == item)
                inventory.Remove(slot);
    }

    public void PlayFX(AudioClip fx)
    {
        if (fx == null)
            return;
        GameObject obj = new GameObject();
        obj.transform.position = transform.position;
        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = fx;
        source.Play();
        Destroy(obj, fx.length);
    }
}
