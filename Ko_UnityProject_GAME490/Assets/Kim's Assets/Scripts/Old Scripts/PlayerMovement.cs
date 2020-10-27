using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    Vector2 input; // Holds horizontal and vertical values of keyboard input
    bool hasMoved; // Checks if the player has moved already or not
    Vector3 direction;

    public float velocity = 5; // Movement speed
    public float turnSpeed = 10; // Turn speed I guess

    public LayerMask ground; // Ground layer
    public float maxGroundAngle = 120;

    public float height = 0.5f; // ?
    public float heightPadding = 0.05f; // ?

    float angle;
    bool grounded;
    float groundAngle;

    Vector3 forward;
    RaycastHit hitInfo;
    Quaternion targetRotation;
    Transform cam;
    public bool debug;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        GetInput(); //Input is based on Horizontal (a,d,<,>) and Vertcial (w,s,^,v) keys
    }

    void GetInput() // Input is based on Horizontal (a,d,<,>) and Vertcial (w,s,^,v) keys
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        
        // If there's no input
        if (input.x == 0 && input.y == 0)
        {
            hasMoved = false;
        }

        // If there is an input
        else if ((input.x != 0 || input.y != 0) && !hasMoved)
        {
            hasMoved = true;
            CalculateDirection(); //Direction relative to the camera's rotation
            CalculateForward(); //If the player is not grounded, forward will be equal to transfrom forward
            CalculateGroundAngle();
            CheckGround();
            ApplyGravity();
            DrawDebugLines();

            // If there's no input from the character controller
            if (Mathf.Abs(input.x) < 1 && Mathf.Abs(input.y) < 1)
            {
                return;
            }

            Rotate(); //Rotate twoards the calcualated angle
            Move(); //Player only moves along it own foward axis
        }
    }

    void CalculateDirection() // Direction relative to the camera's rotation
    {
        /**
        angle = Mathf.Atan2(input.x, input.y);
        angle = Mathf.Rad2Deg * angle;
        angle += cam.eulerAngles.y;
        **/

        if (input.x < 0)
        {
            direction = new Vector3(-1,0);
        }

        else if (input.x > 0)
        {
            direction = new Vector3(1, 0);
        }

        else if (input.y < 0)
        {
            direction = new Vector3(0, 0,-1);
        }

        else if (input.y > 0)
        {
            direction = new Vector3(0, 0, 1);
        }
    }

    void Rotate() //Rotate twoards the calcualated angle (Camera is linear so it shouldn't cause a lot of issues)
    {
        targetRotation = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    void Move() //Player only moves along it own foward axis
    {
        if (groundAngle >= maxGroundAngle)
        {
            return;
        }

        //transform.position += forward * velocity * Time.deltaTime;
        transform.position += direction;
    }

    void CalculateForward() //If the player is not grounded, forward will be equal to transfrom forward
    {
        if (!grounded)
        {
            forward = transform.forward;
            return;
        }

        forward = Vector3.Cross(transform.right, hitInfo.normal);
    }

    void CalculateGroundAngle()
    {
        if (!grounded)
        {
            groundAngle = 90;
            return;
        }

        groundAngle = Vector3.Angle(hitInfo.normal, transform.forward);
    }


    void CheckGround()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, height + heightPadding, ground))
        {
            if (Vector3.Distance(transform.position, hitInfo.point) < height)
            {
                transform.position = Vector3.Lerp(transform.position, transform.position + Vector3.up * height, 5 * Time.deltaTime);
            }
            grounded = true;
        }
        else
        {
            grounded = false;
        }
    }

    void ApplyGravity()
    {
        if (!grounded)
        {
            transform.position += Physics.gravity * Time.deltaTime;
        }
    }

    void DrawDebugLines()
    {
        if (!debug)
        {
            return;
        }

        Debug.DrawLine(transform.position, transform.position + forward * height * 2, Color.blue);
        Debug.DrawLine(transform.position, transform.position - Vector3.up * height, Color.green);
    }

}
