using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    float speed = 7.0f;
    float rotationSpeed = 100.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float translation = (Input.GetAxis("Vertical") * speed) * Time.deltaTime;
        float rotation = (Input.GetAxis("Horizontal") * rotationSpeed) * Time.deltaTime;
        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);

    }

    public void ReceiveDamage(float damage)
    {
        Debug.Log("PLAYER_CONTROLLER: Player hit with " + damage + " damage!");
    }
}
