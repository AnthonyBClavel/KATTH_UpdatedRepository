using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowWorld : MonoBehaviour
{
    [Header("Tweaks")]
    public Transform lookAtObject;
    //public Vector3 offset;

    [Header("Camera")]
    public Camera cam;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        SetDialogueBubblePosition();
    }

    // Sets to the position of the dialogue bubble to the position of the lookAtObject
    private void SetDialogueBubblePosition()
    {
        Vector3 pos = cam.WorldToScreenPoint(lookAtObject.transform.position /* offset*/);

        if (transform.position != pos)
            transform.position = pos;
    }

}
