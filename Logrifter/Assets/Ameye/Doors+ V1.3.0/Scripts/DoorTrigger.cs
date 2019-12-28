using RuntimeScriptField;
using UnityEngine;

namespace DoorsPlus
{
    [RequireComponent(typeof(BoxCollider), typeof(SphereCollider))]
    public class DoorTrigger : MonoBehaviour, IHierarchyIcon
    {
        public string EditorIconPath => transform.gameObject.name;
        public int id;

        public enum TypeOfCollider { Cubic, Spherical }
        public TypeOfCollider colliderType;

        public bool correctTag, correctName, correctView, correctButton, correctScript, correctGameObject;
        public bool hasTag, hasName, isLookingAt, hasPressed, hasScript, isGameObject;
        [HideInInspector]
        [TagSelector]
        public string playerTag = "Untagged";
        public string playerName, character;
        public GameObject lookObject, isObject;
        public ComponentReference script;

        [HideInInspector]
        public bool allGood = true;
        [HideInInspector]
        public bool showTriggerZones;

        // Scripts
        private DefaultDoor defaultDoor;
        private SwingDoor swingDoor;
        private SlidingDoor slidingDoor;
        private DoorDetection doorDetection;
        private string doorType = "";

        private GameObject enteredObject;

        private void Start()
        {
            transform.gameObject.layer = LayerMask.NameToLayer("Trigger Zones");
        }

        private void OnTriggerEnter(Collider other)
        {

            enteredObject = other.gameObject;
            
            if (transform.parent.GetComponentInChildren<DefaultDoor>() != null)
            {
                defaultDoor = transform.parent.GetComponentInChildren<DefaultDoor>();
                doorType = "default";
            }

            else if (transform.parent.GetComponentInChildren<SwingDoor>() != null)
            {
                swingDoor = transform.parent.GetComponentInChildren<SwingDoor>();
                doorType = "swing";
            }

            else if (transform.parent.GetComponentInChildren<SlidingDoor>() != null)
            {
                slidingDoor = transform.parent.GetComponentInChildren<SlidingDoor>();
                doorType = "sliding";
            }
            doorDetection = GameObject.FindGameObjectWithTag("Player").GetComponent<DoorDetection>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (lookObject) doorDetection.CheckUIPrefabs(lookObject);

            if (defaultDoor != null && (defaultDoor.MovementPending || defaultDoor.CurrentRotationBlockIndex >= defaultDoor.RotationTimeline.Count || defaultDoor.CurrentRotationBlockIndex != id)) return;
            if (swingDoor != null && (swingDoor.MovementPending || swingDoor.CurrentRotationBlockIndex >= swingDoor.RotationTimeline.Count || swingDoor.CurrentRotationBlockIndex != id)) return;
            if (slidingDoor != null && (slidingDoor.MovementPending || slidingDoor.CurrentSlidingBlockIndex >= slidingDoor.SlidingTimeline.Count || slidingDoor.CurrentSlidingBlockIndex != id)) return;

            correctTag = (!hasTag || other.CompareTag(playerTag));
            correctName = (!hasName || other.name == playerName);
            correctView = (!isLookingAt || doorDetection.CheckIfLookingAt(lookObject));

            if (character.Length == 1)
            {
                correctButton = (!hasPressed || Input.GetKey(character));
            }
            else if (character.Length > 1)
            {
                correctButton = (!hasPressed || Input.GetButton(character));
            }
            else
            {
                correctButton = (!hasPressed);
            }

            correctScript = (!hasScript || enteredObject.GetComponent(script.script.Name) != null);
            correctGameObject = (!isGameObject || enteredObject == isObject);

            allGood = correctTag && correctName && correctView && correctScript && correctGameObject;

            if (correctTag && correctName && correctView && correctScript && correctButton && correctGameObject)
            {
                if (transform.gameObject.name.Contains("Move Trigger (Rotation"))
                {
                    if (defaultDoor != null) PlaySoundStart();
                    if (defaultDoor != null) defaultDoor.StartCoroutine(defaultDoor.Rotate());
                    if (defaultDoor != null) PlaySoundEnd();
                }
                else if (transform.gameObject.name.Contains("Close Trigger (Rotation"))
                {
                    if (defaultDoor.DoorIsClosing())
                    {
                        if (defaultDoor != null) PlaySoundStart();
                        if (defaultDoor != null) defaultDoor.StartCoroutine(defaultDoor.Rotate());
                        if (defaultDoor != null) PlaySoundEnd();
                    }
                }
                else if (transform.gameObject.name.Contains("Open Trigger (Rotation"))
                {
                    if (defaultDoor.DoorIsOpening())
                    {
                        if (defaultDoor != null) PlaySoundStart();
                        if (defaultDoor != null) defaultDoor.StartCoroutine(defaultDoor.Rotate());
                        if (defaultDoor != null) PlaySoundEnd();
                    }
                }
                else if (transform.gameObject.name.Contains("Front Trigger (Rotation"))
                {
                    if (swingDoor != null) PlaySoundStart();
                    if (swingDoor != null) swingDoor.StartCoroutine(swingDoor.Swing(true));
                    if (swingDoor != null) PlaySoundEnd();
                }
                else if (transform.gameObject.name.Contains("Back Trigger (Rotation"))
                {
                    if (swingDoor != null) PlaySoundStart();
                    if (swingDoor != null) swingDoor.StartCoroutine(swingDoor.Swing(false));
                    if (swingDoor != null) PlaySoundEnd();
                }
                else if (transform.gameObject.name.Contains("Move Trigger (Slide"))
                {
                    if (slidingDoor != null) PlaySoundStart();
                    if (slidingDoor != null) slidingDoor.StartCoroutine(slidingDoor.Slide());
                    if (slidingDoor != null) PlaySoundEnd();
                }
                else if (transform.gameObject.name.Contains("Close Trigger (Slide"))
                {
                    if (slidingDoor.DoorIsClosing())
                    {
                        if (slidingDoor != null) PlaySoundStart();
                        if (slidingDoor != null) slidingDoor.StartCoroutine(slidingDoor.Slide());
                        if (slidingDoor != null) PlaySoundEnd();
                    }
                }
                else if (transform.gameObject.name.Contains("Open Trigger (Slide"))
                {
                    if (slidingDoor.DoorIsOpening())
                    {
                        if (slidingDoor != null) PlaySoundStart();
                        if (slidingDoor != null) slidingDoor.StartCoroutine(slidingDoor.Slide());
                        if (slidingDoor != null) PlaySoundEnd();
                    }
                }
            }

            //TODO: add sound for swing doors and sliding doors
            if (transform.gameObject.name.Contains("Open Trigger") && defaultDoor != null)
            {
                if (!allGood && correctButton && defaultDoor.DoorIsOpening())
                {
                    defaultDoor.Play("locked");
                    allGood = false;
                }
            }

            if (transform.gameObject.name.Contains("Close Trigger") && defaultDoor != null)
            {
                if (!allGood && correctButton && defaultDoor.DoorIsClosing())
                {
                    defaultDoor.Play("locked");
                    allGood = false;
                }
            }

            if (transform.gameObject.name.Contains("Move Trigger") && defaultDoor != null)
            {
                if (!allGood && correctButton)
                {
                    defaultDoor.Play("locked");
                    allGood = false;
                }
            }
        }

        //TODO: add functionality for swing doors
        private void OnTriggerExit(Collider other)
        {
           
            
            switch (doorType)
            {
                case "default":
                    Transform doorTransform = defaultDoor.transform;
                    
                    if (defaultDoor == null || !defaultDoor.ResetOnLeave) return;

                    bool doorHasRotated = doorTransform.rotation == defaultDoor.EndRotation * defaultDoor.RotationOffset;
                    bool hingeWasMoved = doorTransform.parent.transform.rotation == defaultDoor.EndRotation;

                    if (defaultDoor.PivotPosition == DefaultDoor.PositionOfPivot.CorrectlyPositioned && doorHasRotated)
                    {
                        StartCoroutine(RotationTools.Rotate(defaultDoor.gameObject, -defaultDoor.InitialYRotation - defaultDoor.RotationTimeline[0].FinalAngle, -defaultDoor.RotationTimeline[0].InitialAngle - defaultDoor.InitialYRotation, 2, 0, defaultDoor.ShortestWay));
                    }

                    if (defaultDoor.PivotPosition == DefaultDoor.PositionOfPivot.Centered && hingeWasMoved)
                    {
                        StartCoroutine(RotationTools.Rotate(defaultDoor.transform.parent.gameObject, -defaultDoor.RotationTimeline[0].FinalAngle, -defaultDoor.RotationTimeline[0].InitialAngle, 2, 0, defaultDoor.ShortestWay));
                    }

                    defaultDoor.CurrentRotationBlockIndex = 0;
                    defaultDoor.TimesMoved = 0;
                    break;

                case "swing":
    
                    
                    break;

                case "sliding":
                  
                    
                    if (slidingDoor == null || !slidingDoor.ResetOnLeave) return;
                    bool doorHasSlided = false;

                    if (slidingDoor.SlidingTimeline[0].Axis == SlidingDoor.SlidingTimelineData.SlidingAxis.X)
                    {
                        Vector3 endPosition = new Vector3(slidingDoor.SlidingTimeline[0].Distance, 0, 0);
                        if (slidingDoor.transform.position == slidingDoor.InitialPosition + endPosition) doorHasSlided = true;
                        if (doorHasSlided) StartCoroutine(RotationTools.Slide(slidingDoor.gameObject, slidingDoor.InitialPosition + endPosition, slidingDoor.InitialPosition, slidingDoor.SlidingTimeline[0].Speed));
                    }

                    else if (slidingDoor.SlidingTimeline[0].Axis == SlidingDoor.SlidingTimelineData.SlidingAxis.Y)
                    {
                        Vector3 endPosition = new Vector3(0, slidingDoor.SlidingTimeline[0].Distance, 0);
                        if (slidingDoor.transform.position == slidingDoor.InitialPosition + endPosition) doorHasSlided = true;
                        if (doorHasSlided) StartCoroutine(RotationTools.Slide(slidingDoor.gameObject, slidingDoor.InitialPosition + endPosition, slidingDoor.InitialPosition, slidingDoor.SlidingTimeline[0].Speed));
                    }

                    else if (slidingDoor.SlidingTimeline[0].Axis == SlidingDoor.SlidingTimelineData.SlidingAxis.Z)
                    {
                        Vector3 endPosition = new Vector3(0, 0, slidingDoor.SlidingTimeline[0].Distance);
                        if (slidingDoor.transform.position == slidingDoor.InitialPosition + endPosition) doorHasSlided = true;
                        if (doorHasSlided) StartCoroutine(RotationTools.Slide(slidingDoor.gameObject, slidingDoor.InitialPosition + endPosition, slidingDoor.InitialPosition, slidingDoor.SlidingTimeline[0].Speed));
                    }

                    slidingDoor.CurrentSlidingBlockIndex = 0;
                    slidingDoor.TimesMoved = 0;
                    break;
            }
        }

        private void PlaySoundStart()
        {
            if (defaultDoor == null || defaultDoor.transform.GetComponent<DefaultDoor>() == null) return;

            string triggerName = transform.gameObject.name;

            if (triggerName.Contains("Move Trigger") || triggerName.Contains("Front Trigger")) defaultDoor.transform.GetComponent<DefaultDoor>().Play("opening");
            else if (triggerName.Contains("Back Trigger")) defaultDoor.transform.GetComponent<DefaultDoor>().Play("closing");
            else if (defaultDoor.DoorIsOpening()) defaultDoor.transform.GetComponent<DefaultDoor>().Play("opening");
            else defaultDoor.transform.GetComponent<DefaultDoor>().Play("closing");
        }

        private void PlaySoundEnd()
        {
            if (defaultDoor == null || defaultDoor.transform.GetComponent<DefaultDoor>() == null) return;

            string triggerName = transform.gameObject.name;

            if (triggerName.Contains("Move Trigger") || triggerName.Contains("Front Trigger")) defaultDoor.transform.GetComponent<DefaultDoor>().Play("opened");
            else if (triggerName.Contains("Back Trigger")) defaultDoor.transform.GetComponent<DefaultDoor>().Play("closed");
            else if (defaultDoor.DoorIsOpening()) defaultDoor.transform.GetComponent<DefaultDoor>().Play("opened");
            else defaultDoor.transform.GetComponent<DefaultDoor>().Play("closed");
        }
    }
}

