using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temple1respawn : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Application.LoadLevel("Temple1");
        }
    }
}
