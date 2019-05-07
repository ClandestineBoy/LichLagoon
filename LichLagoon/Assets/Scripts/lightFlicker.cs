using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightFlicker : MonoBehaviour
{
    Light thisLight;

    public float startingIntensity = 0f;
    public float intensityRange = 0f;
    private float targetIntensity = 0f;

    public bool flickers = false;
    public bool vacillates = false, upping = false;

    public float flickerDelay = 0f, flickerSpeed = 0f; private float flickerI = 0f;

    // Start is called before the first frame update
    void Start()
    {
        thisLight = this.GetComponent<Light>();
        startingIntensity = thisLight.intensity;

        flickerDelay = Random.Range(.1f, .15f);
        flickerI = flickerDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if (flickers)
        {
            Flicker();
            thisLight.intensity = Mathf.MoveTowards(thisLight.intensity, targetIntensity, Time.deltaTime * flickerSpeed);
        }
        else if (vacillates)
        {
            Vacillate();
        }
    }

    void Flicker()
    {
        flickerI -= Time.deltaTime;

        if (flickerI <= 0)
        {
            flickerDelay = Random.Range(.1f, .15f);
            flickerI = flickerDelay;
            targetIntensity = Random.Range((startingIntensity - intensityRange), (startingIntensity + intensityRange));
        }
    }

    void Vacillate()
    {

    }
}
