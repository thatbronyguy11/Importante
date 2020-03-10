using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drone : MonoBehaviour
{
    public GameObject boom = null;
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "other")
        {
            boom.SetActive(true);
            GetComponent<Rigidbody>().useGravity = true;
            Destroy(gameObject, .5f);
        }
    }

}
