using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollTex : MonoBehaviour
{
    public float ScrollX = 0.5F;
    public float ScrollY = 0.5F;

    // Update is called once per frame
    void Update()
    {
        float OffsetX = Time.time * ScrollX;
        float OffsetY = Time.time * ScrollY;
        GetComponent<Renderer>().material.mainTextureOffset = new Vector2(OffsetX, OffsetY);
    }
}
