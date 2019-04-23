using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trailDashAlternator : MonoBehaviour
{

    public float offDur, onDur;
    private float timerI;

    public bool on;

    private TrailRenderer tr;

    // Start is called before the first frame update
    void Start()
    {
        tr = this.GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        timerI += Time.deltaTime;

        if (on)
        {
            if (!tr.emitting)
            {
                tr.emitting = true;
            }

            if (timerI >= onDur)
            {
                on = false;
                timerI = 0;
            }
        }
        else
        {
            if (tr.emitting)
            {
                tr.emitting = false;
            }

            if (timerI >= offDur)
            {
                on = true;
                timerI = 0;
            }
        }

        
    }
}
