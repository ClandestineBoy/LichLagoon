using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class dialogueScript : MonoBehaviour
{
    GameObject sceneMan;
    SceneManger sm;

    [System.Serializable]
    public class NPC
    {
        public Text currentDialogue, header;
        public Image diagBacker, portrait, mousePrompt;
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
    public Image youBacker, youMousePrompt;
    public string answerTag = " ";  //determines where in conversation we are when player is given a chance to answer
    public string nextTag = " ";

    [Header("Colours")]
    public Color silentPortrait, silentDiag, backerBase;    //activePortraits are just color.white
    public Color tranWhite, tranBlack;
    public float fadeOutSpeed = 0f, fadeInSpeed = 0f;

    [Header("Next Line Data")]
    public NPC[] Xnpc = new NPC [40];
    float[] XinitDelay = new float[25], XendDelay = new float[25], XpostDelay = new float[25];
    bool[] XisPlayer = new bool[25], Xtrigger = new bool[25];
    string[] Xline = new string[25];
    int lineI = -1; int maxI = -2;
    private bool[] XskipAvailable = new bool [25];   public bool _skippable = true;

    [Header("FastForward Tools")]
    public bool ff = false;
    public float ffSpeed = 0f;

    private float[] timer = new float[4];
    public string dialogueID = "";          //potential IDs: "Intro" "Night1" "Night2"

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Start is called before the first frame update
    void Start()
    {
        sceneMan = GameObject.FindGameObjectWithTag("SceneManager");
        sm = sceneMan.GetComponent<SceneManger>();
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
                        3f, true, .5f, true));

            answerTag = "A01";
            nextTag = "A01";
        }

        if (dialogueID == "Night1")
        {
            StartCoroutine(poseQuestion(null, 2f, true,
                        "We'll be camping here until I find each of you a suitable phylactery.",
                        7f, true, .5f, false));

            nextTag = "B01";
            answerTag = "B01";
        }

        if (dialogueID == "Night2")
        {
            StartCoroutine(poseQuestion(null, 2f, true,
                        "TEST",
                        2f, true, .5f, false));

            nextTag = "C01";
            answerTag = "C01";
        }
    }


    // Update is called once per frame
    void Update()
    {
        if(maxI == lineI)
        {
            sm.nextScene = true;
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) && !ff && _skippable)       
        {
            Time.timeScale = ffSpeed * 5;
            //Debug.Log("timescale = " + Time.timeScale);
            ff = true;
            fran.mousePrompt.color = tranWhite;
            gunn.mousePrompt.color = tranWhite;
            rob.mousePrompt.color = tranWhite;
            youMousePrompt.color = tranWhite;
        }
        else if (!ff)                                    
        {
            Time.timeScale = 1f;                
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
                speaker.mousePrompt.color = tranWhite;
                colourShift(speaker.diagBacker, null, backerBase, Time.deltaTime * fadeInSpeed, false);
                colourShift(speaker.portrait, null, silentPortrait, Time.deltaTime * fadeInSpeed, false);
                colourShift(null, speaker.currentDialogue, silentDiag, Time.deltaTime * fadeInSpeed, true);
                colourShift(null, speaker.header, silentDiag, Time.deltaTime * fadeInSpeed, true);

                timer[i] += Time.deltaTime;

                if (timer[i] > 4f)
                {
                    speaker.secondaryActive = false;
                    timer[i] = 0;
                }
            }
            else     //hasn't spoken yet
            {
                speaker.mousePrompt.color = tranWhite;
                colourShift(speaker.diagBacker, null, tranBlack, Time.deltaTime * (fadeOutSpeed + 2), false);
                colourShift(speaker.portrait, null, silentPortrait, Time.deltaTime * (fadeOutSpeed + 2), false);
                colourShift(null, speaker.currentDialogue, tranWhite, Time.deltaTime * (fadeOutSpeed + 2), true);
                colourShift(null, speaker.header, tranWhite, Time.deltaTime * (fadeOutSpeed + 2), true);
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
            if (timer[3] > 4)
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
        ff = false;

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

            oneAnswer.text = "1) Explain again.";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Reassure (Skip Tutorial)";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "Okay, but please: listen this time.";
                answering = false;

                StartCoroutine(poseQuestion(rob, 2f, false,
                   "I'll try me best.",
                   5f, false, .5f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 5f; XpostDelay[0] = .5f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Do more than try. My ears ache.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 5f; XpostDelay[1] = 2f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Good thing you don't have those anymore.";

                Xnpc[2] = null; XinitDelay[2] = 0f; XendDelay[2] = 8f; XpostDelay[2] = .1f; XisPlayer[2] = true; Xtrigger[2] = false;
                Xline[2] = "Now, pay attention--you are all dead; you think, move, breathe, but I assure you, you are dead.";

                Xnpc[3] = null; XinitDelay[3] = 0f; XendDelay[3] = 7.5f; XpostDelay[3] = .5f; XisPlayer[3] = true; Xtrigger[3] = false;
                Xline[3] = "Everything else your corpse can do is all thanks to The Captain; she turned you to Liches, un-dead.";

                Xnpc[4] = rob; XinitDelay[4] = 0f; XendDelay[4] = 5f; XpostDelay[4] = .5f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "That explains a few things.";

                Xnpc[5] = null; XinitDelay[5] = 0f; XendDelay[5] = 9f; XpostDelay[5] = .5f; XisPlayer[5] = true; Xtrigger[5] = false;
                Xline[5] = "All Lichs, if they so please, may join Captain Carrozo's crew, the most formidable pirates this side of the afterlife.";

                Xnpc[6] = fran; XinitDelay[6] = 0; XendDelay[6] = 5f; XpostDelay[6] = 1f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "So we're monsters *and* criminals?";

                Xnpc[7] = null; XinitDelay[7] = 0; XendDelay[7] = 9f; XpostDelay[7] = .25f; XisPlayer[7] = true; Xtrigger[7] = false;
                Xline[7] = "If you like. Either way, you're dead now. You're fugitives. Paladin's don't see us, they see monsters.";

                Xnpc[8] = null; XinitDelay[8] = 0; XendDelay[8] = 8f; XpostDelay[8] = .5f; XisPlayer[8] = true; Xtrigger[8] = false;
                Xline[8] = "So we need to find a way of keeping you alive, whether you join us or not. That's where I lend a hand.";

                Xnpc[9] = gunn; XinitDelay[9] = 0; XendDelay[9] = 5f; XpostDelay[9] = .5f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "You forge armor?";

                Xnpc[10] = null; XinitDelay[10] = 0; XendDelay[10] = 9f; XpostDelay[10] = .25f; XisPlayer[10] = true; Xtrigger[10] = false;
                Xline[10] = "Of a sort. Your corpse is a vessel, animated but decaying. A dead body cannot sustain a living soul.";

                Xnpc[11] = null; XinitDelay[11] = 0; XendDelay[11] = 9.5f; XpostDelay[11] = .25f; XisPlayer[11] = true; Xtrigger[11] = false;
                Xline[11] = "So we take the soul and store it elsewhere: an object to house your spirit, a phylactery. Once the soul is stored, your corpse becomes indestructible...to a point.";

                Xnpc[12] = null; XinitDelay[12] = 0; XendDelay[12] = 7.5f; XpostDelay[12] = 1f; XisPlayer[12] = true; Xtrigger[12] = false;
                Xline[12] = "This is my task; pirate or not, I will find each of you a phylactery.";

                Xnpc[13] = fran; XinitDelay[13] = 0; XendDelay[13] = 5f; XpostDelay[13] = .25f; XisPlayer[13] = false; Xtrigger[13] = true;
                Xline[13] = "--And if we refuse your help?";

                nextTag = "A01b";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "I think you understand.";
                answering = false;  //stops updating this answer section so it doesn't show the next choices

                nextTag = "A01a"; //preemptively updates this answer function to be ready with the choices for the next answer

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";  //zero out dialogue choices

                StartCoroutine(poseQuestion(rob, 1.5f, false,
                    "Well pardon me. It's my first time. I'm nervous.",
                    5f, false, .5f, true));   //comment this out and fill it out - this is the next line read from the next npc



                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 6f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "We are all new to this. Yet here we sit, silent.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 5.5f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "...";

                Xnpc[2] = rob; XinitDelay[2] = 0f; XendDelay[2] = 4.5f; XpostDelay[2] = 2.5f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "Okay, okay...Nevermind.";

                //END INTRO
                maxI = 2; 
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
        }       //A = intro

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

                nextTag = "A01b1";

                StartCoroutine(poseQuestion(fran, 2.5f, false,
                    "And why's that?",
                    4.5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = null; XinitDelay[0] = 0f; XendDelay[0] = 6f; XpostDelay[0] = .25f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "Without a phylactery you will decompose in three days at best.";

                Xnpc[1] = null; XinitDelay[1] = 0f; XendDelay[1] = 7.5f; XpostDelay[1] = .25f; XisPlayer[1] = true; Xtrigger[1] = false;
                Xline[1] = "Even if you look for one alone, I doubt you'd manage to find anything suitable.";

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 5.5f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[1] = false;
                Xline[2] = "I'll do just fine without the help of a pirate.";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 6.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "As will I, no offense. I'm just pretty set on putting my soul into a cannon ball.";

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 4.5f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "I could find an anvil.";

                Xnpc[5] = null; XinitDelay[5] = 0; XendDelay[5] = 7.5f; XpostDelay[5] = 1f; XisPlayer[5] = true; Xtrigger[5] = false;
                Xline[5] = "All fine suggestions, I'm sure. The problem's that you must consider resonance.";

                Xnpc[6] = rob; XinitDelay[6] = 0; XendDelay[6] = 4.5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "..Like music?";

                Xnpc[7] = null; XinitDelay[7] = 0; XendDelay[7] = 8f; XpostDelay[7] = .5f; XisPlayer[7] = true; Xtrigger[7] = false;
                Xline[7] = "Almost. See, a soul has to resonate with its phylactery. You can't just stick it into anything.";

                Xnpc[8] = null; XinitDelay[8] = 0; XendDelay[8] = 8.5f; XpostDelay[8] = .5f; XisPlayer[8] = true; Xtrigger[8] = false;
                Xline[8] = "Objects and souls have histories, and those histories need to form balance, else the phylactery will corrupt.";

                Xnpc[9] = null; XinitDelay[9] = 0; XendDelay[9] = 9f; XpostDelay[9] = 1f; XisPlayer[9] = true; Xtrigger[9] = false;
                Xline[9] = "I can see these histories; it's a magical talent. Objects reveal things to me, and so I find phylacteries for our newest recruits.";

                Xnpc[10] = gunn; XinitDelay[10] = 0; XendDelay[10] = 5f; XpostDelay[10] = .5f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "You find harmony. How can we help?";

                Xnpc[11] = null; XinitDelay[11] = 0; XendDelay[11] = 8f; XpostDelay[11] = 2f; XisPlayer[11] = true; Xtrigger[11] = false;
                Xline[11] = "Well...tell me your feelings! The better I know you, the better home I can find for your soul.";

                Xnpc[12] = gunn; XinitDelay[12] = 0; XendDelay[12] = 5f; XpostDelay[12] = .75f; XisPlayer[12] = false; Xtrigger[12] = false;
                Xline[12] = "Then you should know I don't relish conversation.";

                Xnpc[13] = fran; XinitDelay[13] = 0; XendDelay[13] = 5f; XpostDelay[13] = .25f; XisPlayer[13] = false; Xtrigger[13] = false;
                Xline[13] = "I'd rather be tortured...";

                Xnpc[14] = rob; XinitDelay[14] = 0; XendDelay[14] = 5.5f; XpostDelay[14] = .75f; XisPlayer[14] = false; Xtrigger[14] = false;
                Xline[14] = "How's one go about torturing a skeleton?";

                Xnpc[15] = null; XinitDelay[15] = 0; XendDelay[15] = 4.5f; XpostDelay[15] = .5f; XisPlayer[15] = true; Xtrigger[15] = false;
                Xline[15] = "...No idea.";

                //END INTRO
                maxI = 15;
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "Well, without a phylactery you'll decompose in a week at best. That's assuming the Paladin's don't get to you first.";
                answering = false;

                nextTag = "A01b2";

                StartCoroutine(poseQuestion(fran, 1.5f, false,
                    "I welcome their arrival.",
                    5f, false, 2f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = null; XinitDelay[0] = 0; XendDelay[0] = 8f; XpostDelay[0] = .75f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "They won't spare you. I've seen Paladins slay their own before. You will befall a similar fate without our help.";

                Xnpc[1] = rob; XinitDelay[1] = 0; XendDelay[1] = 6.5f; XpostDelay[1] = 1f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "So why you? I wanna pick what my soul goes into, it's my damn soul.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 5.5f; XpostDelay[2] = .5f; XisPlayer[2] = false; Xtrigger[1] = false;
                Xline[2] = "...Will a soul fit inside a cannon ball?";

                Xnpc[3] = null; XinitDelay[3] = 0; XendDelay[3] = 9f; XpostDelay[3] = .25f; XisPlayer[3] = true; Xtrigger[3] = false;
                Xline[3] = "Well, yes...but that's not exactly the point. You see, a soul has to resonate with its phylactery. You can't just stick it into anything.";

                Xnpc[4] = null; XinitDelay[4] = 0; XendDelay[4] = 7.5f; XpostDelay[4] = .25f; XisPlayer[4] = true; Xtrigger[4] = false;
                Xline[4] = "Objects and souls have histories and those histories need to form balance, else the phylactery will corrupt.";

                Xnpc[5] = null; XinitDelay[5] = 0; XendDelay[5] = 9f; XpostDelay[5] = .5f; XisPlayer[5] = true; Xtrigger[5] = false;
                Xline[5] = "I can see these histories; it's a magical talent. Objects reveal things to me, and so I find phylacteries for our newest recruits.";

                Xnpc[6] = gunn; XinitDelay[6] = 0; XendDelay[6] = 4.5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "You find harmony. How can we help?";

                Xnpc[7] = null; XinitDelay[7] = 0; XendDelay[7] = 7.5f; XpostDelay[7] = .25f; XisPlayer[7] = true; Xtrigger[7] = false;
                Xline[7] = "Well...tell me your feelings! The better I know you, the better home I can find for your soul.";

                Xnpc[8] = gunn; XinitDelay[8] = 0; XendDelay[8] = 5f; XpostDelay[8] = .75f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "Then you should know I don't relish conversation.";

                Xnpc[9] = fran; XinitDelay[9] = 0; XendDelay[9] = 5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "I'd rather be tortured...";

                Xnpc[10] = rob; XinitDelay[10] = 0; XendDelay[10] = 5.5f; XpostDelay[10] = 1f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "How's one go about torturing a skeleton?";

                Xnpc[11] = null; XinitDelay[11] = 0; XendDelay[11] = 4.5f; XpostDelay[11] = 2f; XisPlayer[11] = true; Xtrigger[11] = false;
                Xline[11] = "...No idea.";

                //END INTRO
                maxI = 11;
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {
                youDiag.text = "Paladins have been tracking undead for centuries. You are now a target; they won't hestitate, Paladin armor or not.";
                answering = false;

                answerTag = "A01b3";

                StartCoroutine(poseQuestion(fran, 4f, false,
                    "I know my people.",
                    3f, false, 2f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = null; XinitDelay[0] = 0; XendDelay[0] = 8f; XpostDelay[0] = .25f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "I'm sure you do, but I advise considering how deeply they've abandoned you now. Don't make a naive mistake.";

                Xnpc[1] = fran; XinitDelay[1] = 0; XendDelay[1] = 6f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "You must think me a fool to try to convert me. I don't break, pirate.";

                Xnpc[2] = null; XinitDelay[2] = 0; XendDelay[2] = 6f; XpostDelay[2] = .75f; XisPlayer[2] = true; Xtrigger[1] = false;
                Xline[2] = "Either way, without a phylactery your body will rot within days.";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 5f; XpostDelay[3] = .5f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "I'll find one.";

                Xnpc[4] = null; XinitDelay[4] = 0; XendDelay[4] = 5f; XpostDelay[4] = .25f; XisPlayer[4] = true; Xtrigger[4] = false;
                Xline[4] = "It's not that simple.";

                Xnpc[5] = null; XinitDelay[5] = 0; XendDelay[5] = 8.5f; XpostDelay[5] = .25f; XisPlayer[5] = true; Xtrigger[5] = false;
                Xline[5] = "Objects and souls have histories and those histories need to form balance, else the phylactery will corrupt.";

                Xnpc[6] = null; XinitDelay[6] = 0; XendDelay[6] = 9f; XpostDelay[6] = .5f; XisPlayer[6] = true; Xtrigger[6] = false;
                Xline[6] = "I can see these histories; it's a magical talent. Objects reveal things to me, and so I find phylacteries for our newest recruits.";

                Xnpc[7] = gunn; XinitDelay[7] = 0; XendDelay[7] = 5f; XpostDelay[7] = .5f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "You find harmony. How can we help?";

                Xnpc[8] = null; XinitDelay[8] = 0; XendDelay[8] = 7f; XpostDelay[8] = .5f; XisPlayer[8] = true; Xtrigger[8] = false;
                Xline[8] = "Well...tell me your feelings! The better I know you, the better home I can find for your soul.";

                Xnpc[9] = gunn; XinitDelay[9] = 0; XendDelay[9] = 5f; XpostDelay[9] = .75f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "Then you should know I don't relish conversation.";

                Xnpc[10] = fran; XinitDelay[10] = 0; XendDelay[10] = 5f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "I'd rather be tortured...";

                Xnpc[11] = rob; XinitDelay[11] = 0; XendDelay[11] = 5.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[1] = false;
                Xline[11] = "How's one go about torturing a skeleton?";

                Xnpc[12] = null; XinitDelay[12] = 0; XendDelay[12] = 4.5f; XpostDelay[12] = 2f; XisPlayer[12] = true; Xtrigger[12] = false;
                Xline[12] = "...No idea.";

                //END INTRO
                maxI = 12;
            }
            return;
        }

        if (answerTag == "B01")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Explain";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Apologize";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "There's no point setting up the entire crew ashore--";
                answering = false;

                StartCoroutine(poseQuestion(null, 2.5f, true,
                    "--We'll have to leave as fast as we can when the Paladins come.",
                    6f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "This should...work, I suppose.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 5.5f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Can still enjoy fresh air. Stars are out!";

                Xnpc[2] = gunn; XinitDelay[2] = 0; XendDelay[2] = 5f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[1] = false;
                Xline[2] = "They look different here. The stars.";

                Xnpc[3] = gunn; XinitDelay[3] = 0; XendDelay[3] = 4.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Where are we?";

                Xnpc[4] = null; XinitDelay[4] = 0; XendDelay[4] = 7f; XpostDelay[4] = .25f; XisPlayer[4] = true; Xtrigger[4] = false;
                Xline[4] = "Saint Dominque abouts. We...came here a couple weeks ago.";

                Xnpc[5] = rob; XinitDelay[5] = 0; XendDelay[5] = 7f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Okay. Why didn't we get phylacteries where we died though?";

                Xnpc[6] = gunn; XinitDelay[6] = 0; XendDelay[6] = 5f; XpostDelay[6] = 1f; XisPlayer[6] = false; Xtrigger[6] = true;
                Xline[6] = "Did you kill people here?";     XskipAvailable[6] = false;

                nextTag = "B02";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "I know it's not much...but setting up any further would be irresponsible--";
                answering = false;

                StartCoroutine(poseQuestion(null, 1.5f, true,
                    "The Paladins could get here any day, remember.",
                    5f, false, 2f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "This should...work, I suppose.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 5.5f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Can still enjoy fresh air. Stars are out!";

                Xnpc[2] = gunn; XinitDelay[2] = 0; XendDelay[2] = 5f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[1] = false;
                Xline[2] = "They look different here. The stars.";

                Xnpc[3] = gunn; XinitDelay[3] = 0; XendDelay[3] = 4.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Where are we?";

                Xnpc[4] = null; XinitDelay[4] = 0; XendDelay[4] = 7f; XpostDelay[4] = .25f; XisPlayer[4] = true; Xtrigger[4] = false;
                Xline[4] = "Saint Dominque abouts. We...came here a couple weeks ago.";

                Xnpc[5] = rob; XinitDelay[5] = 0; XendDelay[5] = 7f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Okay. Why didn't we get phylacteries where we died though?";

                Xnpc[6] = gunn; XinitDelay[6] = 0; XendDelay[6] = 5f; XpostDelay[6] = 1f; XisPlayer[6] = false; Xtrigger[6] = true;
                Xline[6] = "Did you kill people here?";     XskipAvailable[6] = false;

                nextTag = "B02";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }       //B = night1

        if (answerTag == "B02")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Answer Robin";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Answer Gunnlaug";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "...It took time to bring you back, Robin.";
                answering = false;

                nextTag = "B03";

                StartCoroutine(poseQuestion(null, 2.5f, true,
                    "This was the first place we could stop safely.",
                    4f, false, 2f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 6f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = true;
                Xline[0] = "Think you'll find something weird in the wreckage?";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "We do what we must.";
                answering = false;

                nextTag = "B03";

                StartCoroutine(poseQuestion(null, 3f, true,
                    "This is the only safe place north of Cuba right now.",
                    3.5f, false, .5f, false));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 1.5f; XendDelay[0] = 2.5f; XpostDelay[0] = 2f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Okay.";    XskipAvailable[0] = false;

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 7f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = true;
                Xline[1] = "...Think you'll find something weird in the wreckage?";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "B03")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Confirm";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Warn";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "I always do.";
                answering = false;

                nextTag = "B04";

                StartCoroutine(poseQuestion(fran, 1.5f, false,
                    "...I've decided to stay.",
                    5f, false, 0f, false));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = fran; XinitDelay[0] = 0f; XendDelay[0] = 5f; XpostDelay[0] = 1.25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "...for my phylactery.";     XskipAvailable[0] = false;

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 3.5f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Alright then!";

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 7f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[1] = false;
                Xline[2] = "I'll treat this as pitiful repayment for what you've done.";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 7.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Don't think I'll harbor any obligations to your...crew.";

                Xnpc[4] = fran; XinitDelay[4] = 0; XendDelay[4] = 4.5f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = true;
                Xline[4] = "Are we clear, pirate?";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "Phylacteries are chosen for their history, not their appearance.";
                answering = false;

                nextTag = "B04";

                StartCoroutine(poseQuestion(null, 4f, true,
                    "You might not like what I find.",
                    5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 4.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Huh. Guess I'll keep an open mind, then?"; XskipAvailable[0] = false;

                Xnpc[1] = fran; XinitDelay[1] = 1f; XendDelay[1] = 4f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "...I've decided to stay."; XskipAvailable[1] = false;

                Xnpc[2] = fran; XinitDelay[2] = 0f; XendDelay[2] = 3.5f; XpostDelay[2] = .5f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...for my phylactery."; XskipAvailable[2] = false;

                Xnpc[3] = rob; XinitDelay[3] = 0f; XendDelay[3] = 3.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Alright then!";

                Xnpc[4] = fran; XinitDelay[4] = 0; XendDelay[4] = 7f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "I'll treat this as pitiful repayment for what you've done.";

                Xnpc[5] = fran; XinitDelay[5] = 0; XendDelay[5] = 7.5f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Don't think I'll harbor any obligations to your...crew.";

                Xnpc[6] = fran; XinitDelay[6] = 0; XendDelay[6] = 4.5f; XpostDelay[6] = .25f; XisPlayer[6] = false; Xtrigger[6] = true;
                Xline[6] = "Are we clear, pirate?";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "B04")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Cooperate";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Retaliate";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "Crystal clear. Glad you changed your mind.";
                answering = false;

                nextTag = "B05";

                StartCoroutine(poseQuestion(fran, 2f, false,
                    "...Thank you.",
                    4f, false, 0f, false));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 3.5f; XpostDelay[0] = .5f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "What now?";

                Xnpc[1] = null; XinitDelay[1] = 0f; XendDelay[1] = 3.5f; XpostDelay[1] = .25f; XisPlayer[1] = true; Xtrigger[1] = false;
                Xline[1] = "We begin. Before I go searching for your phylacteries.";
                    
                Xnpc[2] = null; XinitDelay[2] = 0; XendDelay[2] = 7f; XpostDelay[2] = .25f; XisPlayer[2] = true; Xtrigger[1] = false;
                Xline[2] = "First, I'm going to ask you questions. All I ask in return is honesty.";

                Xnpc[3] = null; XinitDelay[3] = 0; XendDelay[3] = 5f; XpostDelay[3] = .25f; XisPlayer[3] = true; Xtrigger[3] = true;
                Xline[3] = "Let's begin: Tell me about your...."; XskipAvailable[3] = false;
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "Captain Carrozo saved your life--a courtesy the Paladins have never returned.";
                answering = false;

                nextTag = "B05";

                StartCoroutine(poseQuestion(fran, 4.5f, false,
                    "If not for your raids I would never've needed saving.",
                    6f, false, 0f, false));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 2f; XendDelay[0] = 4f; XpostDelay[0] = 1.25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "..."; XskipAvailable[0] = false;

                Xnpc[1] = gunn; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "...What now?";

                Xnpc[2] = null; XinitDelay[2] = 0f; XendDelay[2] = 3.5f; XpostDelay[2] = .25f; XisPlayer[2] = true; Xtrigger[2] = false;
                Xline[2] = "We begin. Before I go searching for your phylacteries.";

                Xnpc[3] = null; XinitDelay[3] = 0; XendDelay[3] = 7f; XpostDelay[3] = .25f; XisPlayer[3] = true; Xtrigger[3] = false;
                Xline[3] = "First, I'm going to ask you questions. All I ask in return is honesty.";

                Xnpc[4] = null; XinitDelay[4] = 0; XendDelay[4] = 5f; XpostDelay[4] = .25f; XisPlayer[4] = true; Xtrigger[4] = true;
                Xline[4] = "Let's begin: Tell me about your...."; XskipAvailable[4] = false;
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "B05")
        {
            youDiag.text = "Tell me about your...";

            oneAnswer.text = "1) Childhood";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Family";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "I want to try and get an understanding of where you all came from.";
                answering = false;

                nextTag = "CHILDHOOD1";

                StartCoroutine(poseQuestion(null, 3.5f, true,
                    "So tell me about your childhood. What do you remember?",
                    6.5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 2.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "...";

                Xnpc[1] = gunn; XinitDelay[1] = 0f; XendDelay[1] = 2.5f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "...";

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 2.5f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[1] = false;
                Xline[2] = "...";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 4.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Guess I'll start?";
                //
                Xnpc[4] = gunn; XinitDelay[4] = 0f; XendDelay[4] = 4f; XpostDelay[4] = .5f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "Please do.";

                Xnpc[5] = rob; XinitDelay[5] = 0f; XendDelay[5] = 7f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Growing up was...interesting. I suppose it is for everyone.";

                Xnpc[6] = rob; XinitDelay[6] = 0; XendDelay[6] = 6f; XpostDelay[6] = 1f; XisPlayer[6] = false; Xtrigger[6] = true;
                Xline[6] = "But when I was about six, things went awry.";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "I want to try and get an understanding of where you all came from.";
                answering = false;

                nextTag = "FAMILY1";

                StartCoroutine(poseQuestion(null, 3.5f, true,
                    "Could you tell me about your family? What were they like?",
                    6.5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 5.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Oh, my family's huge. Was, maybe.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 5.5f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "...They were my world--didn't have much else.";

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 4.5f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[1] = false;
                Xline[2] = "Is that not enough for you?";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 6f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Ha! More than enough. They're a bloody handful.";
                //
                Xnpc[4] = rob; XinitDelay[4] = 0f; XendDelay[4] = 4.5f; XpostDelay[4] = .5f; XisPlayer[4] = false; Xtrigger[4] = true;
                Xline[4] = "Just don't like being cooped up is all.";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "CHILDHOOD1")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Investigate";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Guess";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "Awry how? What happened?";
                answering = false;

                nextTag = "CHILDHOOD2";

                StartCoroutine(poseQuestion(rob, 2f, false,
                    "I got sick. Real bad...",
                    5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 6f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Legs gave up the ghost. They stopped working ever since.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 7.5f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "I guess my home's always been my whole world. Just have my family.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...Had.";   XskipAvailable[2] = false;

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 4.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Anyway...I'm a free spirit now. Finally.";
                
                Xnpc[4] = rob; XinitDelay[4] = 0f; XendDelay[4] = 7f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "Got your crew, working legs: things are looking up!";

                Xnpc[5] = rob; XinitDelay[5] = 0f; XendDelay[5] = 5.5f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "I think I've almost got jumping down.";

                Xnpc[6] = fran; XinitDelay[6] = 0; XendDelay[6] = 6f; XpostDelay[6] = 1f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "I was taught not to revel in Faustian gifts.";

                Xnpc[7] = rob; XinitDelay[7] = 0; XendDelay[7] = 7.5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "It's a hell of a lot more than being pious ever got me.";

                Xnpc[8] = gunn; XinitDelay[8] = 0; XendDelay[8] = 7f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "They have a point. Being dead isn't all bad.";

                Xnpc[9] = fran; XinitDelay[9] = 0; XendDelay[9] = 8.5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = true;
                Xline[9] = "You're both abhorent. I was taught that good requires persistence not magic.";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "You lost someone?";
                answering = false;

                nextTag = "CHILDHOOD2";

                StartCoroutine(poseQuestion(rob, 1.5f, false,
                    "Good try--not quite, though...",
                    5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 6f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "When I was six my legs gave up the ghost. They stopped working ever since.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 7.5f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "I guess my home's always been my whole world. Just have my family.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...Had."; XskipAvailable[2] = false;

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 4.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Anyway...I'm a free spirit now. Finally.";

                Xnpc[4] = rob; XinitDelay[4] = 0f; XendDelay[4] = 7f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "Got your crew, working legs: things are looking up!";

                Xnpc[5] = rob; XinitDelay[5] = 0f; XendDelay[5] = 5.5f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "I think I've almost got jumping down.";

                Xnpc[6] = fran; XinitDelay[6] = 0; XendDelay[6] = 6f; XpostDelay[6] = 1f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "I was taught not to revel in Faustian gifts.";

                Xnpc[7] = rob; XinitDelay[7] = 0; XendDelay[7] = 7.5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "It's a hell of a lot more than being pious ever got me.";

                Xnpc[8] = gunn; XinitDelay[8] = 0; XendDelay[8] = 7f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "They have a point. Being dead isn't all bad.";

                Xnpc[9] = fran; XinitDelay[9] = 0; XendDelay[9] = 8.5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = true;
                Xline[9] = "You're both abhorent. I was taught that good requires persistence not magic.";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "CHILDHOOD2")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Agree";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Qualify";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "I respect that, Francesca, but some things can't be gained from hard work alone.";
                answering = false;

                nextTag = "CHILDHOOD3";

                StartCoroutine(poseQuestion(fran, 5f, false,
                    "...",
                    3f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = fran; XinitDelay[0] = 0f; XendDelay[0] = 6.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "My mother and father were Paladins before me; they understood.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 7.5f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "They hunted people like you. I always admired them.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...They hunted people like us."; XskipAvailable[2] = false;

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 8f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = true;
                Xline[3] = "I won't be this way forever. Once I'm human again I'll rejoin my comrades in arms.";

            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "Magic gives us choices.";
                answering = false;

                nextTag = "CHILDHOOD3";

                StartCoroutine(poseQuestion(fran, 1f, false,
                    "Magic gives us false hope.",
                    5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = fran; XinitDelay[0] = 0f; XendDelay[0] = 6.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "My family were Paladins before me; they understood.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 7.5f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "They hunted people like you. I always admired them.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...They hunted people like us."; XskipAvailable[2] = false;

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 8f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = true;
                Xline[3] = "I won't be this way forever. Once I'm human again I'll rejoin my comrades in arms.";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "CHILDHOOD3")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Tell the Truth";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = null;
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "No. Unfortunately you won't.";
                answering = false;

                nextTag = "CHILDHOODPROMPTS";

                StartCoroutine(poseQuestion(fran, 2f, false,
                    "I beg your pardon?",
                    2f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = null; XinitDelay[0] = 0f; XendDelay[0] = 6.5f; XpostDelay[0] = .25f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "There is no returning to humanity. Many have tried; all failed.";

                Xnpc[1] = null; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .5f; XisPlayer[1] = true; Xtrigger[1] = false;
                Xline[1] = "I'm sorry.";

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 2.5f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 6f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "...Well what about the big fella? What's your story?";

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 3f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "My story is brief:";

                Xnpc[5] = gunn; XinitDelay[5] = 0; XendDelay[5] = 7.5f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "My parents always thought I'd make a fine smith. I did.";

                Xnpc[6] = rob; XinitDelay[6] = 0; XendDelay[6] = 5.5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "That's it? There must be something more.";

                Xnpc[7] = gunn; XinitDelay[7] = 0; XendDelay[7] = 5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "My father baked exquisite bread.";

                Xnpc[8] = rob; XinitDelay[8] = 0; XendDelay[8] = 2f; XpostDelay[8] = .75f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "...Sweet.";

                Xnpc[9] = gunn; XinitDelay[9] = 0; XendDelay[9] = 3.5f; XpostDelay[9] = .75f; XisPlayer[9] = false; Xtrigger[9] = true;
                Xline[9] = "He made sourdough, actually.";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "CHILDHOODPROMPTS")
        {
            youDiag.text = "Who do you want to question further? (Pick One)";

            oneAnswer.text = "1) Francesca";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Gunnlaug";
            thrAnswer.text = "3) Robin";

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "Francesca: you said you joined the Paladin armada for family...";
                answering = false;

                nextTag = "CHILDHOODPROMPTS1";

                StartCoroutine(poseQuestion(null, 4f, true,
                    "Was there no other reason?",
                    4f, false, .5f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";
                
                Xnpc[0] = fran; XinitDelay[0] = 0f; XendDelay[0] = 7.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Do I need some trivial motivation to hunt murderers?";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "You murder people too...";

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "We cleanse them.";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 5.5f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "I'd rather you didn't 'cleanse' me, thanks.";

                Xnpc[4] = fran; XinitDelay[4] = 0; XendDelay[4] = 7.5f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "Do you understand what you've become? What you've become?";

                Xnpc[5] = fran; XinitDelay[5] = 0; XendDelay[5] = 9.5f; XpostDelay[5] = 1f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Lichs are leeches: they cannot survive without sucking the life out of world around them.";

                Xnpc[6] = null; XinitDelay[6] = 0; XendDelay[6] = 8.5f; XpostDelay[6] = .5f; XisPlayer[6] = true; Xtrigger[6] = false;
                Xline[6] = "You force us to fight. We try to give back all the lives we can.";

                Xnpc[7] = fran; XinitDelay[7] = 0; XendDelay[7] = 6.5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "And how many lives did you save on your last raid?";

                Xnpc[8] = fran; XinitDelay[8] = 0; XendDelay[8] = 4f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "Three. You saved three lives.";

                Xnpc[9] = fran; XinitDelay[9] = 0; XendDelay[9] = 5.5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "Were we the only corpses you fiends left intact?";

                Xnpc[10] = rob; XinitDelay[10] = 0; XendDelay[10] = 3.5f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "They're helping us!";

                Xnpc[11] = fran; XinitDelay[11] = 0; XendDelay[11] = 3.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[11] = false;
                Xline[11] = "THEY ARE THIEVES! PIRATES!";   XskipAvailable[11] = false;

                Xnpc[12] = fran; XinitDelay[12] = 0; XendDelay[12] = 7.5f; XpostDelay[12] = .25f; XisPlayer[12] = false; Xtrigger[12] = false;
                Xline[12] = "Only difference is they're stealing lives! The only thing that can't be replaced..."; XskipAvailable[12] = false;

                Xnpc[13] = fran; XinitDelay[13] = 0; XendDelay[13] = 4.5f; XpostDelay[13] = .25f; XisPlayer[13] = false; Xtrigger[13] = false;
                Xline[13] = "Daughters, fathers, friends."; XskipAvailable[13] = false;

                Xnpc[14] = fran; XinitDelay[14] = 0; XendDelay[14] = 8f; XpostDelay[14] = .25f; XisPlayer[14] = false; Xtrigger[14] = false;
                Xline[14] = "Do not pretend that by indoctrinating the scraps they have somehow absolved themselves."; XskipAvailable[14] = false;

                Xnpc[15] = rob; XinitDelay[15] = 0; XendDelay[15] = 2.5f; XpostDelay[15] = .25f; XisPlayer[15] = false; Xtrigger[15] = false;
                Xline[15] = "..."; XskipAvailable[15] = false;

                Xnpc[16] = gunn; XinitDelay[16] = 0; XendDelay[16] = 7f; XpostDelay[16] = .25f; XisPlayer[16] = false; Xtrigger[16] = true;
                Xline[16] = "They survive, as we all do. Like it or not, you are also a leech."; XskipAvailable[16] = true;
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "Gunnlaug: you mentioned your parents. What were they like?";
                answering = false;

                StartCoroutine(poseQuestion(gunn, 2.5f, false,
                    "Kind, patient, steadyhands: the expected qualities of a decent parent.",
                    4.5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = null; XinitDelay[0] = 2f; XendDelay[0] = 4f; XpostDelay[0] = .5f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "I see. What did they do?";

                Xnpc[1] = gunn; XinitDelay[1] = 0f; XendDelay[1] = 4.5f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "My father was a baker. My mother worked iron.";

                Xnpc[2] = gunn; XinitDelay[2] = 0f; XendDelay[2] = 6f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "I learnt from them both, honing my creativity and my craft.";

                Xnpc[3] = gunn; XinitDelay[3] = 0; XendDelay[3] = 5.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "I think they taught me about the beauty in my work..."; XskipAvailable[3] = false;

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "Maybe in death I can be greater..."; XskipAvailable[4] = false;

                Xnpc[5] = fran; XinitDelay[5] = 0; XendDelay[5] = 4.5f; XpostDelay[5] = 1f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "You must have made them very proud.";

                Xnpc[6] = gunn; XinitDelay[6] = 0; XendDelay[6] = 5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "Perhaps. I left home young. Sailed out.";

                Xnpc[7] = gunn; XinitDelay[7] = 0; XendDelay[7] = 3.5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "That was that.";

                Xnpc[8] = rob; XinitDelay[8] = 0; XendDelay[8] = 2f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "WHAT?!";

                Xnpc[9] = rob; XinitDelay[9] = 0; XendDelay[9] = 3.5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "Don't you visit??";

                Xnpc[10] = gunn; XinitDelay[10] = 0; XendDelay[10] = 4.5f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "No. The journey is arduous.";

                Xnpc[11] = gunn; XinitDelay[11] = 0; XendDelay[11] = 7.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[11] = false;
                Xline[11] = "They visited once, for the summer. My partner became quite fond of them.";

                Xnpc[12] = rob; XinitDelay[12] = 0; XendDelay[12] = 3.5f; XpostDelay[12] = .25f; XisPlayer[12] = false; Xtrigger[12] = false;
                Xline[12] = "You must miss them.";

                Xnpc[13] = gunn; XinitDelay[13] = 0; XendDelay[13] = 5.5f; XpostDelay[13] = .25f; XisPlayer[13] = false; Xtrigger[13] = false;
                Xline[13] = "Not especially. They're busy people, as am I.";

                Xnpc[14] = gunn; XinitDelay[14] = 0; XendDelay[14] = 5f; XpostDelay[14] = .25f; XisPlayer[14] = false; Xtrigger[14] = false;
                Xline[14] = "Our love withstands time and distance.";

                Xnpc[15] = gunn; XinitDelay[15] = 0; XendDelay[15] = 4.5f; XpostDelay[15] = .25f; XisPlayer[15] = false; Xtrigger[15] = false;
                Xline[15] = "We see no need to reaffirm that.";

                Xnpc[16] = rob; XinitDelay[16] = 0; XendDelay[16] = 5f; XpostDelay[16] = .25f; XisPlayer[16] = false; Xtrigger[16] = false;
                Xline[16] = "Sounds unhealthy...";

                Xnpc[17] = fran; XinitDelay[17] = 0; XendDelay[17] = 2.5f; XpostDelay[17] = .25f; XisPlayer[17] = false; Xtrigger[17] = false;
                Xline[17] = "Why?";

                Xnpc[18] = rob; XinitDelay[18] = 0; XendDelay[18] = 3.5f; XpostDelay[18] = .25f; XisPlayer[18] = false; Xtrigger[18] = false;
                Xline[18] = "...";

                Xnpc[19] = null; XinitDelay[19] = 0; XendDelay[19] = 3.5f; XpostDelay[19] = .25f; XisPlayer[19] = true; Xtrigger[19] = false;
                Xline[19] = "I think that'll do for tonight. Thank you.";

                Xnpc[20] = null; XinitDelay[20] = 0; XendDelay[20] = 3.5f; XpostDelay[20] = .25f; XisPlayer[20] = true; Xtrigger[20] = false;
                Xline[20] = "I'll begin searching for your phylacteries tomorrow morning.";

                //end
                maxI = 20;
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {
                youDiag.text = "Robin: could you say more about your family?";
                answering = false;

                nextTag = "CHILDHOOD2";

                StartCoroutine(poseQuestion(rob, 2.5f, false,
                    "For sure! Six of the buggers: Davey, Stace, Marge, Henry, an' Alfie.",
                    5.5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = .5f; XendDelay[0] = 3f; XpostDelay[0] = .5f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Sounds...loud.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 4.5f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Bloody was! Not Davey, but Alfie, the youngest--";

                Xnpc[2] = rob; XinitDelay[2] = 0f; XendDelay[2] = 5.5f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "Kid could talk your ear off with a mouth full of Mahi!";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 3.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "You were close?";

                Xnpc[4] = rob; XinitDelay[4] = 0; XendDelay[4] = 7f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "'Course we were! Didn't see much of the boys during the day though--";

                Xnpc[5] = rob; XinitDelay[5] = 0; XendDelay[5] = 5f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "--out fishing or carryin' heavy shit.";

                Xnpc[6] = rob; XinitDelay[6] = 0; XendDelay[6] = 5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "Marge though, she kept around the house with Ma.";

                Xnpc[7] = rob; XinitDelay[7] = 0; XendDelay[7] = 4.5f; XpostDelay[7] = 1f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "Partners in crime, we were...";

                Xnpc[8] = fran; XinitDelay[8] = 0; XendDelay[8] = 3.5f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "You must miss them deeply.";

                Xnpc[9] = rob; XinitDelay[9] = 0; XendDelay[9] = 3.5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "Yeah...a lot.";

                Xnpc[10] = rob; XinitDelay[10] = 0; XendDelay[10] = 6.5f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "Devils, the lot of them. But they had good souls. Good company."; XskipAvailable[10] = false;

                Xnpc[11] = fran; XinitDelay[11] = 0; XendDelay[11] = 4.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[11] = false;
                Xline[11] = "Shame the Lichs stole that from you."; XskipAvailable[11] = false;

                Xnpc[12] = rob; XinitDelay[12] = 0; XendDelay[12] = 6.5f; XpostDelay[12] = .25f; XisPlayer[12] = false; Xtrigger[12] = false;
                Xline[12] = "...True, very true. But I got my legs back and some added bonuses."; XskipAvailable[12] = false;

                Xnpc[13] = rob; XinitDelay[13] = 0; XendDelay[13] = 3f; XpostDelay[13] = .5f; XisPlayer[13] = false; Xtrigger[13] = false;
                Xline[13] = "So it's not all bad..."; XskipAvailable[13] = false;

                Xnpc[14] = rob; XinitDelay[14] = 0; XendDelay[14] = 3.5f; XpostDelay[14] = 1f; XisPlayer[14] = false; Xtrigger[14] = false;
                Xline[14] = "...Never all bad."; XskipAvailable[14] = false;

                Xnpc[15] = fran; XinitDelay[15] = 0; XendDelay[15] = 4.5f; XpostDelay[15] = .5f; XisPlayer[15] = false; Xtrigger[15] = false;
                Xline[15] = "...So it was worth it, then?"; XskipAvailable[15] = false;

                Xnpc[16] = rob; XinitDelay[16] = 0; XendDelay[16] = 2.5f; XpostDelay[16] = .25f; XisPlayer[16] = false; Xtrigger[16] = false;
                Xline[16] = "..."; XskipAvailable[16] = false;

                Xnpc[17] = null; XinitDelay[17] = 0; XendDelay[17] = 3.5f; XpostDelay[17] = .25f; XisPlayer[17] = true; Xtrigger[17] = false;
                Xline[17] = "I think that'll do for tonight. Thank you.";

                Xnpc[18] = null; XinitDelay[18] = 0; XendDelay[18] = 3.5f; XpostDelay[18] = .25f; XisPlayer[18] = true; Xtrigger[18] = false;
                Xline[18] = "I'll begin searching for your phylacteries tomorrow morning.";

                //end
                maxI = 18;
            }
        }

        if (answerTag == "CHILDHOODPROMPTS1")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Agree with Gunn";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Be Diplomatic";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "Gunnlaug is right. Refusing our help is an unwise choice.";
                answering = false;

                nextTag = "CHILDHOODPROMPTS2";

                StartCoroutine(poseQuestion(fran, 4.5f, true,
                    "...",
                    3f, false, .5f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 4.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "...I'm sorry she spouted all that.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Don't speak for me.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 6.5f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "I'm going to bed, Pirate. Find my phylactery quickly.";

                //END NIGHT
                maxI = 3;
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                youDiag.text = "You don't have to like us, but if you wish to survive, please cooperate.";
                answering = false;

                nextTag = "CHILDHOODPROMPTS2";

                StartCoroutine(poseQuestion(fran, 4.5f, true,
                    "...",
                    3f, false, .5f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 4.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "...I'm sorry she spouted all that.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Don't speak for me.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 6.5f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "I'm going to bed, Pirate. Find my phylactery quickly.";

                //END NIGHT
                maxI = 3;
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "FAMILY1")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Investigate";
            twoAnswer.text = "2) Stay Quiet";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                youDiag.text = "Did you consider leaving?";
                answering = false;

                nextTag = "FAMILY2";

                StartCoroutine(poseQuestion(rob, 2f, false,
                    "Never a consideration, mate. Did you miss the crutches next to my dead corpse?",
                    5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 2f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Oh.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 4.5f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Surprised I'm--I was disabled?";

                Xnpc[2] = gunn; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "No. I forgot we're dead."; XskipAvailable[2] = false;

                Xnpc[3] = rob; XinitDelay[3] = .75f; XendDelay[3] = 4.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Anyway...I'm a free spirit now. Finally.";

                Xnpc[4] = rob; XinitDelay[4] = 0f; XendDelay[4] = 7f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "Got your crew, working legs: things are looking up!";

                Xnpc[5] = rob; XinitDelay[5] = 0f; XendDelay[5] = 5.5f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "I think I've almost got jumping down.";

                Xnpc[6] = fran; XinitDelay[6] = 0; XendDelay[6] = 6f; XpostDelay[6] = 1f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "I was taught not to revel in Faustian gifts.";

                Xnpc[7] = rob; XinitDelay[7] = 0; XendDelay[7] = 7.5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "It's a hell of a lot more than being pious ever got me.";

                Xnpc[8] = gunn; XinitDelay[8] = 0; XendDelay[8] = 7f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "They have a point. Being dead isn't all bad.";

                Xnpc[9] = fran; XinitDelay[9] = 0; XendDelay[9] = 8.5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = true;
                Xline[9] = "You're both abhorent. I was taught that good requires persistence not magic.";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "...";
                answering = false;

                nextTag = "CHILDHOOD2";

                StartCoroutine(poseQuestion(fran, 1.5f, false,
                    "Cooped up? You fulfilled a duty to your family.",
                    5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 2.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Ha! Hardly!";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Then how could you possibly complai--";

                Xnpc[2] = gunn; XinitDelay[2] = 0; XendDelay[2] = 3.5f; XpostDelay[2] = .5f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "We should let Robin finish.";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 3.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Thanks, Gunner. Look, Fran--";

                Xnpc[4] = fran; XinitDelay[4] = 0f; XendDelay[4] = 1.5f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "Francesca.";

                Xnpc[5] = rob; XinitDelay[5] = 0f; XendDelay[5] = 8.5f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Listen: I fell ill around age six, legs gave up the ghost and never got going again.";

                Xnpc[6] = rob; XinitDelay[6] = 0; XendDelay[6] = 6f; XpostDelay[6] = 1f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "There was no serving and no duties.";

                Xnpc[7] = rob; XinitDelay[7] = 0; XendDelay[7] = 7f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "I was basically a turnip but less healthy. They looked after me.";

                Xnpc[8] = fran; XinitDelay[8] = 0; XendDelay[8] = 3f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "Oh...I'm sorry."; XskipAvailable[8] = false;

                Xnpc[9] = rob; XinitDelay[9] = .75f; XendDelay[9] = 2.5f; XpostDelay[9] = .75f; XisPlayer[9] = false; Xtrigger[9] = true;
                Xline[9] = "Sure."; XskipAvailable[9] = false;

                Xnpc[10] = rob; XinitDelay[10] = 0; XendDelay[10] = 5.5f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "Anyway...I'm a free spirit now. Finally.";

                Xnpc[11] = rob; XinitDelay[11] = 0f; XendDelay[11] = 7f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[11] = false;
                Xline[11] = "Got your crew, working legs: things are looking up!";

                Xnpc[12] = rob; XinitDelay[12] = 0f; XendDelay[12] = 5.5f; XpostDelay[12] = .25f; XisPlayer[12] = false; Xtrigger[12] = false;
                Xline[12] = "I think I've almost got jumping down.";

                Xnpc[13] = fran; XinitDelay[13] = 0; XendDelay[13] = 6f; XpostDelay[13] = 1f; XisPlayer[13] = false; Xtrigger[13] = false;
                Xline[13] = "...Don't you worry about how they brought us back?";

                Xnpc[14] = rob; XinitDelay[14] = 0; XendDelay[14] = 7f; XpostDelay[14] = .25f; XisPlayer[14] = false; Xtrigger[14] = false;
                Xline[14] = "Corrozo's given me more than piety ever did.";

                Xnpc[15] = gunn; XinitDelay[15] = 0; XendDelay[15] = 7f; XpostDelay[15] = .25f; XisPlayer[15] = false; Xtrigger[15] = false;
                Xline[15] = "They have a point. Being dead isn't all bad.";

                Xnpc[16] = fran; XinitDelay[16] = 0; XendDelay[16] = 8.5f; XpostDelay[16] = .25f; XisPlayer[16] = false; Xtrigger[16] = true;
                Xline[16] = "I don't know. I was taught that good requires persistence, not magic.";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "FAMILY2")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Agree";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Qualify";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "I respect that, Francesca, but some things can't be gained from hard work alone.";
                answering = false;

                nextTag = "FAMILY3";

                StartCoroutine(poseQuestion(fran, 5f, false,
                    "...",
                    3f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = fran; XinitDelay[0] = 0f; XendDelay[0] = 6.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "My mother and father were Paladins before me; they understood.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 7.5f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "They hunted people like you. I always admired them.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...They hunted people like us."; XskipAvailable[2] = false;

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 8f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = true;
                Xline[3] = "I won't be this way forever. Once I'm human again I'll rejoin my comrades in arms.";

            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "Magic gives us choices.";
                answering = false;

                nextTag = "FAMILY3";

                StartCoroutine(poseQuestion(fran, 1f, false,
                    "Magic gives us false hope.",
                    5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = fran; XinitDelay[0] = 0f; XendDelay[0] = 6.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "My family were Paladins before me; they understood.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 7.5f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "They hunted people like you. I always admired them.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...They hunted people like us."; XskipAvailable[2] = false;

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 8f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = true;
                Xline[3] = "I won't be this way forever. Once I'm human again I'll rejoin my comrades in arms.";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "FAMILY3")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Tell the Truth";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = null;
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "No. Unfortunately you won't.";
                answering = false;

                nextTag = "FAMILYPROMPTS";

                StartCoroutine(poseQuestion(fran, 2f, false,
                    "I beg your pardon?",
                    2f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = null; XinitDelay[0] = 0f; XendDelay[0] = 6.5f; XpostDelay[0] = .25f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "There is no returning to humanity. Many have tried; all failed.";

                Xnpc[1] = null; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .5f; XisPlayer[1] = true; Xtrigger[1] = false;
                Xline[1] = "I'm sorry.";

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 2.5f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 6f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "...Well what about the big fella? What's your story?";

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 3f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "My story is brief:";

                Xnpc[5] = gunn; XinitDelay[5] = 0; XendDelay[5] = 7.5f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "My parents always thought I'd make a fine smith. I did.";

                Xnpc[6] = rob; XinitDelay[6] = 0; XendDelay[6] = 5.5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "That's it? There must be something more.";

                Xnpc[7] = gunn; XinitDelay[7] = 0; XendDelay[7] = 5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "My father bakes exquisite bread.";

                Xnpc[8] = rob; XinitDelay[8] = 0; XendDelay[8] = 2f; XpostDelay[8] = .5f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "...Sweet.";

                Xnpc[9] = gunn; XinitDelay[9] = 0; XendDelay[9] = 3.5f; XpostDelay[9] = .75f; XisPlayer[9] = false; Xtrigger[9] = true;
                Xline[9] = "He makes sourdough, actually.";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "FAMILYPROMPTS")
        {
            youDiag.text = "Who do you want to question further? (Pick One)";

            oneAnswer.text = "1) Francesca";
            twoAnswer.text = "2) Gunnlaug";
            thrAnswer.text = "3) Robin";

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                youDiag.text = "Francesca: you said you joined the Paladin armada for your family...";
                answering = false;

                nextTag = "CHILDHOODPROMPTS1";

                StartCoroutine(poseQuestion(null, 4f, true,
                    "Was there no other reason?",
                    4f, false, .5f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = fran; XinitDelay[0] = 0f; XendDelay[0] = 7.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Do I need some trivial motivation to hunt murderers?";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "You murder people too...";

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "We cleanse them.";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 5.5f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "I'd rather you didn't 'cleanse' me, thanks.";

                Xnpc[4] = fran; XinitDelay[4] = 0; XendDelay[4] = 7.5f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "Do you understand what you've become?";

                Xnpc[5] = fran; XinitDelay[5] = 0; XendDelay[5] = 9.5f; XpostDelay[5] = 1f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Lichs are leeches: they cannot survive without sucking the life out of the world around them.";

                Xnpc[6] = null; XinitDelay[6] = 0; XendDelay[6] = 8.5f; XpostDelay[6] = .5f; XisPlayer[6] = true; Xtrigger[6] = false;
                Xline[6] = "You force us to fight. We try to give back all the lives we can.";

                Xnpc[7] = fran; XinitDelay[7] = 0; XendDelay[7] = 6.5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "And how many lives did you save on your last raid?";

                Xnpc[8] = fran; XinitDelay[8] = 0; XendDelay[8] = 4f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "Three. You saved three lives.";

                Xnpc[9] = fran; XinitDelay[9] = 0; XendDelay[9] = 5.5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "Were we the only corpses you fiends left intact?";

                Xnpc[10] = rob; XinitDelay[10] = 0; XendDelay[10] = 3.5f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "They're helping us!";

                Xnpc[11] = fran; XinitDelay[11] = 0; XendDelay[11] = 3.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[11] = false;
                Xline[11] = "THEY ARE THIEVES! PIRATES!"; XskipAvailable[11] = false;

                Xnpc[12] = fran; XinitDelay[12] = 0; XendDelay[12] = 7.5f; XpostDelay[12] = .25f; XisPlayer[12] = false; Xtrigger[12] = false;
                Xline[12] = "Only difference is they're stealing lives! The only thing that can't be replaced..."; XskipAvailable[12] = false;

                Xnpc[13] = fran; XinitDelay[13] = 0; XendDelay[13] = 4.5f; XpostDelay[13] = .25f; XisPlayer[13] = false; Xtrigger[13] = false;
                Xline[13] = "Daughters, fathers, friends."; XskipAvailable[13] = false;

                Xnpc[14] = fran; XinitDelay[14] = 0; XendDelay[14] = 8f; XpostDelay[14] = .25f; XisPlayer[14] = false; Xtrigger[14] = false;
                Xline[14] = "Do not pretend that by indoctrinating the scraps they have somehow absolved themselves."; XskipAvailable[14] = false;

                Xnpc[15] = rob; XinitDelay[15] = 0; XendDelay[15] = 2.5f; XpostDelay[15] = .25f; XisPlayer[15] = false; Xtrigger[15] = false;
                Xline[15] = "..."; XskipAvailable[15] = false;

                Xnpc[16] = gunn; XinitDelay[16] = 0; XendDelay[16] = 7f; XpostDelay[16] = .25f; XisPlayer[16] = false; Xtrigger[16] = true;
                Xline[16] = "They survive, as we all do. Like it or not, you are also a leech."; XskipAvailable[16] = true;
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "Gunnlaug: can you tell me more about your parents? What were they like?";
                answering = false;

                nextTag = "CHILDHOODPROMPTS";

                StartCoroutine(poseQuestion(gunn, 3.5f, false,
                    "Kind, patient, steadyhands: the expected qualities of a decent parent.",
                    4.5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = null; XinitDelay[0] = 2f; XendDelay[0] = 4f; XpostDelay[0] = .5f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "Does your father have a trade?";

                Xnpc[1] = gunn; XinitDelay[1] = 0f; XendDelay[1] = 4.5f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "My father is a baker. My mother works iron.";

                Xnpc[2] = gunn; XinitDelay[2] = 0f; XendDelay[2] = 6f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "I learnt from them both, honing my creativity and my craft.";

                Xnpc[3] = gunn; XinitDelay[3] = 0; XendDelay[3] = 5.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "I think they taught me about the beauty in my work..."; XskipAvailable[3] = false;

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "Maybe in death I can be greater..."; XskipAvailable[4] = false;

                Xnpc[5] = fran; XinitDelay[5] = 0; XendDelay[5] = 4.5f; XpostDelay[5] = 1f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "You must have made them very proud.";

                Xnpc[6] = gunn; XinitDelay[6] = 0; XendDelay[6] = 5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "Perhaps. I left home young. Sailed out.";

                Xnpc[7] = gunn; XinitDelay[7] = 0; XendDelay[7] = 3.5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "That was that.";

                Xnpc[8] = rob; XinitDelay[8] = 0; XendDelay[8] = 2f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "WHAT?!";

                Xnpc[9] = rob; XinitDelay[9] = 0; XendDelay[9] = 3.5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "Don't you visit??";

                Xnpc[10] = gunn; XinitDelay[10] = 0; XendDelay[10] = 4.5f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "No. The journey is arduous.";

                Xnpc[11] = gunn; XinitDelay[11] = 0; XendDelay[11] = 7.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[11] = false;
                Xline[11] = "They visited once, for the summer. My partner became quite fond of them.";

                Xnpc[12] = rob; XinitDelay[12] = 0; XendDelay[12] = 3.5f; XpostDelay[12] = .25f; XisPlayer[12] = false; Xtrigger[12] = false;
                Xline[12] = "You must miss them.";

                Xnpc[13] = gunn; XinitDelay[13] = 0; XendDelay[13] = 5.5f; XpostDelay[13] = .25f; XisPlayer[13] = false; Xtrigger[13] = false;
                Xline[13] = "Not especially. They're busy people, as am I.";

                Xnpc[14] = gunn; XinitDelay[14] = 0; XendDelay[14] = 5f; XpostDelay[14] = .25f; XisPlayer[14] = false; Xtrigger[14] = false;
                Xline[14] = "Our love withstands time and distance.";

                Xnpc[15] = gunn; XinitDelay[15] = 0; XendDelay[15] = 4.5f; XpostDelay[15] = .25f; XisPlayer[15] = false; Xtrigger[15] = false;
                Xline[15] = "We see no need to reaffirm that.";

                Xnpc[16] = rob; XinitDelay[16] = 0; XendDelay[16] = 5f; XpostDelay[16] = .25f; XisPlayer[16] = false; Xtrigger[16] = false;
                Xline[16] = "Sounds unhealthy...";

                Xnpc[17] = fran; XinitDelay[17] = 0; XendDelay[17] = 2.5f; XpostDelay[17] = .25f; XisPlayer[17] = false; Xtrigger[17] = false;
                Xline[17] = "Why?";

                Xnpc[18] = rob; XinitDelay[18] = 0; XendDelay[18] = 3.5f; XpostDelay[18] = .25f; XisPlayer[18] = false; Xtrigger[18] = false;
                Xline[18] = "...";

                Xnpc[19] = null; XinitDelay[19] = 0; XendDelay[19] = 3.5f; XpostDelay[19] = .25f; XisPlayer[19] = true; Xtrigger[19] = false;
                Xline[19] = "I think that'll do for tonight. Thank you.";

                Xnpc[20] = null; XinitDelay[20] = 0; XendDelay[20] = 3.5f; XpostDelay[20] = .25f; XisPlayer[20] = true; Xtrigger[20] = false;
                Xline[20] = "I'll begin searching for your phylacteries tomorrow morning.";

                //end
                maxI = 20;
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {
                youDiag.text = "Robin: could you say more about your family?";
                answering = false;

                nextTag = "CHILDHOODPROMPTS";

                StartCoroutine(poseQuestion(rob, 2.5f, false,
                    "For sure! Six of the buggers: Davey, Stace, Marge, Henry, an' Alfie.",
                    5.5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = .5f; XendDelay[0] = 3f; XpostDelay[0] = .5f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Sounds...loud.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 4.5f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Bloody was! Not Davey, but Alfie, the youngest--";

                Xnpc[2] = rob; XinitDelay[2] = 0f; XendDelay[2] = 5.5f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "Kid could talk your ear off with a mouth full of Mahi!";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 3.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "You were close?";

                Xnpc[4] = rob; XinitDelay[4] = 0; XendDelay[4] = 7f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "Course we were! Didn't see much of the boys during the day though--";

                Xnpc[5] = rob; XinitDelay[5] = 0; XendDelay[5] = 5f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "--out fishing or carryin' heavy shit.";

                Xnpc[6] = rob; XinitDelay[6] = 0; XendDelay[6] = 5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "Marge though, she kept around the house with Ma.";

                Xnpc[7] = rob; XinitDelay[7] = 0; XendDelay[7] = 4.5f; XpostDelay[7] = 1f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "Partners in crime, we were...";

                Xnpc[8] = fran; XinitDelay[8] = 0; XendDelay[8] = 3.5f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "You must miss them deeply.";

                Xnpc[9] = rob; XinitDelay[9] = 0; XendDelay[9] = 3.5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "Yeah...a lot.";

                Xnpc[10] = rob; XinitDelay[10] = 0; XendDelay[10] = 6.5f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "Devils, the lot of them. But they had good souls. Good company."; XskipAvailable[10] = false;

                Xnpc[11] = fran; XinitDelay[11] = 0; XendDelay[11] = 4.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[11] = false;
                Xline[11] = "Shame the Lichs stole that from you."; XskipAvailable[11] = false;

                Xnpc[12] = rob; XinitDelay[12] = 0; XendDelay[12] = 6.5f; XpostDelay[12] = .25f; XisPlayer[12] = false; Xtrigger[12] = false;
                Xline[12] = "...True, very true. But I got my legs back and some added bonuses."; XskipAvailable[12] = false;

                Xnpc[13] = rob; XinitDelay[13] = 0; XendDelay[13] = 3f; XpostDelay[13] = .5f; XisPlayer[13] = false; Xtrigger[13] = false;
                Xline[13] = "So it's not all bad..."; XskipAvailable[13] = false;

                Xnpc[14] = rob; XinitDelay[14] = 0; XendDelay[14] = 3.5f; XpostDelay[14] = 1f; XisPlayer[14] = false; Xtrigger[14] = false;
                Xline[14] = "...Never all bad."; XskipAvailable[14] = false;

                Xnpc[15] = fran; XinitDelay[15] = 0; XendDelay[15] = 4.5f; XpostDelay[15] = .5f; XisPlayer[15] = false; Xtrigger[15] = false;
                Xline[15] = "...So it was worth it, then?"; XskipAvailable[15] = false;

                Xnpc[16] = rob; XinitDelay[16] = 0; XendDelay[16] = 2.5f; XpostDelay[16] = .25f; XisPlayer[16] = false; Xtrigger[16] = false;
                Xline[16] = "..."; XskipAvailable[16] = false;

                Xnpc[17] = null; XinitDelay[17] = 0; XendDelay[17] = 3.5f; XpostDelay[17] = .25f; XisPlayer[17] = true; Xtrigger[17] = false;
                Xline[17] = "I think that'll do for tonight. Thank you.";

                Xnpc[18] = null; XinitDelay[18] = 0; XendDelay[18] = 3.5f; XpostDelay[18] = .25f; XisPlayer[18] = true; Xtrigger[18] = false;
                Xline[18] = "I'll begin searching for your phylacteries tomorrow morning.";

                //end
                maxI = 18;
            }
        }

        if (answerTag == "FAMILYPROMPTS1")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Agree with Gunn";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Be Diplomatic";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "Gunnlaug is right. Refusing our help is an unwise choice.";
                answering = false;

                StartCoroutine(poseQuestion(fran, 4.5f, true,
                    "...",
                    3f, false, .5f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 4.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "...I'm sorry she spouted all that.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Don't speak for me.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 6.5f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "I'm going to bed, Pirate. Find my phylactery quickly.";

                //END NIGHT
                maxI = 3;
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                youDiag.text = "You don't have to like us, but if you wish to survive, please cooperate.";
                answering = false;

                nextTag = "CHILDHOODPROMPTS2";

                StartCoroutine(poseQuestion(fran, 4.5f, true,
                    "...",
                    3f, false, .5f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 4.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "...I'm sorry she spouted all that.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Don't speak for me.";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 3f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 6.5f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "I'm going to bed, Pirate. Find my phylactery quickly.";

                //END NIGHT
                maxI = 3;
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        //C0A artifact stuff

        if (answerTag == "C01")     //Night2
        {
            youDiag.text = "Let's begin:";

            oneAnswer.text = "1) What do you regret?"; 
            twoAnswer.text = null;
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "Let's begin: what do you regret?";
                answering = false;

                StartCoroutine(poseQuestion(fran, 1.5f, false,
                    "Nothing.",
                    2f, false, .25f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 2.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Nothing?";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 3f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "...At all?";

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 2f; XpostDelay[2] = 1f; XisPlayer[2] = false; Xtrigger[1] = false;
                Xline[2] = "No."; XskipAvailable[2] = false;

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 2.5f; XpostDelay[3] = .5f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "That's impressive.";

                Xnpc[4] = rob; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "I regret...a lot, I guess.";

                Xnpc[5] = rob; XinitDelay[5] = 0; XendDelay[5] = 5f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Can you regret things you've never done?";

                Xnpc[6] = gunn; XinitDelay[6] = 0; XendDelay[6] = 5.5f; XpostDelay[6] = 1f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "Absolutely. And now I think we've a chance to do them.";

                Xnpc[6] = null; XinitDelay[6] = 0; XendDelay[6] = 3.5f; XpostDelay[6] = 1f; XisPlayer[6] = true; Xtrigger[6] = false;
                Xline[6] = "What about you, Gunnlaug?";

                Xnpc[6] = gunn; XinitDelay[6] = 0; XendDelay[6] = 5f; XpostDelay[6] = 1f; XisPlayer[6] = false; Xtrigger[6] = true;
                Xline[6] = "I have one regret. That might change soon, though.";

                nextTag = "REGRETPROMPTS";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }       //C = night2

        if (answerTag == "REGRETPROMPTS")
        {
            youDiag.text = "Who do you want to question further? (Pick One)";

            oneAnswer.text = "1) Francesca";
            twoAnswer.text = "2) Gunnlaug";
            thrAnswer.text = "3) Robin";

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "Francesca, you must regret something. Please.";
                answering = false;

                nextTag = "REGRETPROMPTSagain";

                StartCoroutine(poseQuestion(fran, 4f, false,
                    "I've nothing to give, pirate. Probe someone else.",
                    4.5f, false, .5f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 3f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "That's not how it works.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 5f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "I'm sorry; I didn't realize there was a system to this.";

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 5f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = true;
                Xline[2] = "I've nothing of value to share. Try one of the other corpses.";                
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "What do you hope to do, Gunnlaug?";
                answering = false;

                nextTag = "REGRETPROMPTSG";

                StartCoroutine(poseQuestion(gunn, 2.5f, false,
                    "...",
                    2f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 2f; XendDelay[0] = 4.5f; XpostDelay[0] = .5f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "...I think the pommel is my favourite part of a sword.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 4.5f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "I prefer the blade.";

                Xnpc[2] = rob; XinitDelay[2] = 0f; XendDelay[2] = 6f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...Did I miss something?";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 5.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "He's saying he's a pacifist.";

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "I'm saying I'm an artist.";

                Xnpc[5] = gunn; XinitDelay[5] = 0; XendDelay[5] = 4.5f; XpostDelay[5] = 1f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "I've perfected the blade; it bores me.";

                Xnpc[6] = fran; XinitDelay[6] = 0; XendDelay[6] = 5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "Perfect? No weapon is perfect.";

                Xnpc[7] = gunn; XinitDelay[7] = 0; XendDelay[7] = 3.5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "I could change your mind.";

                Xnpc[8] = fran; XinitDelay[8] = 0; XendDelay[8] = 2f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "Ha!...I'll take you up on that.";

                Xnpc[9] = rob; XinitDelay[9] = 0; XendDelay[9] = 3.5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "...So what then if not swords?";

                Xnpc[10] = gunn; XinitDelay[10] = 0; XendDelay[10] = 4.5f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "Jewelry: the finest work a man can craft.";

                Xnpc[11] = rob; XinitDelay[11] = 0; XendDelay[11] = 7.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[11] = true;
                Xline[11] = "I take it you never made any.";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {
                youDiag.text = "Robin, you have to be more specific.";
                answering = false;

                nextTag = "REGRETPROMPTSR";

                StartCoroutine(poseQuestion(null, 3f, true,
                    "What exactly do you regret? Take your time.",
                    4.5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = .5f; XendDelay[0] = 3.5f; XpostDelay[0] = .5f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "I dunno. My family never had cache. Money.";

                Xnpc[1] = gunn; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Reales aren't easy to come by these days.";

                Xnpc[2] = rob; XinitDelay[2] = 0f; XendDelay[2] = 3f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "No they are not.";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 6.5f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "All me brothers and sisters had jobs in and out of the house...";

                Xnpc[4] = rob; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .5f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "They all did their part, right?";

                Xnpc[5] = fran; XinitDelay[5] = 0; XendDelay[5] = 2.5f; XpostDelay[5] = .5f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Of course.";

                Xnpc[6] = rob; XinitDelay[6] = 0; XendDelay[6] = 3.5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = true;
                Xline[6] = "Yeah, that's the problem.";
            }
        }

        if (answerTag == "REGRETPROMPTSG")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Investigate";
            twoAnswer.text = "2) Console";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "Why didn't you pursue your dreams?";
                answering = false;

                StartCoroutine(poseQuestion(gunn, 3f, false,
                    "Wasn't profitable. Gems are hard to come by and even if they weren't, nobody would be buying.",
                    7f, false, .5f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 7f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Things would be different had I lived in Port-au-Prince or somewhere wealthy.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 6.5f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Well now that we're pirates I guess you can just steal what you need...";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 2.5f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "We...?";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 3f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "...";

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "I suppose that's one way of doing it.";

                Xnpc[5] = null; XinitDelay[5] = 0; XendDelay[5] = 4.5f; XpostDelay[5] = .25f; XisPlayer[5] = true; Xtrigger[5] = false;
                Xline[5] = "...Alright, I think that'll do for tonight. Thank you.";

                Xnpc[6] = null; XinitDelay[6] = 0; XendDelay[6] = 4f; XpostDelay[6] = .25f; XisPlayer[6] = true; Xtrigger[6] = false;
                Xline[6] = "I'll continue searching for more phylacteries tomorrow morning.";

                //end
                maxI = 6;
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "Chasing dreams can be...daunting.";
                answering = false;

                StartCoroutine(poseQuestion(gunn, 3.5f, false,
                    "No, it wouldn't've been profitable. Gems are hard to come by and even if they weren't, nobody would be buying.",
                    8.5f, false, .5f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 7f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Things would be different had I lived in Port-au-Prince or somewhere wealthy.";

                Xnpc[1] = rob; XinitDelay[1] = 0f; XendDelay[1] = 6.5f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Well now that we're pirates I guess you can just steal what you need...";

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 2.5f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "We...?";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 3f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "...";

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "I suppose that's one way of doing it.";

                Xnpc[5] = null; XinitDelay[5] = 0; XendDelay[5] = 3.5f; XpostDelay[5] = .25f; XisPlayer[5] = true; Xtrigger[5] = false;
                Xline[5] = "I think that'll do for tonight. Thank you.";

                Xnpc[6] = null; XinitDelay[6] = 0; XendDelay[6] = 3.5f; XpostDelay[6] = .25f; XisPlayer[6] = true; Xtrigger[6] = false;
                Xline[6] = "I'll continue searching for more phylacteries tomorrow morning.";

                //end
                maxI = 6;
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {
                
            }
        }

        if (answerTag == "REGRETPROMPTSR")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Wait";
            twoAnswer.text = "2) Ask";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "...Mhm.";
                answering = false;

                StartCoroutine(poseQuestion(rob, 4f, false,
                    "I just couldn't...",
                    7f, false, .5f, false));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 1.5f; XendDelay[0] = 4f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "I hated it. I hated being useless.";    XskipAvailable[0] = false;

                Xnpc[1] = fran; XinitDelay[1] = 1f; XendDelay[1] = 3.5f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "I don't think you were."; XskipAvailable[1] = false;

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 2.5f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...?"; XskipAvailable[2] = false;

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 6f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Sometimes--I think--helpful people are horrible to themselves.";

                Xnpc[4] = rob; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "You know we'd miss supper sometimes.";

                Xnpc[5] = rob; XinitDelay[5] = 0; XendDelay[5] = 7f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Now I can finally help, and I don't even know if they're alive.";

                Xnpc[6] = fran; XinitDelay[6] = 0; XendDelay[6] = 3.5f; XpostDelay[6] = .25f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "It's not right for you to bear that, Robin.";

                Xnpc[7] = rob; XinitDelay[7] = 0; XendDelay[7] = 4f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "Look: I know it's fucked. Don't worry about it.";

                Xnpc[8] = rob; XinitDelay[8] = 0; XendDelay[8] = 4f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "I'm beat. Could I get some shut-eye?";

                Xnpc[9] = null; XinitDelay[9] = 0; XendDelay[9] = 3.5f; XpostDelay[9] = .25f; XisPlayer[9] = true; Xtrigger[9] = false;
                Xline[9] = "...I think that'll do for tonight. Thank you.";

                Xnpc[10] = null; XinitDelay[10] = 0; XendDelay[10] = 3.5f; XpostDelay[10] = .25f; XisPlayer[10] = true; Xtrigger[10] = false;
                Xline[10] = "I'll continue searching for more phylacteries tomorrow morning.";

                //end
                maxI = 10;
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "What's the problem, Robin?";
                answering = false;

                StartCoroutine(poseQuestion(rob, 4f, false,
                    "C'mon, it's obvious:",
                    4f, false, .5f, false));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 1.5f; XendDelay[0] = 4f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "I hate it. I hated being useless."; XskipAvailable[0] = false;

                Xnpc[1] = fran; XinitDelay[1] = 1f; XendDelay[1] = 3.5f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "I don't think you were."; XskipAvailable[1] = false;

                Xnpc[2] = rob; XinitDelay[2] = 0; XendDelay[2] = 2.5f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...?"; XskipAvailable[2] = false;

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 6f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Sometimes--I think--helpful people are horrible to themselves.";

                Xnpc[4] = rob; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "You know we'd miss supper sometimes.";

                Xnpc[5] = rob; XinitDelay[5] = 0; XendDelay[5] = 7f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Now I can finally help, and I don't even know if they're alive.";

                Xnpc[6] = fran; XinitDelay[6] = 0; XendDelay[6] = 3.5f; XpostDelay[6] = .25f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "It's not right for you to bear that, Robin.";

                Xnpc[7] = rob; XinitDelay[7] = 0; XendDelay[7] = 4f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "Look: I know it's fucked. Don't worry about it.";

                Xnpc[8] = rob; XinitDelay[8] = 0; XendDelay[8] = 4f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "I'm beat. Could I get some shut-eye?";

                Xnpc[9] = null; XinitDelay[9] = 0; XendDelay[9] = 3.5f; XpostDelay[9] = .25f; XisPlayer[9] = true; Xtrigger[9] = false;
                Xline[9] = "...I think that'll do for tonight. Thank you.";

                Xnpc[10] = null; XinitDelay[10] = 0; XendDelay[10] = 3.5f; XpostDelay[10] = .25f; XisPlayer[10] = true; Xtrigger[10] = false;
                Xline[10] = "I'll continue searching for more phylacteries tomorrow morning.";

                //end
                maxI = 10;
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "REGRETPROMPTSagain")
        {
            youDiag.text = "Who do you want to question further? (Pick One)";

            oneAnswer.text = "1) Push Francesca Further";
            twoAnswer.text = "2) Gunnlaug";
            thrAnswer.text = "3) Robin";

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "I'm sorry Francesca. But if you want a phylactery, you have to be honest with me.";
                answering = false;

                nextTag = "REGRETPROMPTSF";

                StartCoroutine(poseQuestion(fran, 4f, false,
                    "I won't be called a liar by the likes of you.",
                    4.5f, false, .5f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 0f; XendDelay[0] = 2.5f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Lies are lies.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 3f; XpostDelay[1] = .75f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "...";   XskipAvailable[1] = false;

                Xnpc[2] = fran; XinitDelay[2] = 1.5f; XendDelay[2] = 3.5f; XpostDelay[2] = .75f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "I lost someone."; XskipAvailable[2] = false;

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 2.5f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "Finally!";

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 2.5f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "Hush.";

                Xnpc[5] = fran; XinitDelay[5] = 0; XendDelay[5] = 5.5f; XpostDelay[5] = 1f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Before all of this, it was my brother Jacob."; XskipAvailable[5] = false;

                Xnpc[6] = fran; XinitDelay[6] = 0; XendDelay[6] = 6f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "I don't...it's not a regret, but I dwell on it.";

                Xnpc[7] = gunn; XinitDelay[7] = 0; XendDelay[7] = 3f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "What happened?";

                Xnpc[8] = fran; XinitDelay[8] = 0; XendDelay[8] = 4.5f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "His friend was murdered, in battle."; 

                Xnpc[9] = fran; XinitDelay[9] = 0; XendDelay[9] = 4f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "Jacob did something stupid."; XskipAvailable[9] = false;

                Xnpc[10] = fran; XinitDelay[10] = 0; XendDelay[10] = 5f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "The Church found out...";

                Xnpc[11] = rob; XinitDelay[11] = 0; XendDelay[11] = 4.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[11] = false;
                Xline[11] = "Good thing they're so forgiving.";

                Xnpc[12] = fran; XinitDelay[12] = 0; XendDelay[12] = 4.5f; XpostDelay[12] = .25f; XisPlayer[12] = false; Xtrigger[12] = false;
                Xline[12] = "...He was burned at the stake."; XskipAvailable[12] = false;

                Xnpc[13] = rob; XinitDelay[13] = .5f; XendDelay[13] = 4.5f; XpostDelay[13] = .25f; XisPlayer[13] = false; Xtrigger[13] = false;
                Xline[13] = "..."; XskipAvailable[13] = false;

                Xnpc[14] = fran; XinitDelay[14] = 0; XendDelay[14] = 6f; XpostDelay[14] = .25f; XisPlayer[14] = false; Xtrigger[14] = false;
                Xline[14] = "Enough about my brother, though. This is about me.";

                Xnpc[15] = rob; XinitDelay[15] = 0; XendDelay[15] = 6.5f; XpostDelay[15] = .25f; XisPlayer[15] = false; Xtrigger[15] = false;
                Xline[15] = "Hang on, if you won't say what happened I'll bloody work it out!";

                Xnpc[16] = fran; XinitDelay[16] = 0; XendDelay[16] = 2.5f; XpostDelay[16] = .25f; XisPlayer[16] = false; Xtrigger[16] = false;
                Xline[16] = "Please: don't.";

                Xnpc[17] = rob; XinitDelay[17] = 0; XendDelay[17] = 4f; XpostDelay[17] = .25f; XisPlayer[17] = false; Xtrigger[17] = false;
                Xline[17] = "Too late, already on it.";

                Xnpc[18] = null; XinitDelay[18] = 0; XendDelay[18] = 2.5f; XpostDelay[18] = .25f; XisPlayer[18] = true; Xtrigger[18] = false;
                Xline[18] = "Robin--";

                Xnpc[19] = fran; XinitDelay[19] = 0; XendDelay[19] = 6.5f; XpostDelay[19] = .25f; XisPlayer[19] = false; Xtrigger[19] = false;
                Xline[19] = "It doesn't matter. I answered the pirate's question. We're done.";

                Xnpc[20] = rob; XinitDelay[20] = 0; XendDelay[20] = 3f; XpostDelay[20] = .25f; XisPlayer[20] = false; Xtrigger[20] = false;
                Xline[20] = "But we're not!";

                Xnpc[21] = null; XinitDelay[21] = 0; XendDelay[21] = 5.5f; XpostDelay[21] = .25f; XisPlayer[21] = false; Xtrigger[21] = false;
                Xline[21] = "Pretty hard to get thrown on a pyre these days...";

                Xnpc[22] = null; XinitDelay[22] = 0; XendDelay[22] = 6.5f; XpostDelay[22] = .25f; XisPlayer[22] = false; Xtrigger[22] = true;
                Xline[22] = "...But if there's one thing I know Paladins hate, it's necromancy.";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "...Then what do you hope to do, Gunnlaug?";
                answering = false;

                nextTag = "REGRETPROMPTSG";

                StartCoroutine(poseQuestion(gunn, 2.5f, false,
                    "...",
                    2f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 2f; XendDelay[0] = 4f; XpostDelay[0] = .5f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "The pommel is my favourite part of a sword.";

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 4.5f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "I prefer the blade.";

                Xnpc[2] = rob; XinitDelay[2] = 0f; XendDelay[2] = 6f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "...Did I miss something?";

                Xnpc[3] = fran; XinitDelay[3] = 0; XendDelay[3] = 5.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "He's saying he's a pacifist.";

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "I'm saying I'm an artist.";

                Xnpc[5] = gunn; XinitDelay[5] = 0; XendDelay[5] = 4.5f; XpostDelay[5] = 1f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "I've perfected the blade; it bores me.";

                Xnpc[6] = fran; XinitDelay[6] = 0; XendDelay[6] = 5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "Perfect? No weapon is perfect.";

                Xnpc[7] = gunn; XinitDelay[7] = 0; XendDelay[7] = 3.5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "I could change your mind.";

                Xnpc[8] = fran; XinitDelay[8] = 0; XendDelay[8] = 2f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "Oh, uhm...I'd like to see that.";

                Xnpc[9] = rob; XinitDelay[9] = 0; XendDelay[9] = 3.5f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "...So what then if not swords?";

                Xnpc[10] = gunn; XinitDelay[10] = 0; XendDelay[10] = 4.5f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "Jewelry: the finest work a man can craft.";

                Xnpc[11] = rob; XinitDelay[11] = 0; XendDelay[11] = 7.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[11] = true;
                Xline[11] = "I take it you never made any.";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {
                youDiag.text = "...Then Robin, you have to be more specific.";
                answering = false;

                nextTag = "REGRETPROMPTSR";

                StartCoroutine(poseQuestion(null, 3f, true,
                    "What exactly do you regret? Take your time.",
                    4.5f, false, 0f, true));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = .5f; XendDelay[0] = 3.5f; XpostDelay[0] = .5f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "I dunno. My family never had cache. Money.";

                Xnpc[1] = gunn; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Reales aren't easy to come by these days.";

                Xnpc[2] = rob; XinitDelay[2] = 0f; XendDelay[2] = 3f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "No they are not.";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 6.5f; XpostDelay[3] = .75f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "All me brothers and sisters had jobs in and out of the house...";

                Xnpc[4] = rob; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .5f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "They all did their part, right?";

                Xnpc[5] = fran; XinitDelay[5] = 0; XendDelay[5] = 2.5f; XpostDelay[5] = .5f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Of course.";

                Xnpc[6] = rob; XinitDelay[6] = 0; XendDelay[6] = 3.5f; XpostDelay[6] = .5f; XisPlayer[6] = false; Xtrigger[6] = true;
                Xline[6] = "Yeah, that's the problem.";
            }
        }

        if (answerTag == "REGRETPROMPTSF")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Stop Robin";
            twoAnswer.text = "2) Say Nothing";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "I appreciate your help, Robin--";
                answering = false;

                StartCoroutine(poseQuestion(null, 3.5f, true,
                    "But we cannot force people to share things. It's the wrong way.",
                    6.5f, false, .5f, false));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = .5f; XendDelay[0] = 3f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Sorry."; XskipAvailable[0] = false;

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 3.5f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Thank you, pirate."; XskipAvailable[1] = false;

                Xnpc[2] = fran; XinitDelay[2] = 0; XendDelay[2] = 5f; XpostDelay[2] = .5f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "And Robin? Don't dare try prying into my life again."; XskipAvailable[2] = false;

                Xnpc[3] = null; XinitDelay[3] = 0; XendDelay[3] = 4f; XpostDelay[3] = .25f; XisPlayer[3] = true; Xtrigger[3] = false;
                Xline[3] = "...I think that'll do for tonight. Apologies, Francesca.";

                Xnpc[4] = null; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .25f; XisPlayer[4] = true; Xtrigger[4] = false;
                Xline[4] = "I'll begin searching for your phylacteries tomorrow morning.";

                //end
                maxI = 9;
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "...";
                answering = false;

                nextTag = "FranDefuse";

                StartCoroutine(poseQuestion(rob, 5.5f, false,
                    "Why would your brother be fiddling with necromancy, though?",
                    4f, false, .25f, false));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = .75f; XendDelay[0] = 3f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = false;       //start getting fast here, pressuring Fran
                Xline[0] = "...The friend.";

                Xnpc[1] = rob; XinitDelay[1] = 1f; XendDelay[1] = 2.75f; XpostDelay[1] = .25f; XisPlayer[1] = false; Xtrigger[1] = false;
                Xline[1] = "Ah so he--";

                Xnpc[2] = gunn; XinitDelay[2] = 0; XendDelay[2] = 3.5f; XpostDelay[2] = .25f; XisPlayer[2] = false; Xtrigger[2] = false;
                Xline[2] = "--brought him back and was apprehended.";

                Xnpc[3] = rob; XinitDelay[3] = 0; XendDelay[3] = 3.5f; XpostDelay[3] = .25f; XisPlayer[3] = false; Xtrigger[3] = false;
                Xline[3] = "But...was he successful, Francesca?";

                Xnpc[4] = gunn; XinitDelay[4] = 0; XendDelay[4] = 4f; XpostDelay[4] = .25f; XisPlayer[4] = false; Xtrigger[4] = false;
                Xline[4] = "He would've been caught practicing before then.";

                Xnpc[5] = rob; XinitDelay[5] = 0; XendDelay[5] = 2.75f; XpostDelay[5] = .25f; XisPlayer[5] = false; Xtrigger[5] = false;
                Xline[5] = "Oh. Definitely.";

                Xnpc[6] = gunn; XinitDelay[6] = 0; XendDelay[6] = 3f; XpostDelay[6] = .25f; XisPlayer[6] = false; Xtrigger[6] = false;
                Xline[6] = "I wonder how though...";

                Xnpc[7] = rob; XinitDelay[7] = 0; XendDelay[7] = 3.5f; XpostDelay[7] = .25f; XisPlayer[7] = false; Xtrigger[7] = false;
                Xline[7] = "Oh god, this' awful, but...";

                Xnpc[8] = rob; XinitDelay[8] = 0; XendDelay[8] = 3.5f; XpostDelay[8] = .25f; XisPlayer[8] = false; Xtrigger[8] = false;
                Xline[8] = "Were you there when they caught him?";

                Xnpc[9] = fran; XinitDelay[9] = 0; XendDelay[9] = 2.25f; XpostDelay[9] = .25f; XisPlayer[9] = false; Xtrigger[9] = false;
                Xline[9] = "I--";

                Xnpc[10] = gunn; XinitDelay[10] = 0; XendDelay[10] = 2.25f; XpostDelay[10] = .25f; XisPlayer[10] = false; Xtrigger[10] = false;
                Xline[10] = "Oh no.";
                
                Xnpc[11] = rob; XinitDelay[11] = 0; XendDelay[11] = 4.5f; XpostDelay[11] = .25f; XisPlayer[11] = false; Xtrigger[11] = false;
                Xline[11] = "How'd the inquisitors know? Someone had to tell them--";

                Xnpc[12] = rob; XinitDelay[12] = 0; XendDelay[12] = 4f; XpostDelay[12] = .25f; XisPlayer[12] = false; Xtrigger[12] = false;
                Xline[12] = "Was he practicing at home? Did your family kno--"; XskipAvailable[12] = false;

                Xnpc[13] = fran; XinitDelay[13] = 0; XendDelay[13] = 2.5f; XpostDelay[13] = 1f; XisPlayer[13] = false; Xtrigger[13] = false;
                Xline[13] = "ENOUGH!";   XskipAvailable[13] = false;

                Xnpc[14] = rob; XinitDelay[14] = 0; XendDelay[14] = 2f; XpostDelay[14] = .25f; XisPlayer[14] = false; Xtrigger[14] = false;
                Xline[14] = "Oh..."; XskipAvailable[14] = false;

                Xnpc[15] = gunn; XinitDelay[15] = 0; XendDelay[15] = 2f; XpostDelay[15] = 4f; XisPlayer[15] = false; Xtrigger[15] = false;
                Xline[15] = "..."; XskipAvailable[15] = false;

                Xnpc[16] = fran; XinitDelay[16] = 0; XendDelay[16] = 5f; XpostDelay[16] = .25f; XisPlayer[16] = false; Xtrigger[16] = false;
                Xline[16] = "My brother is not some case for you to crack.";

                Xnpc[17] = fran; XinitDelay[17] = 0; XendDelay[17] = 8f; XpostDelay[17] = .25f; XisPlayer[17] = false; Xtrigger[17] = false;
                Xline[17] = "We've all lost family, so I'm sure you understand when I ask you not to treat a death like a fucking puzzle.";

                Xnpc[18] = gunn; XinitDelay[18] = 0; XendDelay[18] = 3f; XpostDelay[18] = .25f; XisPlayer[18] = false; Xtrigger[18] = false;
                Xline[18] = "Apologies.";

                Xnpc[19] = rob; XinitDelay[19] = 0; XendDelay[19] = 3f; XpostDelay[19] = .25f; XisPlayer[19] = false; Xtrigger[19] = true;
                Xline[19] = "...";
            }
            else if (thrAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))  //if choice 3 is selected
            {

            }
        }

        if (answerTag == "FranRefuse")
        {
            youDiag.text = "";

            oneAnswer.text = "1) Defuse";
            twoAnswer.text = null;
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
            {
                youDiag.text = "I think it's best we all go to bed.";
                answering = false;

                StartCoroutine(poseQuestion(fran, 3.5f, false,
                    "I couldn't agree more.",
                    6.5f, false, .5f, false));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = null; XinitDelay[0] = 0; XendDelay[0] = 3f; XpostDelay[0] = .25f; XisPlayer[0] = true; Xtrigger[0] = false;
                Xline[0] = "...I'm sorry.";

                Xnpc[1] = null; XinitDelay[1] = 0; XendDelay[1] = 4f; XpostDelay[1] = .25f; XisPlayer[1] = true; Xtrigger[1] = false;
                Xline[1] = "I'll continue searching for more phylacteries tomorrow morning.";

                //end
                maxI = 1;
            }
        }

        ///*********LINE************************************************************************************************///
    }

///_______________________________________________________________________________________________________________________________________________________________________
//////////////////////END PLAYER SCRIPT
///_______________________________________________________________________________________________________________________________________________________________________

    IEnumerator poseQuestion(NPC speaker, float initDelay, bool isPlayer, string line, float endDelay, bool trigger, float postDelay, bool skippable)
    {
        //initialise skippable dialogue
        for (int i = 0; i < XskipAvailable.Length; i++)
        {
            XskipAvailable[i] = true;
        }

        youMousePrompt.color = tranWhite;
        yield return new WaitForSeconds(initDelay);

        if (!isPlayer)    //if an npc is speaking
        {
            speaker.primaryActive = true; speaker.secondaryActive = false;     //fade in speaker's dialogue box, dialogue text, and brighten their portrait

            if (skippable)
            {
                speaker.mousePrompt.color = Color.white;
                _skippable = true;
            }
            else
            {
                _skippable = false;
            }

            speaker.currentDialogue.text = line;
        }
        else
        {
            if (skippable)
            {
                youMousePrompt.color = Color.white;
                _skippable = true;
            }
            else
            {
                _skippable = false;
            }

            playerSpeaking = true;

            oneAnswer.text = null;  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = null;
            thrAnswer.text = null;
            youDiag.text = line;
        }

        ///Debug.Log(skippable);
        ff = false;
        ffSpeed = endDelay;

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
            answerTag = nextTag;
        }
        else     //if another NPC is gonna pipe in before player talking
        {
            lineI++;
            StartCoroutine(poseQuestion(Xnpc[lineI], XinitDelay[lineI], XisPlayer[lineI], Xline[lineI], XendDelay[lineI], Xtrigger[lineI], XpostDelay[lineI], XskipAvailable[lineI]));
            ///Debug.Log("New Line: " + Xnpc[lineI]);
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
