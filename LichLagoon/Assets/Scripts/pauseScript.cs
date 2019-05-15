using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pauseScript : MonoBehaviour
{

    public Image pauseImage, pausePrompt;
    public bool paused;

    public float quitTimer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!paused)    //unpaused
        {
            colourShift(pauseImage, null, new Color(1, 1, 1, 0), Time.deltaTime * 100, false, false);
            colourShift(pausePrompt, null, new Color(1, 1, 1, .75f), Time.deltaTime * 100, false, false);

            if (Input.GetKeyDown(KeyCode.P))
            {
                paused = true;
            }
        }
        else     //paused
        {
            colourShift(pauseImage, null, new Color(1, 1, 1, 1), Time.deltaTime * 100, false, false);
            colourShift(pausePrompt, null, new Color(1, 1, 1, 0), Time.deltaTime * 100, false, false);

            if (Input.GetKey(KeyCode.Escape))
            {
                quitTimer += Time.fixedDeltaTime;

                if (quitTimer > 1.2f)
                {
                    Application.Quit();
                }
            }
            else
            {
                quitTimer = 0f;
            }

            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Mouse0))
            {
                paused = false;
            }
        }
    }

    void colourShift(Image curImg, Text curText, Vector4 target, float speed, bool isText, bool isBody)
    {
        //Vector4 purpleMod = new Vector4(Random.Range(.9f, 1f), Random.Range(.6f, .8f), Random.Range(.7f, .9f), 1);

        if (isText)
        {
            if (isBody)
            {
                curText.color = Vector4.MoveTowards
                            (
                                curText.color,
                                target,
                                speed * Time.deltaTime
                            );
            }
            else
            {
                curText.color = Vector4.MoveTowards
                            (
                                curText.color,
                                target,
                                speed * Time.deltaTime
                            );
            }
        }
        else
        {
            curImg.color = Vector4.MoveTowards
                        (
                            curImg.color,
                            target,
                            speed * Time.deltaTime
                        );
        }

        ///Debug.Log("colourShift");
    }
}
