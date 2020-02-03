using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temple1to2 : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Application.LoadLevel("Temple2");
        }
    }
}
