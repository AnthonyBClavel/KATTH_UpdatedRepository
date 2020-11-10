using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed, gravityModifier, runSpeed = 5f;
    public CharacterController charCon;

    private Vector3 moveInput;

    private bool isJogging;
    private bool isWalking;

    public Animator Anim;

    //public Transform groundCheckPoint;
    //public LayerMask whatIsGround;
    //public Transform camTrans;
    //public float jumpPower = 12f;
    //public float mouseSensitivity;
    //public bool invertX;
    //public bool invertY;
    //private bool canJump, canDoubleJump;
   
    public float turnSpeed = 10;
    Quaternion targetRotation;
    Vector3 forward;
    Transform cam;
    float angle;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {

        float yStore = moveInput.y;

        Vector3 vertMove = transform.forward * Input.GetAxisRaw("Vertical");
        Vector3 horiMove = transform.right * Input.GetAxisRaw("Horizontal");

        moveInput = horiMove + vertMove;
        moveInput.Normalize();

        
        angle = Mathf.Atan2(horiMove.x, vertMove.y);
        angle = Mathf.Rad2Deg * angle;
        angle += cam.eulerAngles.y;
        forward = Vector3.Cross(horiMove, vertMove);


        if (Input.GetKey(KeyCode.LeftShift) && isWalking)
        {
            moveInput = moveInput * runSpeed;
            isJogging = true;
        }
        else
        {
            moveInput = moveInput * moveSpeed;
            isJogging = false;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }

        moveInput.y = yStore;

        moveInput.y += Physics.gravity.y * gravityModifier * Time.deltaTime;

        if (charCon.isGrounded)
        {
            moveInput.y = Physics.gravity.y * gravityModifier * Time.deltaTime;
        }

        charCon.Move(moveInput * Time.deltaTime);

        Anim.SetFloat("moveSpeed", moveInput.magnitude);
        Anim.SetBool("isJogging", isJogging);
        Anim.SetBool("isWalking", isWalking);

        //For Jumping and Double Jumping - Use the hashed out variables above to use the code below (if we decide to implement jumping)

        /* canJump = Physics.OverlapSphere(groundCheckPoint.position, 0.25f, whatIsGround).Length > 0;

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            moveInput.y = jumpPower;

            canDoubleJump = true;
        }
        else if (canDoubleJump && Input.GetKeyDown(KeyCode.Space))
        {
            moveInput.y = jumpPower;

            canDoubleJump = false;
        } 

        Anim.SetBool("onGround", canJump);  */

        
        targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        transform.position += forward * moveSpeed * Time.deltaTime;
    }


}
