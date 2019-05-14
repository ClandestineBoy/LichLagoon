using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraClamp : MonoBehaviour
{

    private float rotY;
    public float rotMin;
    public float rotMax;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rotY = this.transform.eulerAngles.y;
        Debug.Log("before: " + rotY + "  |  " + Time.time);
        rotY = Mathf.Clamp(rotY, rotMin, rotMax);
        Debug.Log("before: " + rotY + "  |  " + Time.time);
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, rotY, this.transform.eulerAngles.z);
    }
}
