using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        other.GetComponent<Animation>().Play("open");
    }
    void OnTriggerLeave(Collider other)
    {
        other.GetComponent<Animation>().Play("close");
    }
}
