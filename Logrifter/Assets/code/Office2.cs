using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Office2 : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            Application.LoadLevel("Office2");
        }
    }
}
