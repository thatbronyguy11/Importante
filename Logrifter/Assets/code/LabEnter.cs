using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabEnter : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {

        Application.LoadLevel("Lab1");

    }
}
