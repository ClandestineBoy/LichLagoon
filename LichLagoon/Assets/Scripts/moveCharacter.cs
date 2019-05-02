using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class moveCharacter : MonoBehaviour
{
    //The component that helps us control the character's motion
    CharacterController characterController;

    //The camera attatched to the player
    public Camera cam;

    //mouse sensitivity
    public float sensitivityX;
    public float sensitivityY;
    private float currentX, currentY;
    public Transform verticalLook;

    //The animator that adds the walking head bob
    Animator bob;

    [Header("UI Fade")]
    public Image loreBacker, hex0, hex1;
    public Text headerText, bodyText;
    public float headFadeInSpeed, headFadeOutSpeed, bodyFadeInSpeed, bodyFadeOutSpeed, backFadeInSpeed, backFadeOutSpeed;

    [Header ("Grab Variables")]

    public GameObject grabPos;
    public GameObject UIAnchor;

    // Movement speed in units/sec.
    public float grabSpeed = 1.0F;
    private bool grabbing;
    GameObject grabbedObj;
    grabbable grabbedItem;
    private float targetAngle = 0;

    public float grabDist;

    public GameObject artifactTags;
    private ArtifactTags tags;


   [Header ("Mod Values")]
    //movement speed
    public float speed = 6.0f;

    //the gravity applied to the player
    public float gravity = 20.0f;

    //the direction the player is moving in
    private Vector3 moveDirection = Vector3.zero;

    public Transform sitTransform;
    private bool isDay;
    
	[Header ("KeyInputs")]
    //all of the key inputs
    public KeyCode forward;
    public KeyCode back;
    public KeyCode left;
    public KeyCode right;
    public KeyCode jump;

    [Header("Audio")]
    //the duration in between footsteps
    public float stepInterval = 0;

    //the counter for stepInterval
    private float stepI = 0;

    //audio source for footsteps
    private AudioSource stepSource;

    //array containing all sand footstep sounds
    public AudioClip[] sandSteps = new AudioClip[6];

    //array containing all wood footstep sounds
    public AudioClip[] woodSteps = new AudioClip[6];

    //booleans determining what surface is being walked on
    private bool moving = false, onWood = false, onSand = false;


    void Start()
    {
        

        artifactTags = GameObject.FindGameObjectWithTag("Tags");
        tags = artifactTags.GetComponent<ArtifactTags>();

        //assigning public variables to components
        characterController = GetComponent<CharacterController>();
        bob = cam.GetComponent<Animator>();
        stepSource = this.GetComponent<AudioSource>();

        //locking cursor into the center of the screen and making it invisible
        //Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

        stepI = stepInterval;
    }


    void Update()
    {
        //Script controlling character camera rotation
        Look();

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        { 
            //Script controlling character movement
            if(!tags.display)
                Movement();


            if (grabbing)
            {
                grabPos.GetComponent<bobberScript>().enabled = true;  //start bobbing the grab anchor when holding something

                if (moving)
                {
                    ///grabbedItem.ps.Stop();  //stop and disconnect UI particle effect when moving
                    grabbedItem.particlesFollowPlayer = false;
                    ///colourShift(loreBacker, null, new Color(1, 1, 1, 0), backFadeOutSpeed, false, false);    //fade backer colour in
                    colourShift(hex0, null, new Color(1, .9f, .9f, 0), backFadeOutSpeed, false, false);    //fade hexs colour in
                    colourShift(hex1, null, new Color(1, .9f, .9f, 0), backFadeOutSpeed, false, false);
                    colourShift(null, headerText, new Color(1, 1, 1, 0), headFadeOutSpeed, true, false);    //fade header colour in
                    colourShift(null, bodyText, new Color(1, 1, 1, 0), bodyFadeOutSpeed, true, true);      //fade body text in
                }
                else if (!moving)
                {
                    ///grabbedItem.ps.Play();  //plays UI particle effect when held and not moving
                    grabbedItem.particlesFollowPlayer = true;
                    ///colourShift(loreBacker, null, new Color(1, 1, 1, .6f), backFadeInSpeed, false, false);    //fade backer colour in
                    colourShift(hex0, null, new Color(1, .9f, .9f, .7f), backFadeInSpeed, false, false);    //fade hexs colour in
                    colourShift(hex1, null, new Color(1, .9f, .9f, .7f), backFadeInSpeed, false, false);
                    colourShift(null, headerText, Color.white, headFadeInSpeed, true, false);    //fade header colour in
                    colourShift(null, bodyText, Color.white, bodyFadeInSpeed * .25f, true, true);      //fade body text in
                }
            }
            else
            {
                grabPos.GetComponent<bobberScript>().enabled = false;  //stop bobbing the grab anchor when not holding anything
                                                                       ///colourShift(loreBacker, null, new Color (1, 1, 1, 0), backFadeOutSpeed, false, false);    //fade backer colour in
                colourShift(hex0, null, new Color(1, .9f, .9f, 0), backFadeOutSpeed, false, false);    //fade hexs colour in
                colourShift(hex1, null, new Color(1, .9f, .9f, 0), backFadeOutSpeed, false, false);
                colourShift(null, headerText, new Color(1, 1, 1, 0), headFadeOutSpeed, true, false);    //fade header colour in
                colourShift(null, bodyText, new Color(1, 1, 1, 0), bodyFadeOutSpeed, true, true);      //fade body text in
            }

            //Script
            checkGround();

            //Script checking mouse clicks to pick up objects
            PickUp();
        }
        else if(SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0))
        {
            
        }
    }

    public Sun_Rotation sunRotator;

    void DayTransition(bool boo)
    {
        sunRotator.goToNight = boo;
    }

    void dropItem()
    {
        grabbedObj.transform.SetParent(null);
        if (grabbedObj.GetComponent<MeshCollider>() != null)
            grabbedObj.GetComponent<MeshCollider>().enabled = true;
        if (grabbedObj.GetComponent<BoxCollider>() != null)
            grabbedObj.GetComponent<BoxCollider>().enabled = true;
        grabbedItem.setGrabbed(false);
        grabbing = false;
        grabbedItem.setInHand(false);
        DontDestroyOnLoad(grabbedObj);
    }
    void PickUp()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(1))
        {
            if (!grabbing && tags.tagged.Count > 0)
            {
                if (tags.display == true)
                {
                    tags.display = false;
                }
                else
                {
                    tags.display = true;
                }
            }
            if (grabbing && grabbedItem.getInHand())
            {
                dropItem();
                tags.addTag(grabbedObj);  
            }
        }
            if (Input.GetMouseButtonDown(0))
        {
            if (!grabbing)
            {
                if (Physics.Raycast(ray, out hit, grabDist, 1 << LayerMask.NameToLayer("Default")))
                {
                    if (hit.collider.gameObject.tag == "Fire")
                    {
                        DayTransition(true);
                    }
                        if (hit.collider.gameObject.GetComponent<grabbable>() != null && sunRotator.goToNight == false)
                    {
                        grabbedObj = hit.collider.gameObject;
                        grabbedObj.transform.SetParent(grabPos.transform);

                        if(grabbedObj.GetComponent<MeshCollider>() != null)
                            grabbedObj.GetComponent<MeshCollider>().enabled = false;

                        if (grabbedObj.GetComponent<BoxCollider>() != null)
                            grabbedObj.GetComponent<BoxCollider>().enabled = false;

                        grabbedItem = grabbedObj.GetComponent<grabbable>();
                        grabbedItem.setGrabbed(true);
                        grabbedItem.setGrabObj(grabPos);
                        grabbedItem.setUIAnchor(UIAnchor);
                        grabbedItem.setGrabSpeed(grabSpeed);
                        grabbing = true;
                        targetAngle = verticalLook.localRotation.x;
                        tags.tagged.Remove(grabbedObj);

                        bodyText.text = grabbedItem.lore;
                        headerText.text = grabbedObj.name;

                        if (tags.display)
                            tags.display = false;
                    }
                }
            }
            else
            {
                if (grabbedItem.getInHand())
                {
                    dropItem();                 
                }
            }
        }
    }

    void Look()
    {
        currentX += Input.GetAxis("Mouse X") * sensitivityX;
        currentY += Input.GetAxis("Mouse Y") * sensitivityY;

        if (currentY > 90f)
        {
            currentY = 90f;
        }
        else if (currentY < -90f)
        {
            currentY = -90f;
        }

		verticalLook.localRotation = Quaternion.Euler(-currentY, 0, 0);
        transform.rotation = Quaternion.Euler(0, currentX, 0);

        if (!tags.display)
        {
            artifactTags.transform.position = transform.position;
            artifactTags.transform.rotation = transform.rotation;
        }
    }

    void Movement()
    {
        if (characterController.isGrounded)
        {
            // We are grounded, so recalculate
            // move direction directly from axes

            moveDirection = Vector3.zero;
            if (Input.GetKey(forward))
            {
                moveDirection += Vector3.forward;
            }
            if (Input.GetKey(back))
            {
                moveDirection += Vector3.back;
            }
            if (Input.GetKey(left))
            {
                moveDirection += Vector3.left;
            }
            if (Input.GetKey(right))
            {
                moveDirection += Vector3.right;
            }
            moveDirection.Normalize();
            moveDirection *= speed;

            //Rotation with look direction
            moveDirection = transform.rotation * moveDirection;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        if(Mathf.Abs(moveDirection.x) < .1f && Mathf.Abs(moveDirection.z) < .1f)
        {
            moving = false;
            bob.enabled = false;

            //when you stop moving, reset the footstep timer for step sfx
            //stepI = stepInterval;
        }
        else
        {
            moving = true;
            bob.enabled = true;

            //count down for step audio
            stepCount();
        }
    }

    void stepCount ()
    {
        stepI -= Time.deltaTime;

        if (stepI <= 0) {
            stepI = stepInterval;
            
            if (onSand)
            {
                stepSource.PlayOneShot(sandSteps[Random.Range(0, sandSteps.Length)]);       //if on sand, play a random sand footstep sfx
            }

            else if (onWood)
            {
                stepSource.PlayOneShot(woodSteps[Random.Range(0, woodSteps.Length)]);       //if on wood, play a random wood footstep sfx
            }
        }
    }

    void checkGround ()
    {
        Vector3 downDirection = new Vector3(0, -1, 0);
        RaycastHit hit;
        float mag = 1.25f;

        Debug.DrawRay(transform.position, downDirection, Color.green);

        if (Physics.Raycast(transform.position, downDirection, out hit, mag))   //CHECK to see if player is currently standing on sand or wood
        {
            if (hit.collider.gameObject.tag == "Sand")
            {
                onSand = true;
                onWood = false;
            }

            else if (hit.collider.gameObject.tag == "Wood")
            {
                onSand = false;
                onWood = true;
            }
        }
        else
        {
            onSand = false;
            onWood = false;
        }
    }

    void colourShift (Image curImg, Text curText, Vector4 target, float speed, bool isText, bool isBody)
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
