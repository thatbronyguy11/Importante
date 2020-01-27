using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class drawlaser : MonoBehaviour
{

    public int laserDistance;
    public LineRenderer mLineRenderer;
    public string bounceTag;
    public int maxBounce;
    private float timer = 0;

    // Use this for initialization
    void Start()
    {
        StartCoroutine("FireMahLazer");
    }

    // Update is called once per frame
    void Update()
    {

            timer = 0;
    }

    IEnumerator FireMahLazer()
    {
        //Debug.Log("Running");
        mLineRenderer.enabled = true;
        int laserReflected = 1; //How many times it got reflected
        int vertexCounter = 1; //How many line segments are there
        bool loopActive = true; //Is the reflecting loop active?

        Vector3 laserDirection = transform.forward; //direction of the next laser
        Vector3 lastLaserPosition = transform.localPosition; //origin of the next laser

        mLineRenderer.SetVertexCount(1);
        mLineRenderer.SetPosition(0, transform.position);
        RaycastHit hit;

        while (loopActive)
        {

            if (Physics.Raycast(lastLaserPosition, laserDirection, out hit, laserDistance) && hit.transform.gameObject.tag == bounceTag)
            {

                Debug.Log("Bounce");
                laserReflected++;
                vertexCounter += 3;
                mLineRenderer.SetVertexCount(vertexCounter);
                mLineRenderer.SetPosition(vertexCounter - 3, Vector3.MoveTowards(hit.point, lastLaserPosition, 0.01f));
                mLineRenderer.SetPosition(vertexCounter - 2, hit.point);
                mLineRenderer.SetPosition(vertexCounter - 1, hit.point);
                mLineRenderer.SetWidth(.1f, .1f);
                lastLaserPosition = hit.point;
                laserDirection = Vector3.Reflect(laserDirection, hit.normal);
            }
            else
            {

                Debug.Log("No Bounce");
                laserReflected++;
                vertexCounter++;
                mLineRenderer.SetVertexCount(vertexCounter);
                Vector3 lastPos = lastLaserPosition + (laserDirection.normalized * laserDistance);
                Debug.Log("InitialPos " + lastLaserPosition + " Last Pos" + lastPos);
                mLineRenderer.SetPosition(vertexCounter - 1, lastLaserPosition + (laserDirection.normalized * laserDistance));

                loopActive = false;
            }
            if (laserReflected > maxBounce)
                loopActive = false;
        }

        if (Input.GetKey("space") && timer < 2)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            StartCoroutine("FireMahLazer");
        }
        else
        {
            yield return null;
            mLineRenderer.enabled = false;
        }
    }
}