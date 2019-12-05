using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerL1 : MonoBehaviour
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
            bool enabled = !light.enabled;
            light.enabled = enabled;   
            Light[] lights = light.GetComponentsInChildren<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].enabled = enabled;
            }
        }
        
    }
}
