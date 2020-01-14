using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class die2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public GameObject light1 = null;
    // Update is called once per frame
    void Update()
    {
        if(light1.activeSelf == false)
        {
            gameObject.SetActive(false);
        }
    }
}
