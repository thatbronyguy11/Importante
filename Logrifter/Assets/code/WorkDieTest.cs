using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WorkDieTest : MonoBehaviour
{
    // Start is called before the first frame update
   
        public float x = 1.0f;
        public float y = 1.0f;
        public float z = 1.0f;
        public GameObject light1 = null;

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount > 1)
        {
            // Widen the object by x, y, and z values
            if (x >= .2f)
            {
                x = x * 0.999f;
                y = y * 0.999f;
                z = z * 0.999f;
                transform.localScale = new Vector3(x, y, z);
            }
            else
            {
                if (x >= .05f)
                {
                    x = x * 0.99f;
                    y = y * 0.99f;
                    z = z * 0.99f;
                    transform.localScale = new Vector3(x, y, z);
                }
                else
                {
                    light1.transform.parent = null;
                    light1.transform.Rotate(0.0f, 0.0f, 0.0f);
                    light1.SetActive(true);
                    gameObject.SetActive(false);
                }
            }
        }
    }
    
        

}
