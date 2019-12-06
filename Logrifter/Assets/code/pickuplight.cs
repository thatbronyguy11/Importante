using UnityEngine;
using System.Collections;

public class pickuplight : MonoBehaviour
{


    public float speed = 10;
    public bool canHold = true;
    public GameObject light;
    public Transform guide;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!canHold)
                throw_drop();
            else
                Pickup();
        }//mause If

        if (!canHold && light)
            light.transform.position = guide.position;

    }//update

    //We can use trigger or Collision
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "light")
            if (!light) // if we don't have anything holding
                light = col.gameObject;
    }

    //We can use trigger or Collision
    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "light")
        {
            if (canHold)
                light = null;
        }
    }


    private void Pickup()
    {
        if (!light)
            return;

        //turn off gravity and stuff
        //light.GetComponent<Rigidbody>().isKinematic = true;

        //We set the object parent to our guide empty object.
        light.transform.SetParent(guide);

        //Set gravity to false while holding it
        light.GetComponent<Rigidbody>().useGravity = false;

        //we apply the same rotation our main object (Camera) has.
        light.transform.localRotation = transform.rotation;
        //We re-position the light on our guide object 
        light.transform.position = guide.position;

        canHold = false;
    }

    private void throw_drop()
    {
        if (!light)
            return;

        //turn back on gravity and stuff
        //light.GetComponent<Rigidbody>().isKinematic = false;

        //Set our Gravity to true again.
        light.GetComponent<Rigidbody>().useGravity = true;
        // we don't have anything to do with our light field anymore
        light = null;
        //Apply velocity on throwing
        guide.GetChild(0).gameObject.GetComponent<Rigidbody>().velocity = transform.forward * speed;

        //Unparent our light
        guide.GetChild(0).parent = null;
        canHold = true;
    }
}//class