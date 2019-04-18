using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCharacter : MonoBehaviour
{
    CharacterController characterController;
    public Camera cam;
    Animator bob;

    public float speed = 6.0f;
    //public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;

    public KeyCode forward;
    public KeyCode back;
    public KeyCode left;
    public KeyCode right;
    public KeyCode jump;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        bob = cam.GetComponent<Animator>();
    }

    void Update()
    {
        Movement();
        Look();
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
        if(Mathf.Abs(moveDirection.x) < .1f && Mathf.Abs(moveDirection.z) < .1f)
        {
            bob.enabled = false;
            //bob.Play("Camera_Head_Bob");
        }
        else
        {
            bob.enabled = true;
        }
    }
}
