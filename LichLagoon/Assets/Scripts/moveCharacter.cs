using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCharacter : MonoBehaviour
{
    //The component that helps us control the character's motion
    CharacterController characterController;

    //The camera attatched to the player
    public Camera cam;

    //The animator that adds the walking head bob
    Animator bob;

	[Header ("Mod Values")]
    //movement speed
    public float speed = 6.0f;

    //the gravity applied to the player
    public float gravity = 20.0f;

    //the direction the player is moving in
    private Vector3 moveDirection = Vector3.zero;

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
        //assigning public variables to components
        characterController = GetComponent<CharacterController>();
        bob = cam.GetComponent<Animator>();
        stepSource = this.GetComponent<AudioSource>();

        //locking cursor into the center of the screen and making it invisible
        Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

        stepI = stepInterval;   
    }

    void Update()
    {
        //Script controlling character movement
        Movement();

        //Script controlling character camera rotation
        Look();

        //Script
        checkGround();
    }

    //mouse sensitivity
    public float sensitivityX;
    public float sensitivityY;
    private float currentX, currentY;
    public Transform verticalLook;

    void Look()
    {
        currentX += Input.GetAxis("Mouse X") * sensitivityX;
        currentY += Input.GetAxis("Mouse Y") * sensitivityY;

		verticalLook.localRotation = Quaternion.Euler(-currentY, 0, 0);
        transform.rotation = Quaternion.Euler(0, currentX, 0);
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
            moving = true;
            bob.enabled = false;

            //when you stop moving, reset the footstep timer for step sfx
            //stepI = stepInterval;
        }
        else
        {
            moving = false;
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
}
