using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun_Rotation : MonoBehaviour
{
    public bool isSun;
    public bool isMoon;

    public bool night;
    public bool day = true;

    public GameObject Sun;
    public GameObject Moon;

    public Material daySky;
    public Material nightSky;

    bool goToNight;

    float exposure = 0;
    // Start is called before the first frame update
    void Start()
    {
        RenderSettings.skybox = daySky;
        goToNight = false;   
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            goToNight = true;
        }
        if (goToNight && day)
        {
            transform.RotateAround(Vector3.zero, Vector3.right, .5f);
        }
        if (isSun && transform.position.y <= -45 && day)
        {
            night = true;
            day = false;
            RenderSettings.skybox = nightSky;
            RenderSettings.skybox.SetFloat("_Exposure", 0);
            Moon.GetComponent<Light>().intensity = 0;
        }
        else if(night)
        {
            if(GetComponent<Light>().intensity > 0)
            {
                GetComponent<Light>().intensity -= .01f;
            }
            if(Moon.GetComponent<Light>().intensity < .1f)
            {
                Moon.GetComponent<Light>().intensity += .0005f;
            }
            if(exposure < 1)
            {
                exposure += .005f;
            }
            RenderSettings.skybox.SetFloat("_Exposure", exposure);
        }   
    }
}
