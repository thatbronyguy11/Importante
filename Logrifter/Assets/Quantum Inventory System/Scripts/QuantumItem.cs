using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuantumItem : MonoBehaviour
{
    public string item, type;
    public int quantity;
    public Sprite icon;
    public bool stackable;
    [TextArea(3,5)]
    public string metaData;
}
