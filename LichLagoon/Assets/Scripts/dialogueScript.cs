﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class dialogueScript : MonoBehaviour
{

    [System.Serializable]
    public struct NPC
    {
        public Text currentDialogue;
        public Image diagBacker, portrait;
        public GameObject heldObj;    //This and the below stuff probably wont be used in this intro script
        public float heldResponseValue, delayResponseValue;
        public bool primaryActive, secondaryActive;
    };

    public NPC fran;
    public NPC gunn;
    public NPC rob;

    [Header("You")]
    public bool answering = false, playerSpeaking = false;
    public Text youDiag, oneAnswer, twoAnswer, thrAnswer;
    public Image youBacker;
    public string answerTag = " ";  //determines where in conversation we are when player is given a chance to answer

    [Header("Colours")]
    public Color silentPortrait;    //activePortraits are just color.white
    public Color tranWhite, tranBlack;
    public float fadeOutSpeed = 0f, fadeInSpeed = 0f;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Start is called before the first frame update
    void Start()
    {
        //initialise npc dialogue
        fran.currentDialogue.text = "";
        fran.diagBacker.color = new Color(1, 1, 1, 0);

        gunn.currentDialogue.text = "";
        gunn.diagBacker.color = new Color(1, 1, 1, 0);

        rob.currentDialogue.text = "";
        rob.diagBacker.color = new Color(1, 1, 1, 0);

        youDiag.text = "";
        youBacker.color = new Color(1, 1, 1, 0);

        StartCoroutine(poseQuestion(rob, 1.5f, true,
                    "I'm the loremaster!",
                    1f, true, 2f));
    }


    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 3; i++)
        {
            NPC speaker = new NPC();

            if (i == 0)
            {
                speaker = fran;
            }
            else if (i == 1)
            {
                speaker = gunn;
            }
            else if (i == 2)
            {
                speaker = rob;
            }

            if (speaker.primaryActive)     //if last to speak
            {
                colourShift(speaker.diagBacker, null, Color.white, Time.deltaTime * fadeInSpeed, false);
                colourShift(speaker.portrait, null, Color.white, Time.deltaTime * fadeInSpeed, false);
                colourShift(null, speaker.currentDialogue, Color.black, Time.deltaTime * fadeInSpeed, true);
            }
            else if (speaker.secondaryActive)  //if has spoken in current exchange, but not most recently
            {
                colourShift(speaker.diagBacker, null, silentPortrait, Time.deltaTime * fadeInSpeed, false);
                colourShift(speaker.portrait, null, silentPortrait, Time.deltaTime * fadeInSpeed, false);
                colourShift(null, speaker.currentDialogue, Color.black, Time.deltaTime * fadeInSpeed, true);
            }
            else     //hasn't spoken yet
            {
                colourShift(speaker.diagBacker, null, tranWhite, Time.deltaTime * fadeOutSpeed, false);
                colourShift(speaker.portrait, null, silentPortrait, Time.deltaTime * fadeOutSpeed, false);
                colourShift(null, speaker.currentDialogue, tranBlack, Time.deltaTime * fadeOutSpeed, true);
            }
        }

        if (answering)  //if the player now has the ability to respond in conversation
        {
            answerFunc();
        }

        else if (playerSpeaking)
        {
            colourShift(youBacker, null, Color.white, Time.deltaTime * fadeInSpeed, false);
            colourShift(null, youDiag, Color.black, Time.deltaTime * fadeInSpeed, true);
            colourShift(null, oneAnswer, Color.black, Time.deltaTime * fadeInSpeed, true);
            colourShift(null, twoAnswer, Color.black, Time.deltaTime * fadeInSpeed, true);
            colourShift(null, thrAnswer, Color.black, Time.deltaTime * fadeInSpeed, true);
        }
        else    //fade out player dialogue when they aren't talking/answering
        {
            colourShift(youBacker, null, tranWhite, Time.deltaTime * fadeOutSpeed, false);
            colourShift(null, youDiag, tranBlack, Time.deltaTime * fadeOutSpeed, true);
            colourShift(null, oneAnswer, tranBlack, Time.deltaTime * fadeOutSpeed, true);
            colourShift(null, twoAnswer, tranBlack, Time.deltaTime * fadeOutSpeed, true);
            colourShift(null, thrAnswer, tranBlack, Time.deltaTime * fadeOutSpeed, true);
        }
    }

///_______________________________________________________________________________________________________________________________________________________________________
//////////////////////PLAYER SCRIPT
///_______________________________________________________________________________________________________________________________________________________________________
    void answerFunc ()
    {
        colourShift(youBacker, null, Color.white, Time.deltaTime * fadeInSpeed, false);
        colourShift(null, youDiag, Color.black, Time.deltaTime * fadeInSpeed, true);
        colourShift(null, oneAnswer, Color.black, Time.deltaTime * fadeInSpeed, true);
        colourShift(null, twoAnswer, Color.black, Time.deltaTime * fadeInSpeed, true);
        colourShift(null, thrAnswer, Color.black, Time.deltaTime * fadeInSpeed, true);

        if (answerTag == "A01")
        {
            youDiag.text = "I understand. Let's begin: tell me about your...";

            oneAnswer.text = "1) Childhood";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Family";
            thrAnswer.text = "3) Occupation";

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "I understand. Let's begin: tell me about your childhood.";
                answering = false;  //stops updating this answer section so it doesn't show the next choices

                answerTag = "A01a"; //preemptively updates this answer function to be ready with the choices for the next answer

                StartCoroutine(poseQuestion(rob, 1.5f, false,
                    "My dad is cool[placeholder].",
                    2f, false, 3f));   //comment this out and fill it out - this is the next line read from the next npc
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "I understand. Let's begin: tell me about your family.";
                answering = false;

                answerTag = "A01b";

                //StartCoroutine(poseQuestion());
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {
                youDiag.text = "I understand. Let's begin: tell me about your occupation.";
                answering = false;

                answerTag = "A01c";

                //StartCoroutine(poseQuestion());
            }
        }

    }

///_______________________________________________________________________________________________________________________________________________________________________
//////////////////////END PLAYER SCRIPT
///_______________________________________________________________________________________________________________________________________________________________________

    IEnumerator poseQuestion(NPC speaker, float initDelay, bool isPlayer, string line, float endDelay, bool trigger, float postDelay)
    {
        /*There will never be downtime during a conversation, codewise (we're always running an enumerator):
         FIRST) A pose question function is called; this is a delay, then an npc poses a question (or stays silent);
                this either calls another pose question for another npc to pipe in, or an answer from the player is called instead;

         SECOND) We end this function, then flip states to enable the player to answer

         THIRD) The player's answer activates a response from an npc; the end of this response either calls another response or
                calls a new pose question (starting with another delay)
         */
         
        yield return new WaitForSeconds(initDelay);

        if (!isPlayer)    //if an npc is speaking
        {
            speaker.primaryActive = true; speaker.secondaryActive = false;     //fade in speaker's dialogue box, dialogue text, and brighten their portrait
            
            speaker.currentDialogue.text = line;
        }
        else
        {
            playerSpeaking = true;
        }

        yield return new WaitForSeconds(endDelay);

        if (trigger)    //if next is the player talking
        {
            answering = true;
        }
        else     //if another NPC is gonna pipe in before player talking
        {
            //StartCoroutine(poseQuestion());
        }

        yield return new WaitForSeconds(postDelay);     //pause before darkening this most recent line


        if (!isPlayer)
        {
            speaker.primaryActive = false; speaker.secondaryActive = true;    //darken this most recent text
        }
        else
        {
            playerSpeaking = false;     //fade player text to transparent
        }

        
    }


    void colourShift(Image curImg, Text curText, Vector4 target, float speed, bool isText)
    {
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
    }
}
