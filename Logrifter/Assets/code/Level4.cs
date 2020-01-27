using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level4 : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {

        if (other.gameObject.tag == "L4")
        {
            Application.LoadLevel("Temple4");
        }

    }
}
