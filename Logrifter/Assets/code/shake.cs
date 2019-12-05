using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shake : MonoBehaviour
{

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {

    }

    void OnTriggerStay(Collider other)
    {
        animator.enabled = enabled;
    }

    void OnTriggerLeave(Collider other)
    {
        animator.enabled = !enabled;
    }
}