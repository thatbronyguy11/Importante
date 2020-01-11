using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class level3 : MonoBehaviour
{
    //when you be in a colider
    void OnTriggerStay(Collider Finish)
    {

        Application.LoadLevel("Temple3");

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
