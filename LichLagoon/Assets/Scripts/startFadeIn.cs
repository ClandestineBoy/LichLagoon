using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class startFadeIn : MonoBehaviour
{

    public bool In, isText;
    public float fadeSpeed, transMod;

    // Update is called once per frame
    void Update()
    {
        if (!In)
        {
            if (isText) {
                colourShift(null, this.GetComponent<Text>(), new Color(1, 1, 1, transMod), fadeSpeed * Time.deltaTime, true);

                if (this.GetComponent<Text>().color == new Color (1, 1, 1, transMod))
                {
                    In = true;
                }
            }
            else
            {
                colourShift(this.GetComponent<Image>(), null, new Color(1, 1, 1, transMod), fadeSpeed * Time.deltaTime, false);

                if (this.GetComponent<Image>().color == new Color(1, 1, 1, transMod))
                {
                    In = true;
                }
            }
        }
    }

    void colourShift(Image curImg, Text curText, Vector4 target, float speed, bool isText)
    {
        //Vector4 purpleMod = new Vector4(Random.Range(.9f, 1f), Random.Range(.6f, .8f), Random.Range(.7f, .9f), 1);

        if (isText)
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
