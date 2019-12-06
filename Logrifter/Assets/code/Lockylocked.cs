using UnityEngine;

public class Lockylocked : MonoBehaviour
{
    void Update()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

}