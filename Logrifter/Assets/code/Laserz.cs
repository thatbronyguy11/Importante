using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class Laserz : MonoBehaviour
{
    public float range = 1000;
    private LineRenderer line;
    public bool playerOnly = true;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.SetVertexCount(2);
    }

    void Update() // consider void FixedUpdate()
    {
        //RaycastHit hit = Physics.Raycast(transform.position, transform.up, transform.forward, range); // transform.position + (transform.right * (float)offset) can be used for casting not from center.
       // if (hit)
        //{
            //line.SetPosition(0, transform.position);
            //line.SetPosition(1, hit.point);
            //Collider collider = hit.collider;
            //if (collider.gameObject.tag == "Player")
            //{
                // Register hit.
          //  }
       // }
      //  else
      //  {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position + (transform.right * range)); // (transform.right * ((float)offset + range)) can be used for casting not from center.
        //}
    }
}