using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAIController : MonoBehaviour
{
    [Tooltip("The target to follow")]
    public GameObject target;
    public bool followTarget = true;
    [Tooltip("The speed the object will move at")]
    public float speed = 10f;
    public float stoppingDistance = 0;
    public bool destroyOnCollision = false;
    [Tooltip("How much time (in seconds) must elapse before the object destroys itself, after colliding")]
    public float delay = 0f;
    public bool orbit = false;
    float t = 0;


    // Update is called once per frame
    void Update()
    {
        if(followTarget)
        {
            Seek();
        }

        if(orbit)
        {
            Orbit();
        }
    }
    private void Seek()
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if(distance > stoppingDistance)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 12 * Time.deltaTime);
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        
    }
    private void Orbit()
    {
        float newX = Mathf.Cos(t);
        float newZ = Mathf.Sin(t);

        transform.position = new Vector3(newX, transform.position.y, newZ);
        t += 0.03f;
    }
    void OnTriggerEnter(Collider collider)
    {
        if (destroyOnCollision)
        {
            Destroy(gameObject);
        }
    }
}
