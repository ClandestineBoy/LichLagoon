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
    public string dialogueID = "";

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
                        7f, true, .5f, true));

            nextTag = "B01";
            answerTag = "B01";
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

            oneAnswer.text = "1) Don't explain. (Skip Tutorial)";  //choices presented to player (leave as "null" if there are less choices
            twoAnswer.text = "2) Explain again.";
            thrAnswer.text = null;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))  //if choice 1 is selected
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
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
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
        }

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
                    4f, false, 2f, false));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 7f; XpostDelay[0] = .25f; XisPlayer[0] = false; Xtrigger[0] = true;
                Xline[0] = "Think you'll find something weird in the wreckage?";
            }
            else if (twoAnswer.text != null && Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))  //if choice 2 is selected
            {
                youDiag.text = "We do what we must.";
                answering = false;

                nextTag = "B03";

                StartCoroutine(poseQuestion(null, 3f, true,
                    "This is only safe place North of Cuba right now.",
                    4f, false, 2f, false));

                oneAnswer.text = ""; twoAnswer.text = ""; thrAnswer.text = "";

                Xnpc[0] = gunn; XinitDelay[0] = 2.5f; XendDelay[0] = 3f; XpostDelay[0] = 2f; XisPlayer[0] = false; Xtrigger[0] = false;
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

                Xnpc[0] = rob; XinitDelay[0] = 0f; XendDelay[0] = 6f; XpostDelay[0] = 1.25f; XisPlayer[0] = false; Xtrigger[0] = false;
                Xline[0] = "Huh. Guess I'll keep an open mind, then?"; XskipAvailable[0] = false;

                Xnpc[1] = fran; XinitDelay[1] = 0f; XendDelay[1] = 4f; XpostDelay[1] = .5f; XisPlayer[1] = false; Xtrigger[1] = false;
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
                youDiag.text = "I want to try and get an understanding where you all came from.";
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
                youDiag.text = "Captain Carrozo saved your life--a courtesy the Paladins have never returned.";
                answering = false;

                nextTag = "B05";

                StartCoroutine(poseQuestion(fran, .5f, false,
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
                youDiag.text = "Magic gives us choices.";
                answering = false;

                nextTag = "CHILDHOOD3";

                StartCoroutine(poseQuestion(fran, .5f, false,
                    "Magic gives us false hope.",
                    5f, false, 0f, true));

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

                nextTag = "CHILDHOOD2";

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
