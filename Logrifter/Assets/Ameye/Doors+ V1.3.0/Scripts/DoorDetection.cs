// DoorDetection.cs
// Created by Alexander Ameye
// Version 1.2.0

using UnityEngine;

public class DoorDetection : MonoBehaviour
{
    public float Reach;
    public bool LookingAt;
    
    public RaycastHit hitPublic;
    public Camera cam;

    #region UI Settings
    public GameObject LookingAtPrefab, LookingAtPrefabInstance, LookingAtObject;
    public bool LookingAtTextActive;
    public GameObject InTriggerZoneLookingAtPrefab, InTriggerZoneLookingAtPrefabInstance;
    public bool InTriggerZoneTextActive;
    public bool TextWhenLookingAt, InTriggerZone;
    #endregion

    #region Debugging Settings
    public bool DebugRay;
    public Color DebugRayColor = Color.green;
    public float DebugRayColorAlpha = 1.0F;
    #endregion

    private bool inzone;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Trigger")) inzone = true;
        else inzone = false;
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name.Contains("Trigger")) inzone = true;
        else inzone = false;
    }

    public void OnTriggerExit(Collider other)
    {
        inzone = false;
    }

    private void Start()
    {
        gameObject.name = "Player";
        gameObject.tag = "Player";

        var oldmask = transform.GetComponentInChildren<Camera>().cullingMask;
        transform.GetComponentInChildren<Camera>().cullingMask = oldmask &
            ~(1 << LayerMask.NameToLayer("Trigger Zones"));
    }

    public void Update()
    {
        if (inzone)
        {
            if (InTriggerZoneTextActive || InTriggerZoneLookingAtPrefab == null) return;
            InTriggerZoneLookingAtPrefabInstance = Instantiate(InTriggerZoneLookingAtPrefab);
        
            InTriggerZoneTextActive = true;
            InTriggerZoneLookingAtPrefabInstance.transform.SetParent(transform, true);
        }

        else
        {
            if (InTriggerZoneTextActive == true)
            {
                DestroyImmediate(InTriggerZoneLookingAtPrefabInstance);
                InTriggerZoneTextActive = false;
            }

            if (LookingAtTextActive != true) return;
            Destroy(LookingAtPrefabInstance);
            LookingAtTextActive = false;
        }
    }

    public void CheckUIPrefabs(GameObject obj)
    {
        //Set origin of ray to 'center of screen' and direction of ray to 'cameraview'.
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0F));

        RaycastHit hit; //Variable reading information about the collider hit.

        //Cast ray from center of the screen towards where the player is looking.
        if (Physics.Raycast(ray, out hit, Reach))
        {
            hitPublic = hit;

            if (hit.collider.gameObject.name == obj.name)
            {
                //Display the UI element when the player is in reach of the door.
                if (LookingAtTextActive == false && LookingAtPrefab != null)
                {
                    LookingAtPrefabInstance = Instantiate(LookingAtPrefab);
                    LookingAtTextActive = true;
                    LookingAtPrefabInstance.transform.SetParent(transform, true);
                }
            }

            else
            {
                //Destroy the UI element when Player is no longer in reach of the door.
                if (LookingAtTextActive)
                {
                    Destroy(LookingAtPrefabInstance);
                    LookingAtTextActive = false;
                }
            }
        }

        else
        {
            //Destroy the UI element when Player is no longer in reach of the door.
            if (LookingAtTextActive)
            {
                Destroy(LookingAtPrefabInstance);
                LookingAtTextActive = false;
            }
        }

        if (DebugRay) Debug.DrawRay(ray.origin, ray.direction * Reach, DebugRayColor);
    }


    public bool CheckIfLookingAt(GameObject obj)
    {
        //Set origin of ray to 'center of screen' and direction of ray to 'cameraview'.
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0F));

        RaycastHit hit; //Variable reading information about the collider hit.

        //Cast ray from center of the screen towards where the player is looking.
        if (Physics.Raycast(ray, out hit, Reach))
        {
            hitPublic = hit;

            if (hit.collider.gameObject.name == obj.name)
            {
                LookingAt = true;

                //Display the UI element when the player is in reach of the door.
                if (LookingAtTextActive == false && LookingAtPrefab != null)
                {
                    LookingAtPrefabInstance = Instantiate(LookingAtPrefab);
                    LookingAtTextActive = true;
                    LookingAtPrefabInstance.transform.SetParent(transform, true); //Make the player the parent object of the text element.
                }
            }

            else
            {
                LookingAt = false;

                if (LookingAtTextActive)
                {
                    DestroyImmediate(LookingAtPrefabInstance);
                    LookingAtTextActive = false;
                }
            }
        }

        else
        {
            LookingAt = false;

            if (LookingAtTextActive)
            {
                DestroyImmediate(LookingAtPrefabInstance);
                LookingAtTextActive = false;
            }
        }

        if (DebugRay) Debug.DrawRay(ray.origin, ray.direction * Reach, DebugRayColor);
        return LookingAt;
    }
}
