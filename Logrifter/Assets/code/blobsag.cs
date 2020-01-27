using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blobsag : MonoBehaviour
{
    // Start is called before the first frame update
    public float x = 1.0f;
    public float y = 1.0f;
    public float z = 1.0f;
    public GameObject light1 = null;

    void Start()
    {
        transform.localScale = new Vector3(x, y, z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
