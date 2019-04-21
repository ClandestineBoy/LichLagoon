using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grabbable : MonoBehaviour
{
    public bool buried;

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
            journeyLength = Vector3.Distance(startPos, grabPos.transform.position);

            if (rb.isKinematic)
            {
                rb.isKinematic = false;
            }
            grabBegin = false;
        }

        if (grabbed)
        { 
            Grabbed();
        }
    }

    void Grabbed()
    {
        // Distance moved = time * speed.
        float distCovered = (Time.time - startTime) * grabSpeed;
        // Fraction of journey completed = current distance divided by total distance.
        float fracJourney = distCovered / journeyLength;
        // Set our position as a fraction of the distance between the markers.
        transform.position = Vector3.Lerp(startPos, grabPos.transform.position, fracJourney);

        if(transform.position == grabPos.transform.position)
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
