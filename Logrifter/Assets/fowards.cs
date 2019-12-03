using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fowards : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 xVelocity = rb.velocity;
        if(xVelocity.x < 0)
        {
            xVelocity.x = (xVelocity.x * -1);
        }
        if(xVelocity.x <= 20)
        {
            xVelocity.x = (xVelocity.x + 2);
        }
    }
}
