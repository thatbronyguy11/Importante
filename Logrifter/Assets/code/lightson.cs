using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightson : MonoBehaviour
{
    public GameObject light = null;
    void OnTriggerEnter(Collider other)
    {

        if (other.name == "Player")
        {
            light.SetActive(true);
        }
    }
}
