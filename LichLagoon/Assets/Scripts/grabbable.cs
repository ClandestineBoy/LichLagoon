using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grabbable : MonoBehaviour
{
    public bool buried;


    private bool inHand = false, inHandOne = false;

    public float grabScale;
    public float restScale;
    public float grabScale;
    public float restScale;
    private Vector3 grabS;
    private Vector3 restS;

    private bool inHand;

    private bool grabbed;
    private GameObject grabPos, UIAnchor;
    Rigidbody rb;
    public ParticleSystem ps;
    private float grabSpeed;

    public bool particlesFollowPlayer = true;

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

        if (inHand)
        {
            inHandFunc();
        }
        if (!grabbed)
        {
            inHandOne = false;
            ps.Stop();  //if not in hand, the object stop emitting particles into the lore UI
            //StartCoroutine(delayedClear());
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

    void inHandFunc()
    {
        if (!inHandOne)
        {
            ps.Play();  //plays UI particle effect when held
            inHandOne = true;
        }
        Debug.Log(ps.isPlaying);

        if (particlesFollowPlayer)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];

            for (float t = 0f; t < 1f; t += 0.1f)
            {
                int count = ps.GetParticles(particles);
                for (int i = 0; i < count; i++)
                {
                    particles[i].position = Vector3.Lerp(particles[i].position, UIAnchor.transform.position, Time.deltaTime / 3);
                }
                ps.SetParticles(particles, count);
            }
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
    IEnumerator delayedClear()
    {
        yield return new WaitForSeconds(2.5f);

        ps.Clear();
    }
}
