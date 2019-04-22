﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bobberScript : MonoBehaviour
{
    [Header("Bob")]
    public float bobTimerMax = 0f, bobTimerI = 0f;
	public bool bobs = false, upping = true;
    public float bobSpeed = 0f, bobAmpMod = 0f, bobSpeedMod = 0f;

	[Header("Grow")]
	public Vector3 topScale, botScale;
	public bool shrinkGrows = false, growing = true;
	private float growSpeed = 0f;
	public float growSpeedMod = 0f;

	void Start()	{
		
	}

    // Update is called once per frame
    void Update()
    {
		if (bobs) {
            bobSpeed = Mathf.Sin(bobTimerI * Mathf.PI) * (bobAmpMod / 1000);
            //bobSpeed = ((bobSpeedMod / (bobTimerI + 1f)) * Time.deltaTime);
            //bobSpeed = (bobSpeedMod * Time.deltaTime) / (Mathf.Abs(bobTimerI - (bobTimerMax / 2)) * 10);
            bobTimerI += Time.deltaTime / bobSpeedMod;

            if (bobTimerI >= bobTimerMax)
            {
                bobTimerI = 0;
                upping = false;
            }

            this.transform.position += Vector3.up * bobSpeed;
		}

		if (shrinkGrows) {
			if (growing) {
				growSpeed = Vector3.Distance (this.transform.localScale, topScale) * growSpeedMod + .01f;
				this.transform.localScale = Vector3.MoveTowards (this.transform.localScale, topScale, Time.deltaTime * growSpeed);

				if (this.transform.localScale.magnitude >= topScale.magnitude) {
					growing = false;
				}
			}
			else {
				growSpeed = Vector3.Distance (this.transform.localScale, botScale) * growSpeedMod + .01f;
				this.transform.localScale = Vector3.MoveTowards (this.transform.localScale, botScale, Time.deltaTime * growSpeed);

				if (this.transform.localScale.magnitude <= botScale.magnitude) {
					growing = true;
				}
			}
		}
    }
}
