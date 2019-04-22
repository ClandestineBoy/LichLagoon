﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotater : MonoBehaviour
{

    public bool activeRotate = false;
    public Vector3 dir = new Vector3 (0, 0, 0);
    public float rotSpeed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activeRotate)
        {
            rotaterFunc();
        }
    }

    void rotaterFunc()
    {
        this.transform.localEulerAngles += dir * rotSpeed * Time.deltaTime;
    }
}
