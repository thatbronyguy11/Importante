using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public GameObject Laser = null;
    public bool fire = false;
    public int time = 0;
    void OnTriggerEnter(Collider other)
    
    {
        if(other.gameObject.tag == "other")
        {
            time = 50;
        }
        
    }
    void OnTriggerExit(Collider other)
    {
        fire = false;
    }
    void Update()
    {
        if(time > 1)
        {
            Laser.SetActive(true);
        }
        else
        {
            Laser.SetActive(false);
        }
        time--;
        if (time < 0)
        {
            time = 0;
        }
    }
    public bool getkilledbigblob = false;
//    void update()
//    {
//        if (Physics.Raycast(ray, out hit))
//        {
//            if (hit.collider.gameObject.tag == "BigBlob")
//            {
//                getkilledbigblob = true;
//            }
//            else
//            {
//                getkilledbigblob = false;
//            }
//        }
//    }
}
