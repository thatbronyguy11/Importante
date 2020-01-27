using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fallingplatform : MonoBehaviour
{
    public GameObject Spring = null;
    // Start is called before the first frame update
    void Start()
    {
        HingeJoint hinge = GetComponent<HingeJoint>();

        // Make the spring reach shoot for a 70 degree angle.
        // This could be used to fire off a catapult.

        hinge.useSpring = true;
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.name == "Player")
        {
            Spring.GetComponent<HingeJoint>().useSpring = false;
        }

    }
}
