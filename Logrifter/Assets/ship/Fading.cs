using UnityEngine;
using System.Collections;

public class Fading : MonoBehaviour
{

    private void Update()
    {
        var material = GetComponent<Renderer>().material;
        var color = material.color;
        if (color.a > 0)
        {
            material.color = new Color(color.r, color.g, color.b, color.a + (1f * Time.deltaTime));
        }
    }
}

