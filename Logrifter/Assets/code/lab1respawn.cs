using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lab1respawn : MonoBehaviour
{
    
    void OnTriggerStay(Collider other)
    {

        if (other.gameObject.tag == "Respawn")
        {
            Application.LoadLevel("lab1");
        }

    }
}
