using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class escape2 : MonoBehaviour
{
    //when you be in a colider
    void OnTriggerStay(Collider other)
    {

        Application.LoadLevel("Crash2");

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
