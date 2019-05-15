using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class introProcedure : MonoBehaviour
{
    /*ok what does this need to do: have an intro backer, ship, and ship's trail.
    Then, it'll need a reference to the player's dialogue box, dialogue text element.
    It'll also need references to a Boat parent object (this will move and bob with everything else aboard).
    Then, references to the npc sprites, as well as their dialogue backers and text. They should probably be a class?
    Lastly, we'll need a fadeOutRect to blacken when the intro is over.
    */

    [Header ("Fade")]
    public Image fadeRect;
    public float fadeInSpeed, fadeOutSpeed;
    public GameObject intro0, intro1, intro2, intro3;

    [Header("Player")]
    public Image playerDiagBack;
    public Text playerDiag;
    public GameObject player, playFulcrum;

    [Header("Boat")]
    public GameObject boatParent;

    public class NPC
    {
        public string currentDialogue;
        public Image diagBacker, portrait;
        public Text intro0, intro1, intro2, intro3;
        //public GameObject heldObj;    //This and the below stuff probably wont be used in this intro script
        //public float heldResponseValue, delayResponseValue;
    };

    public bool firstMapVisible = true, atFadeIn = false, inBoatConvo = false, atFadeOut = false;

    // Start is called before the first frame update
    void Start()
    {
        ///Debug.Log("introProcObj = " + this.gameObject.name);
        StartCoroutine(firstMapEnum());
    }

    // Update is called once per frame
    void Update()
    {
        if (firstMapVisible)
        {
            player.transform.localEulerAngles = new Vector3(0, 180, 0);
            playFulcrum.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        if (atFadeIn)
        {
            firstMapVisible = false;

            player.transform.localEulerAngles = new Vector3(player.transform.localEulerAngles.x, player.transform.localEulerAngles.y, player.transform.localEulerAngles.z);
            playFulcrum.transform.localEulerAngles = new Vector3(playFulcrum.transform.localEulerAngles.x, playFulcrum.transform.localEulerAngles.y, playFulcrum.transform.localEulerAngles.z);

            colourShift(fadeRect, null, new Color(0, 0, 0, 0), fadeInSpeed * .8f, false);
            colourShift(null, intro0.GetComponent<Text>(), new Color(1, 1, 1, 0), fadeInSpeed * 2, true);
            colourShift(null, intro1.GetComponent<Text>(), new Color(1, 1, 1, 0), fadeInSpeed * 2, true);
            colourShift(null, intro2.GetComponent<Text>(), new Color(1, 1, 1, 0), fadeInSpeed * 2, true);
            colourShift(null, intro3.GetComponent<Text>(), new Color(1, 1, 1, 0), fadeInSpeed * 2, true);

            if (fadeRect.color == new Color (0, 0, 0, 0))
            {
                atFadeIn = false; //first map just means fade rect now
                inBoatConvo = true;
                ///this.GetComponent<dialogueScript>().enabled = true;
            }
        }
        else if (inBoatConvo)
        {
            player.GetComponent<pauseScript>().enabled = true;
            //nothing? that i can think of rn
        }
        else if (atFadeOut)
        {
            colourShift(fadeRect, null, new Color(0, 0, 0, 1), fadeOutSpeed, false);

            if (fadeRect.color == new Color(0, 0, 0, 1))
            {
                //SceneManager.LoadScene("");
            }
        }
    }

    IEnumerator firstMapEnum ()
    {
        yield return new WaitForSeconds(2f);
        intro0.SetActive(true);

        yield return new WaitForSeconds(6f);
        intro1.SetActive(true);

        yield return new WaitForSeconds(6f);
        intro2.SetActive(true);

        yield return new WaitForSeconds(6f);
        intro3.SetActive(true);

        yield return new WaitForSeconds(7f);
        atFadeIn = true;

        yield return new WaitForSeconds(6f);
        this.GetComponent<dialogueScript>().enabled = true;
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
