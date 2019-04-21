using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grabbable : MonoBehaviour
{
    public bool grabbed;
    public GameObject grabPos;
    Rigidbody rb;
    public float grabSpeed;

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
        grabbed = false;
        grabBegin = false;
    }

    Vector3 startPos;
    public bool grabBegin = false;

    void Update()
    {
        if (grabBegin)
        {
            // Keep a note of the time the movement started.
            startTime = Time.time;
            startPos = transform.position;
            // Calculate the journey length.
            journeyLength = Vector3.Distance(startPos, grabPos.transform.position);

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
    }
}
