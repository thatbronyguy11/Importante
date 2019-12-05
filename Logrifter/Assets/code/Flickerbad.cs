using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flickerbad : MonoBehaviour
{
    Light light;
    public float L1;
    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        L1 = Random.Range(1.0f, 60.0f);
        if (L1 > 58.0)
        {
            
            light.enabled = enabled;   
            
        }
        else
        {
            light.enabled = !enabled;
        }
        
    }
}
