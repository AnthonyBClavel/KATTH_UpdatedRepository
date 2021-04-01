using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : MonoBehaviour
{
    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);
    private Vector3 originalRotation;

    private Animator characterAnim;

    private TileMovementController playerScript;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        characterAnim = GetComponent<Animator>();
        originalRotation = gameObject.transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            PlayGreetingAnim();
        }
    }

    // Rotates the npc so that it faces the player
    public void SetRotationNPC()
    {
        if (playerScript.playerDirection == up)
            gameObject.transform.localEulerAngles = down;
        if (playerScript.playerDirection == left)
            gameObject.transform.localEulerAngles = right;
        if (playerScript.playerDirection == down)
            gameObject.transform.localEulerAngles = up;
        if (playerScript.playerDirection == right)
            gameObject.transform.localEulerAngles = left;
    }

    // Sets the game object (npc) back to its original position
    public void ResetRotationNPC()
    {
        gameObject.transform.localEulerAngles = originalRotation;
    }

    // Triggers the character's greeting animation;
    public void PlayGreetingAnim()
    {
        characterAnim.SetTrigger("Greet");
    }


}
