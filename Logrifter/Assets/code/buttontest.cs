using UnityEngine;
using System.Collections;

public class buttontest : MonoBehaviour
{


    public GameObject light1 = null;
    public GameObject invert = null;

    public GameObject Spring = null;
    public GameObject Gravity = null;
    public GameObject uhh = null;


        void Start()
    {
        HingeJoint hinge = GetComponent<HingeJoint>();

        // Make the spring reach shoot for a 70 degree angle.
        // This could be used to fire off a catapult.

        hinge.useSpring = true;
    }
    void OnTriggerEnter(Collider other)
    {

        if (other.name == "Button")
        {


            
            light1.SetActive(true);
            invert.SetActive(false);
            Spring.GetComponent<HingeJoint>().useSpring = true;
            Gravity.SetActive(true);
            uhh.SetActive(true);
        }

    }

    void OnTriggerExit(Collider other)
    {

        if (other.name == "Button")
        {

            
            light1.SetActive(false);
            invert.SetActive(true);
            Spring.GetComponent<HingeJoint>().useSpring = false;
            Gravity.SetActive(false);
            uhh.SetActive(false);
        }

    }




}