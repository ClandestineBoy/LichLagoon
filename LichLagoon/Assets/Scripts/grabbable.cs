using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grabbable : MonoBehaviour
{
    public bool buried;


    public float grabScale;
    public float restScale;
    private Vector3 grabS;
    private Vector3 restS;

    private bool inHand;
    private bool grabbed;
    private GameObject grabPos;
    Rigidbody rb;
    private float grabSpeed;

    // Time when the movement started.
    private float startTime;

    // Total distance between the markers.
    private float journeyLength;

 
    void Start()
    {
        //attatch a Rigidbody to the object if there isnt one already
        if(GetComponent<Rigidbody>() == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        else
        {
            rb = GetComponent<Rigidbody>();
        }
        if (buried)
        {
            rb.isKinematic = true;
        }
        grabbed = false;
        grabBegin = false;
        restS = new Vector3(restScale, restScale, restScale);
        grabS = new Vector3(grabScale, grabScale, grabScale);
    }

    Vector3 startPos;
    private bool grabBegin = false;

    void Update()
    {
        if (grabBegin)
        {
            // Keep a note of the time the movement started.
            startTime = Time.time;
            startPos = transform.position;
            // Calculate the journey length.
            journeyLength = Vector3.Distance(restS, grabS);

            buried = false;
            rb.isKinematic = true;
            grabBegin = false;
        }

        if (grabbed)
        {   
            Grabbed();
        }
        else if (!buried)
        {
            transform.localScale = restS;
            rb.isKinematic = false;
        }
    }

    void Grabbed()
    {
        // Distance moved = time * speed.
        float distLeft = Vector3.Distance(transform.position, grabPos.transform.position) * Time.deltaTime;

        float shrinkDone = (Time.time - startTime)*3;
        float shrinkJourney = 1-(shrinkDone/journeyLength);

        // Set our position as a fraction of the distance between the markers.
        transform.position = Vector3.MoveTowards(transform.position, grabPos.transform.position, (distLeft*grabSpeed));
        transform.localScale = Vector3.Lerp(grabS, restS, shrinkJourney);

        if (Vector3.Distance(transform.position,grabPos.transform.position) < .1f)
        {
            inHand = true;
        }
    }

    public void setGrabbed(bool boo)
    {
        grabbed = boo;
        grabBegin = true;
    }
    public void setGrabObj(GameObject obj)
    {
        grabPos = obj;
    }
    public void setGrabSpeed(float speed)
    {
        grabSpeed = speed;
    }
    public void setInHand(bool boo)
    {
        inHand = boo;
    }
    public bool getInHand()
    {
        return inHand;
    }
}
