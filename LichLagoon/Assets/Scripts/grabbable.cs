using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grabbable : MonoBehaviour
{
    public bool buried;

    private bool inHand;
    private bool grabbed;
    private GameObject grabPos, UIAnchor;
    Rigidbody rb;
    ParticleSystem ps;
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

        ps = GetComponent<ParticleSystem>();
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

            buried = false;
            rb.isKinematic = true;
            grabBegin = false;
        }

        if (inHand)
        {
            inHandFunc();
        }
        else
        {
            ps.Stop();  //if not in hand, the object stop emitting particles into the lore UI
        }

        if (grabbed)
        {
            Grabbed();

           
        }
        else if (!buried)
        {
            rb.isKinematic = false;
        }
    }

    void Grabbed()
    {
        // Distance moved = time * speed.
        float distLeft = Vector3.Distance(transform.position, grabPos.transform.position) * Time.deltaTime;

        // Set our position as a fraction of the distance between the markers.
        transform.position = Vector3.MoveTowards(transform.position, grabPos.transform.position, (distLeft*grabSpeed));

        if(Vector3.Distance(transform.position,grabPos.transform.position) < .1f)
        {
            inHand = true;
        }
    }

    void inHandFunc()
    {
        Debug.Log("inHandFunc: " + Time.time);
        if (!ps.isPlaying)
        {
            ps.Play();  //plays UI particle effect when held
        }

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];

        for (float t = 0f; t < 1f; t += 0.1f)
        {
            int count = ps.GetParticles(particles);
            for (int i = 0; i < count; i++)
            {
                particles[i].position = Vector3.Lerp(particles[i].position, UIAnchor.transform.localPosition, Time.deltaTime / 3);
            }
            ps.SetParticles(particles, count);
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
    public void setUIAnchor(GameObject obj)
    {
        UIAnchor = obj;
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
