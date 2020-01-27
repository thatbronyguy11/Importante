using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuantumInventory : MonoBehaviour
{
    [System.NonSerialized] public List<Slot> inventory, hotbar, slots;
    public int maxSlots;
    public KeyCode interact, action;
    public AudioClip open, close, pickUp, moveSlot, drop;
    public float distance;

    GameObject inventoryObj;

    GameObject[] go;
    // [0 SLOT][1 INVENTORY][2 HOTBAR][3 ITEM]

    Transform[] t;
    // [0 CONTAINER][1 INVENTORY][2 HOTBAR][3 SLOTS][4 OPTIONS][5 DOC VIEW]

    Sprite error;
    
    bool o;
    Transform playerCamera, canvas, invSlots;
    PlayerMove pm;
    PlayerLook pl;
    CanvasScaler cs;
    Dropdown sort;
    Text info;
    QuantumContainer quantumContainer;

    private void Start()
    {
        o = !o;
        inventory = new List<Slot>();
        hotbar = new List<Slot>();
        slots = new List<Slot>();

        foreach (Transform child in transform)
            if (child.GetComponent<Camera>() != null)
                playerCamera = child;

        foreach (Transform child in transform)
            if (child.GetComponent<PlayerLook>() != null)
                pl = child.GetComponent<PlayerLook>();

        if (GetComponent<PlayerMove>() != null)
            pm = GetComponent<PlayerMove>();

        if (maxSlots > 40)
            maxSlots = 40;

        canvas = GameObject.Find("Canvas").transform;
        cs = canvas.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1280, 720);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 0.5f;

        go = new GameObject[5];
        t = new Transform[7];
        go[0] = Resources.Load<GameObject>("Core/QIS/_slot");
        go[1] = Resources.Load<GameObject>("Core/QIS/_inventory");
        go[2] = Resources.Load<GameObject>("Core/QIS/_hotbar");
        go[3] = Resources.Load<GameObject>("Core/QIS/_erit");
        error = Resources.Load<Sprite>("Core/QIS/_ertex");
        t[1] = Instantiate<GameObject>(go[1], canvas).transform;
        invSlots = t[1].Find("_slots");
        t[0] = t[1].Find("_container");
        t[4] = t[1].Find("_options");
        t[3] = t[4].Find("_moreSlots");
        t[5] = t[1].Find("_docViewer");
        t[2] = Instantiate<GameObject>(go[2], canvas).transform;
        t[6] = t[4].Find("_info");
        sort = t[4].Find("_sort").GetComponent<Dropdown>();
        sort.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { RefreshInventory(); });
        info = t[6].Find("Text").GetComponent<Text>();
        t[1].gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(interact))
        {
            RaycastHit hit;
            int layer = gameObject.layer;
            gameObject.layer = 2;
            if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, distance))
            {
                if (hit.collider.GetComponent<QuantumItem>() != null)
                    Gather(hit.collider.GetComponent<QuantumItem>());
                else if (hit.collider.GetComponent<QuantumContainer>() != null)
                    Container(hit.collider.GetComponent<QuantumContainer>());
            }
            gameObject.layer = layer;
        }

        if (Input.GetKeyDown(action))
        {
            Freeze();
            ActionInventory();
            SetActive(false, true, false);
        }
    }

    public void Freeze()
    {
        o = !o;

        if (pm != null && pl != null)
        { pm.enabled = o; pl.enabled = o; }

        if (o)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Container(QuantumContainer container)
    {
        if (container.locked != "")
        {
            if (!FindMetaData("Key", container.locked))
            {
                container.PlayFX(container.lockState);
                return;
            }
        }

        Freeze();
        ActionInventory();

        SetActive(true, false, false);
        quantumContainer = container;
        quantumContainer.PlayFX(quantumContainer.open);

        RefreshInventory();
    }

    public void ActionInventory()
    {
        t[1].gameObject.SetActive(!o);
        if (t[1].gameObject.activeSelf)
            PlayFX(open);
        else
            PlayFX(close);
        if (t[0].gameObject.activeSelf)
            quantumContainer.PlayFX(quantumContainer.close);
        SetActive(false, true, false);
        RefreshInventory();
    }

    private void DeNuller ()
    {
        foreach (Transform child in invSlots)
            Destroy(child.gameObject);
        foreach (Transform child in t[0])
            Destroy(child.gameObject);
        foreach (Transform child in t[2])
            Destroy(child.gameObject);
        foreach (Transform child in t[3])
            Destroy(child.gameObject);
    }

    private void RefreshInventory()
    {
        DeNuller();

        if (sort.value == 0) //NORMAL
        {
            foreach (Slot slot in inventory)
            {
                GameObject x = Instantiate(go[0], invSlots);
                if (slot.icon != null)
                    x.transform.Find("Icon").GetComponent<Image>().sprite = slot.icon;
                else
                    x.transform.Find("Icon").GetComponent<Image>().sprite = error;
                x.transform.Find("Type").GetComponent<Image>().sprite = Resources.Load<Sprite>("Core/QIS/" + slot.type);
                if (slot.quantity > 1)
                    x.transform.Find("Text").GetComponent<Text>().text = slot.quantity.ToString("");
                else
                    x.transform.Find("Text").GetComponent<Text>().text = "";
                x.GetComponent<Button>().onClick.AddListener(delegate { Action(slot); });
            }
        }
        else // TYPE [ITEM-DOCUMENT-KEY-CONSUMABLE-SLOT-CUSTOM]
        {
            InstantiateSlot("Item");
            InstantiateSlot("Document");
            InstantiateSlot("Key");
            InstantiateSlot("Consumable");
            InstantiateSlot("Slot");
            InstantiateSlot("Custom");
        }

        foreach (Slot slot in hotbar)
        {
            GameObject x = Instantiate(go[0], t[2]);
            if (slot.icon != null)
                x.transform.Find("Icon").GetComponent<Image>().sprite = slot.icon;
            else
                x.transform.Find("Icon").GetComponent<Image>().sprite = error;
            x.transform.Find("Type").GetComponent<Image>().sprite = Resources.Load<Sprite>("Core/QIS/" + slot.type);
            if (slot.quantity != 1)
                x.transform.Find("Text").GetComponent<Text>().text = slot.quantity.ToString("");
            else
                x.transform.Find("Text").GetComponent<Text>().text = "";
            x.GetComponent<Button>().onClick.AddListener(delegate { HotbarAction(slot); });
        }

        foreach (Slot slot in slots)
        {
            GameObject x = Instantiate(go[0], t[3]);
            if (slot.icon != null)
                x.transform.Find("Icon").GetComponent<Image>().sprite = slot.icon;
            else
                x.transform.Find("Icon").GetComponent<Image>().sprite = error;
            x.transform.Find("Type").GetComponent<Image>().sprite = Resources.Load<Sprite>("Core/QIS/" + slot.type);
            if (slot.quantity != 1)
                x.transform.Find("Text").GetComponent<Text>().text = slot.quantity.ToString("");
            else
                x.transform.Find("Text").GetComponent<Text>().text = "";
            x.GetComponent<Button>().onClick.AddListener(delegate { SlotAction(slot); });
        }

        if (t[0].gameObject.activeSelf)
        {
            foreach (Slot slot in quantumContainer.inventory)
            {
                GameObject x = Instantiate(go[0], t[0]);
                if (slot.icon != null)
                    x.transform.Find("Icon").GetComponent<Image>().sprite = slot.icon;
                else
                    x.transform.Find("Icon").GetComponent<Image>().sprite = error;
                x.transform.Find("Type").GetComponent<Image>().sprite = Resources.Load<Sprite>("Core/QIS/" + slot.type);
                if (slot.quantity != 1)
                    x.transform.Find("Text").GetComponent<Text>().text = slot.quantity.ToString("");
                else
                    x.transform.Find("Text").GetComponent<Text>().text = "";
                x.GetComponent<Button>().onClick.AddListener(delegate { ContainerAction(slot); });
            }
        }

        info.text = "Max Slots: " + maxSlots;
        info.text += "\nUsed Slots: " + inventory.Count;
        info.text += "\nFree Slots: " + (maxSlots - inventory.Count);
        info.text += "\n\nHotbar Max Slots: 9";
        info.text += "\nHotbar Used Space: " + hotbar.Count;
        info.text += "\nHotbar Free Space: " + (9 - hotbar.Count);
        int additionalSlots = 0;
        foreach (Slot slot in slots)
            additionalSlots += int.Parse(slot.metaData);
        info.text += "\n\nAdditional Slots: " + additionalSlots;
    }

    private void InstantiateSlot(string type)
    {
        foreach (Slot slot in inventory)
        {
            if (slot.type == type)
            {
                GameObject x = Instantiate(go[0], invSlots);
                if (slot.icon != null)
                    x.transform.Find("Icon").GetComponent<Image>().sprite = slot.icon;
                else
                    x.transform.Find("Icon").GetComponent<Image>().sprite = error;
                x.transform.Find("Type").GetComponent<Image>().sprite = Resources.Load<Sprite>("Core/QIS/" + slot.type);
                if (slot.quantity != 1)
                    x.transform.Find("Text").GetComponent<Text>().text = slot.quantity.ToString("");
                else
                    x.transform.Find("Text").GetComponent<Text>().text = "";
                x.GetComponent<Button>().onClick.AddListener(delegate { Action(slot); });
            }
        }
    }

    private void Action(Slot slot)
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Drop(slot);
            inventory.Remove(slot);
            PlayFX(moveSlot);
        }
        else
        {
            if (t[0].gameObject.activeSelf)
            {
                if (quantumContainer.inventory.Count >= quantumContainer.maxSlots)
                    return;
                quantumContainer.Gather(slot);
                inventory.Remove(slot);
                PlayFX(moveSlot);
            }
            else
            {
                switch (slot.type)
                {
                    case "Document":
                        SetActive(false, false, true);
                        t[5].Find("Text").GetComponent<Text>().text = slot.metaData;
                        break;
                    case "Slot":
                        SetActive(false, true, false);
                        ChangeMaxSlots(int.Parse(slot.metaData));
                        slots.Add(slot);
                        inventory.Remove(slot);
                        PlayFX(moveSlot);
                        break;
                    default:
                        if (hotbar.Count >= 9)
                            return;
                        GatherHotbar(slot);
                        inventory.Remove(slot);
                        PlayFX(moveSlot);
                        break;
                }
            }
        }

        RefreshInventory();
    }

    private void HotbarAction(Slot slot)
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Drop(slot);
            hotbar.Remove(slot);
            PlayFX(drop);
        }
        else
        {
            if (t[0].gameObject.activeSelf)
            {
                if (quantumContainer.inventory.Count >= quantumContainer.maxSlots)
                    return;
                quantumContainer.Gather(slot);
                hotbar.Remove(slot);
                PlayFX(moveSlot);
            }
            else
            {
                if (inventory.Count >= maxSlots)
                    return;
                Gather(slot);
                hotbar.Remove(slot);
                PlayFX(moveSlot);
            }
        }

        RefreshInventory();
    }

    private void ContainerAction(Slot slot)
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Drop(slot);
            quantumContainer.inventory.Remove(slot);
            PlayFX(drop);
            RefreshInventory();
        }
        else if (!Input.GetKey(KeyCode.LeftControl) && inventory.Count < maxSlots)
        {
            Gather(slot);
            quantumContainer.inventory.Remove(slot);
            PlayFX(moveSlot);
            RefreshInventory();
        }
    }

    private void SlotAction(Slot slot)
    {
        int i = int.Parse(slot.metaData);
        ChangeMaxSlots(-i);

        if (Input.GetKey(KeyCode.LeftControl))
        {
            Drop(slot);
            slots.Remove(slot);
            PlayFX(drop);
            RefreshInventory();
        }
        else if (!Input.GetKey(KeyCode.LeftControl) && inventory.Count < maxSlots)
        {
            Gather(slot);
            slots.Remove(slot);
            PlayFX(moveSlot);
            RefreshInventory();
        }
    }

    private void ChangeMaxSlots(int quantity)
    {
        maxSlots += quantity;

        if (maxSlots > 40)
            maxSlots = 40;
        else if (maxSlots < 0)
            maxSlots = 0;
    }

    private void Drop(Slot slot)
    {
        GameObject x = Resources.Load<GameObject>("Core/QIS/" + slot.item);
        if (x == null)
        { x = Instantiate(go[3]); x.GetComponent<Renderer>().material = Resources.Load<Material>("Core/QIS/Materials/_" + slot.type.ToUpper()); }
        else
        { Instantiate(x); }

        x.transform.position = transform.position;

        x.GetComponent<QuantumItem>().item = slot.item;
        x.GetComponent<QuantumItem>().type = slot.type;
        x.GetComponent<QuantumItem>().quantity = slot.quantity;
        if (slot.icon == null)
            x.GetComponent<QuantumItem>().icon = error;
        else
            x.GetComponent<QuantumItem>().icon = slot.icon;
        x.GetComponent<QuantumItem>().stackable = slot.stackable;
        x.GetComponent<QuantumItem>().metaData = slot.metaData;
    }

    private void SetActive(bool container, bool options, bool viewer)
    {
        t[0].gameObject.SetActive(container);
        t[4].gameObject.SetActive(options);
        t[5].gameObject.SetActive(viewer);
    }

    public void Gather(QuantumItem item)
    {
        if (inventory.Count >= maxSlots)
            return;
        Slot slot = FindSlot(item.item);
        if (slot == null || !slot.stackable)
            inventory.Add(new Slot(item));
        else if (slot != null && slot.stackable && slot.type == item.type)
            slot.quantity += item.quantity;
        PlayFX(pickUp);
        Destroy(item.gameObject);
    }

    public void Gather(Slot item)
    {
        if (inventory.Count >= maxSlots)
            return;
        Slot slot = FindSlot(item.item);
        if (slot == null || !slot.stackable)
            inventory.Add(item);
        else if (slot != null && slot.stackable && slot.type == item.type)
            slot.quantity += item.quantity;
        PlayFX(pickUp);
    }

    public void GatherHotbar(Slot item)
    {
        if (hotbar.Count >= 9)
            return;
        Slot slot = FindHotbarSlot(item.item);
        if (slot == null || !slot.stackable)
            hotbar.Add(item);
        else if (slot != null && slot.stackable && slot.type == item.type)
            slot.quantity += item.quantity;
        PlayFX(pickUp);
    }

    public bool FindItem(string item)
    {
        foreach (Slot slot in inventory)
            if (slot.item == item)
                return true;
        return false;
    }

    public bool FindItem(string item, int quantity)
    {
        foreach (Slot slot in inventory)
            if (slot.item == item && slot.quantity >= quantity)
                return true;
        return false;
    }

    public Slot FindSlot(string item)
    {
        foreach (Slot slot in inventory)
            if (slot.item == item)
                return slot;
        return null;
    }

    public Slot FindHotbarSlot(string item)
    {
        foreach (Slot slot in hotbar)
            if (slot.item == item)
                return slot;
        return null;
    }

    public bool FindMetaData(string type, string metaData)
    {
        foreach (Slot slot in inventory)
            if (slot.type == type && slot.metaData == metaData)
                return true;

        foreach (Slot slot in hotbar)
            if (slot.type == type && slot.metaData == metaData)
                return true;
        return false;
    }

    public void FindItemAndRemove(string item)
    {
        foreach (Slot slot in inventory)
            if (slot.item == item)
                inventory.Remove(slot);
    }

    public void FindItemAndRemove(string item, int quantity)
    {
        foreach (Slot slot in inventory)
            if (slot.item == item && slot.quantity >= quantity)
            {
                slot.quantity -= quantity;
                if (slot.quantity <= 0)
                    inventory.Remove(slot);
            }
    }

    public Slot GetHotbarID (int i)
    {
        return hotbar[i];
    }

    private void PlayFX(AudioClip fx)
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

    [System.Serializable]
    public class Slot
    {
        public string item, type;
        public int quantity;
        public Sprite icon;
        public bool stackable;
        [TextArea(3, 5)]
        public string metaData;

        public Slot(string item, string type, int quantity, Sprite icon, bool stackable, string metaData)
        {
            this.item = item;
            this.type = type;
            this.quantity = quantity;
            this.icon = icon;
            this.stackable = stackable;
            this.metaData = metaData;
        }

        public Slot(QuantumItem quantum)
        {
            this.item = quantum.item;
            this.type = quantum.type;
            this.quantity = quantum.quantity;
            this.icon = quantum.icon;
            this.stackable = quantum.stackable;
            this.metaData = quantum.metaData;
        }
    }
}
