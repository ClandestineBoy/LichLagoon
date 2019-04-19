using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCharacter : MonoBehaviour
{
    CharacterController characterController;
    public Camera cam;
    Animator bob;

	[Header ("Mod Values")]
    public float speed = 6.0f;
    //public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;

	[Header ("KeyInputs")]
    public KeyCode forward;
    public KeyCode back;
    public KeyCode left;
    public KeyCode right;
    public KeyCode jump;

    [Header("Audio")]
    public float stepInterval = 0;
    private float stepI = 0;
    private AudioSource stepSource;
    public AudioClip[] sandSteps = new AudioClip[6];
    public AudioClip[] woodSteps = new AudioClip[6];
    private bool moving = false, onWood = false, onSand = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        bob = cam.GetComponent<Animator>();

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

        stepI = stepInterval;
        stepSource = this.GetComponent<AudioSource>();
    }

    void Update()
    {
        Movement();
        Look();
        checkGround();
    }

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
        

       // player.rotation = Quaternion.Slerp(player.rotation, , Time.deltaTime * 10);
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
           /* if (Input.GetKey(jump))
            {
                moveDirection.y = jumpSpeed;
            }*/
            //Rotation with look direction
            moveDirection = transform.rotation * moveDirection;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        if(Mathf.Abs(moveDirection.x) < .1f && Mathf.Abs(moveDirection.z) < .1f)    //if not moving
        {
            bob.enabled = false;
            //stepI = stepInterval;   //when you stop moving, reset the footstep timer for step sfx
            ///bob.Play("Camera_Head_Bob");
        }
        else     //if moving
        {
            bob.enabled = true;
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
