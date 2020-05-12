using UnityEngine;
using System.Collections;

public class buttontest : MonoBehaviour
{


    public GameObject light1 = null;
    public GameObject invert = null;

    public GameObject hinge = null;
    public GameObject Gravity = null;
    public GameObject uhh = null;


        void Start()
    {
        HingeJoint joint = hinge.GetComponent<HingeJoint>();

        // Make the spring reach shoot for a 70 degree angle.
        // This could be used to fire off a catapult.

        joint.useSpring = true;
    }
    void OnTriggerEnter(Collider other)
    {

        if (other.name == "Button")
        {


            HingeJoint joint = hinge.GetComponent<HingeJoint>();
            light1.SetActive(true);
            invert.SetActive(false);
            joint.useSpring = true;
            Gravity.SetActive(true);
            uhh.SetActive(true);
        }

    }

    void OnTriggerExit(Collider other)
    {

        if (other.name == "Button")
        {

            HingeJoint joint = hinge.GetComponent<HingeJoint>();
            light1.SetActive(false);
            invert.SetActive(true);
            joint.useSpring = false;
            joint.constraints = RigidbodyConstraints.FreezeRotationX;
            Gravity.SetActive(false);
            uhh.SetActive(false);
        }

    }




}