using UnityEngine;
using System.Collections;

public class strobe: MonoBehaviour
{

    public Color altColor = Color.black;
    public Renderer rend;

    //I do not know why you need this?
    void Example()
    {
        altColor.g = 0f;
        altColor.r = 0f;
        altColor.b = 0f;
        altColor.a = 255f;
    }

    void Start()
    {
        //Call Example to set all color values to zero.
        Example();
        //Get the renderer of the object so we can access the color
        rend = GetComponent<Renderer>();
        //Set the initial color (0f,0f,0f,0f)
        rend.material.color = altColor;
    }

    void Update()
    {
        if (altColor.a > 0)
        {
            //Alter the color          
            altColor.a += 1f;
            //altColor.g += 1f;
            //altColor.r += 1f;
            //altColor.b += 1f;
            //Assign the changed color to the material. 
            rend.material.color = altColor;
        }
    }
}

