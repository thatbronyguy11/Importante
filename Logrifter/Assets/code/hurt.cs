using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class hurt : MonoBehaviour
{
    public bool strike1 = false;
    public bool strike2 = false;
    public bool strike3 = false;
    int timer = 0;
    void OnTriggerEnter(Collider other)
    {
        if (timer < 1)
        {

            if (other.gameObject.tag == "Player")
            {
                if (strike1 = true)
                {
                    if (strike2 = true)
                    {
                        if (strike3 = true)
                        {
                            Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name);
                        }
                        else
                        {
                            strike3 = true;
                            timer = 60;
                        }
                    }
                    else
                    {
                        strike2 = true;
                        timer = 60;
                    }
                }
                else
                {
                    strike1 = true;
                    timer = 60;
                }
                timer = 60;
            }
        }
        else
        {
            timer = timer - 1;
        }
        if (timer < 0)
        {
            timer = 0;
        }

    }
}
