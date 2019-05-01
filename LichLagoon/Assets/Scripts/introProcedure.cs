using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class introProcedure : MonoBehaviour
{
    /*ok what does this need to do: have an intro backer, ship, and ship's trail.
    Then, it'll need a reference to the player's dialogue box, dialogue text element.
    It'll also need references to a Boat parent object (this will move and bob with everything else aboard).
    Then, references to the npc sprites, as well as their dialogue backers and text. They should probably be a class?
    Lastly, we'll need a fadeOutRect to blacken when the intro is over.
    */

    [Header ("Map")]
    public Image mapBacker, mapShip;

    [Header("Player")]
    public Image playerDiagBack;
    public Text playerDiag;

    [Header("Boat")]
    public GameObject boatParent;

    public class NPC
    {
        public string currentDialogue;
        public Image diagBacker, portrait;
        //public GameObject heldObj;    //This and the below stuff probably wont be used in this intro script
        //public float heldResponseValue, delayResponseValue;
    };

    public Image fadeOutRect;

    private bool firstMapVisible = false, inBoatConvo = false, atFadeOut = false;

    // Start is called before the first frame update
    void Start()
    {
        firstMapVisible = true;
        StartCoroutine(firstMapEnum());
    }

    // Update is called once per frame
    void Update()
    {
        if (firstMapVisible)
        {

        }
        else if (inBoatConvo)
        {
            colourShift(mapBacker, null, new Color(1, 1, 1, 0), Time.deltaTime, false);     //fade out map
            colourShift(mapShip, null, new Color(1, 1, 1, 0), Time.deltaTime, false);     //fade out ship on map
            mapShip.GetComponent<TrailRenderer>().startColor = Vector4.MoveTowards      //fade out ship's dotted trail
                (
                    mapShip.GetComponent<TrailRenderer>().startColor, new Color (1, 1, 1, 0), Time.deltaTime
                );
        }
        else if (atFadeOut)
        {

        }
    }

    IEnumerator firstMapEnum ()
    {


        return null;
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
