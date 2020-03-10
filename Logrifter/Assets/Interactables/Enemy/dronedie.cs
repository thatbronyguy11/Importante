using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dronedie : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "drone")
        {
            // destroy this object
           // Destroy(collider.gameObject,.2f);
        }
    }
}
