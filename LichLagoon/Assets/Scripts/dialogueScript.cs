using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class dialogueScript : MonoBehaviour
{

    [System.Serializable]
    public class NPC
    {
        public Text currentDialogue, header;
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
    public Color silentPortrait, silentDiag, backerBase;    //activePortraits are just color.white
    public Color tranWhite, tranBlack;
    public float fadeOutSpeed = 0f, fadeInSpeed = 0f;

    [Header("Next Line Data")]
    public NPC[] Xnpc = new NPC [20];
    float[] XinitDelay = new float[20], XendDelay = new float[20], XpostDelay = new float[20];
    bool[] XisPlayer = new bool[20], Xtrigger = new bool[20];
    string[] Xline = new string[20];
    int lineI = -1;

    private float[] timer = new float[4];
    public string dialogueID = "";

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
        youDiag.color = tranBlack;
        youBacker.color = tranWhite;

        oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

        if (dialogueID == "Intro")
        {
            StartCoroutine(poseQuestion(rob, 1.5f, false,    //any name will work for null under "NPC"
                        "You've gotta run me through this just one more time.",
                        3f, true, .5f));

            answerTag = "A01";
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))        //DEBUG REMOVE THIS FOR FINAL
        {
            Time.timeScale = 2.5f;              //DEBUG REMOVE THIS FOR FINAL
        }
        else                                    //DEBUG REMOVE THIS FOR FINAL
        {
            Time.timeScale = 1f;                //DEBUG REMOVE THIS FOR FINAL
        }

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
                colourShift(speaker.diagBacker, null, backerBase, Time.deltaTime * fadeInSpeed, false);
                colourShift(speaker.portrait, null, Color.white, Time.deltaTime * fadeInSpeed, false);
                colourShift(null, speaker.currentDialogue, Color.white, Time.deltaTime * fadeInSpeed, true);
                colourShift(null, speaker.header, Color.white, Time.deltaTime * fadeInSpeed, true);

                timer[i] = 0;
            }
            else if (speaker.secondaryActive)  //if has spoken in current exchange, but not most recently
            {
                colourShift(speaker.diagBacker, null, backerBase, Time.deltaTime * fadeInSpeed, false);
                colourShift(speaker.portrait, null, silentPortrait, Time.deltaTime * fadeInSpeed, false);
                colourShift(null, speaker.currentDialogue, silentDiag, Time.deltaTime * fadeInSpeed, true);
                colourShift(null, speaker.header, silentDiag, Time.deltaTime * fadeInSpeed, true);

                timer[i] += Time.deltaTime;

                if (timer[i] > 9f)
                {
                    speaker.secondaryActive = false;
                    timer[i] = 0;
                }
            }
            else     //hasn't spoken yet
            {
                colourShift(speaker.diagBacker, null, tranBlack, Time.deltaTime * (fadeOutSpeed + 2), false);
                colourShift(speaker.portrait, null, silentPortrait, Time.deltaTime * (fadeOutSpeed + 2), false);
                colourShift(null, speaker.currentDialogue, tranWhite, Time.deltaTime * (fadeOutSpeed + 2), true);
                colourShift(null, speaker.header, tranWhite, Time.deltaTime * fadeInSpeed, true);
            }
        }

        if (answering)  //if the player now has the ability to respond in conversation
        {
            answerFunc();
            timer[3] = 0;
        }

        else if (playerSpeaking)
        {
            timer[3] = 0;

            colourShift(youBacker, null, backerBase, Time.deltaTime * fadeInSpeed, false);
            colourShift(null, youDiag, Color.white, Time.deltaTime * fadeInSpeed, true);
            colourShift(null, oneAnswer, Color.white, Time.deltaTime * fadeInSpeed, true);
            colourShift(null, twoAnswer, Color.white, Time.deltaTime * fadeInSpeed, true);
            colourShift(null, thrAnswer, Color.white, Time.deltaTime * fadeInSpeed, true);
        }
        else    //fade out player dialogue when they aren't talking/answering
        {
            if (timer[3] > 2)
            {
                colourShift(youBacker, null, tranBlack, Time.deltaTime * fadeOutSpeed, false);
                colourShift(null, youDiag, tranWhite, Time.deltaTime * fadeOutSpeed, true);
                colourShift(null, oneAnswer, tranWhite, Time.deltaTime * fadeOutSpeed, true);
                colourShift(null, twoAnswer, tranWhite, Time.deltaTime * fadeOutSpeed, true);
                colourShift(null, thrAnswer, tranWhite, Time.deltaTime * fadeOutSpeed, true);
            }
            else
            {
                timer[3] += Time.deltaTime;
            }
        }
    }

///_______________________________________________________________________________________________________________________________________________________________________
//////////////////////PLAYER SCRIPT
///_______________________________________________________________________________________________________________________________________________________________________
    void answerFunc ()
    {
        lineI = -1;

        colourShift(youBacker, null, backerBase, Time.deltaTime * fadeInSpeed, false);
        colourShift(null, youDiag, Color.white, Time.deltaTime * fadeInSpeed, true);
        colourShift(null, oneAnswer, Color.white, Time.deltaTime * fadeInSpeed, true);
        colourShift(null, twoAnswer, Color.white, Time.deltaTime * fadeInSpeed, true);
        colourShift(null, thrAnswer, Color.white, Time.deltaTime * fadeInSpeed, true);

        ///*********LINE************************************************************************************************///
        if (answerTag == "A01")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Don't explain. (Skip Tutorial)";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Explain again.";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "I think you understand.";
                answering = false;  //stops updating this answer section so it doesn't show the next choices

                answerTag = "A01a"; //preemptively updates this answer function to be ready with the choices for the next answer

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";  //zero out dialogue choices

                StartCoroutine(poseQuestion(rob, 1.5f, false,
                    "Well pardon me. It's my first time. I'm nervous.",
                    3f, false, .5f));   //comment this out and fill it out - this is the next line read from the next npc

                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 4f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "We are all new to this. Yet here we sit, silent.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 3.5f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "...";

                Xnpc[2] = rob; XinitDelay[2] = 0f; XendDelay[2] = 2.5f; XpostDelay[2] = 2.5f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "Okay, okay...Nevermind.";

                //END INTRO
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "Okay, but please: listen this time.";
                answering = false;

                StartCoroutine(poseQuestion(rob, 2f, false,
                   "I'll try me best.",
                   3f, false, .5f));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 3f; XpostDelay[0] = .5f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Do more than try. My ears ache.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 3f; XpostDelay[1] = 2f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Good thing you don't have those anymore.";

                Xnpc[2] = null; XinitDelay[2] = 0f; XendDelay[2] = 6f; XpostDelay[2] = .1f; XisPlayer[2] = true; Xtrigger[2] = false;
                Xline[2] = "Now, pay attention--you are all dead; you think, move, breathe, but I assure you, you are dead.";

                Xnpc[3] = null; XinitDelay[3] = 0f; XendDelay[3] = 5.5f; XpostDelay[3] = .5f; XisPlayer[3] = true; Xtrigger[3] = false;
                Xline[3] = "Everything else your corpse can do is all thanks to The Captain; she turned you to Liches, un-dead.";

                Xnpc[4] = rob; XinitDelay[4] = 0f; XendDelay[4] = 3f; XpostDelay[4] = .5f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "That explains a few things.";

                Xnpc[5] = null; XinitDelay[5] = 0f; XendDelay[5] = 7f; XpostDelay[5] = .5f; XisPlayer[5] = true; Xtrigger[5] = false;
                Xline[5] = "All Lichs, if they so please, may join Captain Jones's crew, the most formidable pirates this side of the afterlife.";

                Xnpc[6] = fran; XinitDelay[6] = 0; XendDelay[6] = 3f; XpostDelay[6] = 1f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "So we're monsters *and* criminals?";

                Xnpc[7] = null; XinitDelay[7] = 0; XendDelay[7] = 7f; XpostDelay[7] = .25f; XisPlayer[7] = true; Xtrigger[7] = false;
                Xline[7] = "If you like. Either way, you're dead now. You're fugitives. Paladin's don't see us, they see monsters.";

                Xnpc[8] = null; XinitDelay[8] = 0; XendDelay[8] = 6f; XpostDelay[8] = .5f; XisPlayer[8] = true; Xtrigger[8] = false;
                Xline[8] = "So we need to find a way of keeping you alive, whether you join us or not. That's where I lend a hand.";

                Xnpc[9] = gunn; XinitDelay[9] = 0; XendDelay[9] = 3f; XpostDelay[9] = .5f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "You forge armor?";

                Xnpc[10] = null; XinitDelay[10] = 0; XendDelay[10] = 7f; XpostDelay[10] = .25f; XisPlayer[10] = true; Xtrigger[10] = false;
                Xline[10] = "Of a sort. Your corpse is a vessel, animated but decaying. A dead body cannot sustain a living soul.";

                Xnpc[11] = null; XinitDelay[11] = 0; XendDelay[11] = 7.5f; XpostDelay[11] = .25f; XisPlayer[11] = true; Xtrigger[11] = false;
                Xline[11] = "So we take the soul and store it elsewhere: an object to house your spirit, a phylactery. Once the soul is stored, your corpse becomes indestructible...to a point.";

                Xnpc[12] = null; XinitDelay[12] = 0; XendDelay[12] = 5.5f; XpostDelay[12] = 1f; XisPlayer[12] = true; Xtrigger[12] = false;
                Xline[12] = "This is my task; pirate or not, I will find each of you a phylactery.";

                Xnpc[13] = fran; XinitDelay[13] = 0; XendDelay[13] = 3f; XpostDelay[13] = .25f; XisPlayer[13] = false; Xtrigger[13] = true;
                Xline[13] = "--And if we refuse your help?";

                answerTag = "A01b";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {
                youDiag.text = "";
                answering = false;

                answerTag = "";

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                //StartCoroutine(poseQuestion());
            }

            return;
        }

        if (answerTag == "A01b")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Insist";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Be blunt";
            thrAnswer.text = "3) Warn";

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "Nobody's ever refused my help before. I would highly recommend you take it.";
                answering = false;

                answerTag = "A01b1";

                StartCoroutine(poseQuestion(fran, 2.5f, false,
                    "And why's that?",
                    2.5f, false, 0f));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = null; XinitDelay[0] = 0f; XendDelay[0] = 4f; XpostDelay[0] = .25f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "Without a phylactery you will decompose in three days at best.";

                Xnpc[1] = null; XinitDelay[1] = 0f; XendDelay[1] = 5.5f; XpostDelay[1] = .25f; XisPlayer[1] = true; Xtrigger[1] = false;
                Xline[1] = "Even if you look for one alone, I doubt you'd manage to find anything suitable.";

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 3.5f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[1] = false;
                Xline[2] = "I'll do just fine without the help of a pirate.";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 4.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "As will I, no offense. I'm just pretty set on putting my soul into a cannon ball.";

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 2.5f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "I could find an anvil.";

                Xnpc[5] = null; XinitDelay[5] = 0; XendDelay[5] = 5.5f; XpostDelay[5] = 1f; XisPlayer[5] = true; Xtrigger[5] = false;
                Xline[5] = "All fine suggestions, I'm sure. The problem's that you must consider resonance.";

                Xnpc[6] = rob; XinitDelay[6] = 0; XendDelay[6] = 2.5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "..Like music?";

                Xnpc[7] = null; XinitDelay[7] = 0; XendDelay[7] = 6f; XpostDelay[7] = .5f; XisPlayer[7] = true; Xtrigger[7] = false;
                Xline[7] = "Almost. See, a soul has to resonate with its phylactery. You can't just stick it into anything.";

                Xnpc[8] = null; XinitDelay[8] = 0; XendDelay[8] = 6.5f; XpostDelay[8] = .5f; XisPlayer[8] = true; Xtrigger[8] = false;
                Xline[8] = "Objects and souls have histories, and those histories need to form balance, else the phylactery will corrupt.";

                Xnpc[9] = null; XinitDelay[9] = 0; XendDelay[9] = 7f; XpostDelay[9] = 1f; XisPlayer[9] = true; Xtrigger[9] = false;
                Xline[9] = "I can see these histories; it's a magical talent. Objects reveal things to me, and so I find phylacteries for our newest recruits.";

                Xnpc[10] = gunn; XinitDelay[10] = 0; XendDelay[10] = 3f; XpostDelay[10] = .5f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "You find harmony. How can we help?";

                Xnpc[11] = null; XinitDelay[11] = 0; XendDelay[11] = 6f; XpostDelay[11] = 2f; XisPlayer[11] = true; Xtrigger[11] = false;
                Xline[11] = "Well...tell me your feelings! The better I know you, the better home I can find for your soul.";

                Xnpc[12] = gunn; XinitDelay[12] = 0; XendDelay[12] = 3f; XpostDelay[12] = .75f; XisPlayer[12] = false; Xtrigger[12] = false;
                Xline[12] = "Then you should know I don't relish conversation.";

                Xnpc[13] = fran; XinitDelay[13] = 0; XendDelay[13] = 3f; XpostDelay[13] = .25f; XisPlayer[13] = false; Xtrigger[13] = false;
                Xline[13] = "I'd rather be tortured...";

                Xnpc[14] = rob; XinitDelay[14] = 0; XendDelay[14] = 3.5f; XpostDelay[14] = .75f; XisPlayer[14] = false; Xtrigger[14] = false;
                Xline[14] = "How's one go about torturing a skeleton?";

                Xnpc[15] = null; XinitDelay[15] = 0; XendDelay[15] = 2.5f; XpostDelay[15] = .5f; XisPlayer[15] = true; Xtrigger[15] = false;
                Xline[15] = "...No idea.";

                //END INTRO
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "Well, without a phylactery you'll decompose in a week at best. That's assuming the Paladin's don't get to you first.";
                answering = false;

                answerTag = "A01b2";

                StartCoroutine(poseQuestion(fran, 1.5f, false,
                    "I welcome their arrival.",
                    3f, false, 2f));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = null; XinitDelay[0] = 0; XendDelay[0] = 6f; XpostDelay[0] = .75f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "They won't spare you. I've seen Paladin's slay their own before. You will befall a similar fate without our help.";

                Xnpc[1] = rob; XinitDelay[1] = 0; XendDelay[1] = 4.5f; XpostDelay[1] = 1f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "So why you? I wanna pick what my soul goes into, it's my damn soul.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 3.5f; XpostDelay[2] = .5f; XisPlayer[2] = false; Xtrigger[1] = false;
                Xline[2] = "...Will a soul fit inside a cannon ball?";

                Xnpc[3] = null; XinitDelay[3] = 0; XendDelay[3] = 7f; XpostDelay[3] = .25f; XisPlayer[3] = true; Xtrigger[3] = false;
                Xline[3] = "Well, yes...but that's not exactly the point. You see, a soul has to resonate with its phylactery. You can't just stick it into anything.";

                Xnpc[4] = null; XinitDelay[4] = 0; XendDelay[4] = 5.5f; XpostDelay[4] = .25f; XisPlayer[4] = true; Xtrigger[4] = false;
                Xline[4] = "Objects and souls have histories and those histories need to form balance, else the phylactery will corrupt.";

                Xnpc[5] = null; XinitDelay[5] = 0; XendDelay[5] = 7f; XpostDelay[5] = .5f; XisPlayer[5] = true; Xtrigger[5] = false;
                Xline[5] = "I can see these histories; it's a magical talent. Objects reveal things to me, and so I find phylacteries for our newest recruits.";

                Xnpc[6] = gunn; XinitDelay[6] = 0; XendDelay[6] = 2.5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "You find harmony. How can we help?";

                Xnpc[7] = null; XinitDelay[7] = 0; XendDelay[7] = 5.5f; XpostDelay[7] = .25f; XisPlayer[7] = true; Xtrigger[7] = false;
                Xline[7] = "Well...tell me your feelings! The better I know you, the better home I can find for your soul.";

                Xnpc[8] = gunn; XinitDelay[8] = 0; XendDelay[8] = 3f; XpostDelay[8] = .75f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "Then you should know I don't relish conversation.";

                Xnpc[9] = fran; XinitDelay[9] = 0; XendDelay[9] = 3f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "I'd rather be tortured...";

                Xnpc[10] = rob; XinitDelay[10] = 0; XendDelay[10] = 3.5f; XpostDelay[10] = 1f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "How's one go about torturing a skeleton?";

                Xnpc[11] = null; XinitDelay[11] = 0; XendDelay[11] = 2.5f; XpostDelay[11] = 2f; XisPlayer[11] = true; Xtrigger[11] = false;
                Xline[11] = "...No idea.";

                //END INTRO
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {
                youDiag.text = "Paladin's have been tracking undead for centuries. You are now a target; they won't hestitate, Paladin armor or not.";
                answering = false;

                answerTag = "A01b3";

                StartCoroutine(poseQuestion(fran, 2.25f, false,
                    "I know my people.",
                    3f, false, 2f));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = null; XinitDelay[0] = 0; XendDelay[0] = 6f; XpostDelay[0] = .25f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "I'm sure you do, but I advise considering how deeply they've abandoned you now. Don't make a naive mistake.";

                Xnpc[1] = fran; XinitDelay[1] = 0; XendDelay[1] = 4f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "You must think me a fool to try to convert me. I don't break, pirate.";

                Xnpc[2] = null; XinitDelay[2] = 0; XendDelay[2] = 4f; XpostDelay[2] = .75f; XisPlayer[2] = true; Xtrigger[1] = false;
                Xline[2] = "Either way, without a phylactery your body will rot within days.";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 3f; XpostDelay[3] = .5f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "I'll find one.";

                Xnpc[4] = null; XinitDelay[4] = 0; XendDelay[4] = 3f; XpostDelay[4] = .25f; XisPlayer[4] = true; Xtrigger[4] = false;
                Xline[4] = "It's not that simple.";

                Xnpc[5] = null; XinitDelay[5] = 0; XendDelay[5] = 6.5f; XpostDelay[5] = .25f; XisPlayer[5] = true; Xtrigger[5] = false;
                Xline[5] = "Objects and souls have histories and those histories need to form balance, else the phylactery will corrupt.";

                Xnpc[6] = null; XinitDelay[6] = 0; XendDelay[6] = 7f; XpostDelay[6] = .5f; XisPlayer[6] = true; Xtrigger[6] = false;
                Xline[6] = "I can see these histories; it's a magical talent. Objects reveal things to me, and so I find phylacteries for our newest recruits.";

                Xnpc[7] = gunn; XinitDelay[7] = 0; XendDelay[7] = 3f; XpostDelay[7] = .5f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "You find harmony. How can we help?";

                Xnpc[8] = null; XinitDelay[8] = 0; XendDelay[8] = 5f; XpostDelay[8] = .5f; XisPlayer[8] = true; Xtrigger[8] = false;
                Xline[8] = "Well...tell me your feelings! The better I know you, the better home I can find for your soul.";

                Xnpc[9] = gunn; XinitDelay[9] = 0; XendDelay[9] = 3f; XpostDelay[9] = .75f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "Then you should know I don't relish conversation.";

                Xnpc[10] = fran; XinitDelay[10] = 0; XendDelay[10] = 3f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "I'd rather be tortured...";

                Xnpc[11] = rob; XinitDelay[11] = 0; XendDelay[11] = 3.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[1] = false;
                Xline[11] = "How's one go about torturing a skeleton?";

                Xnpc[12] = null; XinitDelay[12] = 0; XendDelay[12] = 2.5f; XpostDelay[12] = 2f; XisPlayer[12] = true; Xtrigger[12] = false;
                Xline[12] = "...No idea.";

                //END INTRO
            }

            return;
        }
        ///*********LINE************************************************************************************************///


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

            oneAnswer.text = null;  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = null;
            thrAnswer.text = null;
            youDiag.text = line;
        }

        yield return new WaitForSeconds(endDelay);

        if (!isPlayer)
        {
            speaker.primaryActive = false; speaker.secondaryActive = true;    //darken this most recent text
        }
        else
        {
            playerSpeaking = false;     //fade player text to transparent
        }

        yield return new WaitForSeconds(postDelay);     //pause before darkening this most recent line

        if (trigger)    //if next is the player talking
        {
            answering = true;
        }
        else     //if another NPC is gonna pipe in before player talking
        {
            lineI++;
            StartCoroutine(poseQuestion(Xnpc[lineI], XinitDelay[lineI], XisPlayer[lineI], Xline[lineI], XendDelay[lineI], Xtrigger[lineI], XpostDelay[lineI]));
            Debug.Log("New Line: " + Xnpc[lineI]);
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
