﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleTime : MonoBehaviour
{
    //when you be in a colider
    void OnTriggerStay(Collider other)
    {

        Application.LoadLevel("Temple1");

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
