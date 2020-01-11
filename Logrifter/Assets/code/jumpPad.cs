using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jumpPad : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<Rigidbody>().AddForce(Vector3.up * 300);
    }
}