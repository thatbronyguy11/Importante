using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotation : MonoBehaviour
{

    // Start is called before the first frame update

    void Start()

    {



    }

    public int SpeedY = 0;
    public int SpeedX = 0;
    public int SpeedZ = 0;

    // Update is called once per frame

    void Update()

    {

        transform.Rotate(0, SpeedY * Time.deltaTime, 0); //rotates "Speed" degrees per second around z axis
        transform.Rotate(SpeedX * Time.deltaTime, 0, 0); //rotates "Speed" degrees per second around z axis
        transform.Rotate(0, 0, SpeedZ * Time.deltaTime); //rotates "Speed" degrees per second around z axis

    }

}
