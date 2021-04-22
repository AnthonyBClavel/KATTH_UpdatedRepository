using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactScript : MonoBehaviour
{
    [Header("Bools")]
    public bool canRotateArtifact = false;
    public bool isInspectingArtifact = false;
    private bool hasSetNewCameraView = false;
    private bool canTransitionFade = true;

    public bool canInspectArtifact = false;
    [Header("Transforms")]
    public Transform pixelatedCamera;
    public Transform artifactHolder;
    public Transform artifactTransform;
    private Transform playerTransform;

    [Header("Variables")]
    public string nPCName;
    public float rotationSpeedWithKeys;
    public float rotationSpeedWithMouse;
    private float horizontalAxis;
    private float verticalAxis;
    private int lastArtifactIndex;

    [Header("Artifact Dialogue Array")]
    public TextAsset[] artifactDialogueFiles;

    private Vector3 originalArtifactHolderRotation;
    private Vector3 originalArtifactRotation;
    private Vector3 inspectingArtifactRotation;
    private Vector3 cameraOrigPosition;
    private Vector3 cameraOrigrotation;

    private CameraController cameraScript;
    private CharacterDialogue characterDialogueScript;
    private PauseMenu pauseMenuScript;

    void Awake()
    {
        cameraScript = FindObjectOfType<CameraController>();
        playerTransform = FindObjectOfType<TileMovementController>().gameObject.transform;
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalArtifactHolderRotation = artifactHolder.eulerAngles;

        originalArtifactRotation = artifactTransform.localEulerAngles;
        inspectingArtifactRotation = new Vector3(-20, -180, 0);
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isInspectingArtifact)
            {
                SetInspectingRotation();
            }
            else
            {
                SetDefaultRotation();
            }

            //transform.rotation = Quaternion.identity;
        }

        if (Input.GetKeyDown(KeyCode.P) && canTransitionFade)
        {
            canInspectArtifact = !canInspectArtifact;
            StartCoroutine("InspectArtifact");          
            canTransitionFade = false;
        }

    }

    void FixedUpdate()
    {
        /*if (Input.GetKey(KeyCode.W))
            artifact.eulerAngles = new Vector3(artifact.eulerAngles.x + rotationSpeed, artifact.eulerAngles.y, 0);
        if (Input.GetKey(KeyCode.S))
            artifact.eulerAngles = new Vector3(artifact.eulerAngles.x - rotationSpeed, artifact.eulerAngles.y, 0);
        if (Input.GetKey(KeyCode.A))
            artifact.eulerAngles = new Vector3(artifact.eulerAngles.x, artifact.eulerAngles.y + rotationSpeed, 0);
        if (Input.GetKey(KeyCode.D))
            artifact.eulerAngles = new Vector3(artifact.eulerAngles.x, artifact.eulerAngles.y - rotationSpeed, 0);*/

        //float horizontalAxis = Input.GetAxis("Horizontal");
        //float verticalAxis = Input.GetAxis("Vertical");
        //Vector3 currEulerAngles = transform.localEulerAngles;

        //currEulerAngles.y += -horizontalAxis * rotationSpeed * Time.deltaTime;
        //currEulerAngles.x += verticalAxis * rotationSpeed * Time.deltaTime;

        //transform.localRotation = Quaternion.Euler(currEulerAngles);

        MoveObjectWithKeysCheck();
        MoveObjectWithMouseCheck();
    }

    // Sets the new set of dialogue for the artifact
    public void SetArtifactDialogue()
    {
        int attempts = 3;
        int newArtifactIndex = UnityEngine.Random.Range(0, artifactDialogueFiles.Length);

        while (newArtifactIndex == lastArtifactIndex && attempts > 0)
        {
            newArtifactIndex = UnityEngine.Random.Range(0, artifactDialogueFiles.Length);
            attempts--;
        }

        lastArtifactIndex = newArtifactIndex;
        characterDialogueScript.setPlayerDialogue(artifactDialogueFiles[newArtifactIndex]);
    }

    // Sets the camera to look at the artifact up close
    public void SetNewCameraView()
    {
        if (!hasSetNewCameraView)
        {
            isInspectingArtifact = true;
            cameraScript.canMoveToArtifactView = true;

            cameraOrigPosition = pixelatedCamera.position;
            cameraOrigrotation = pixelatedCamera.eulerAngles;

            pixelatedCamera.position = playerTransform.localPosition + new Vector3(0, 2.5f, 0);
            pixelatedCamera.eulerAngles = new Vector3(pixelatedCamera.transform.eulerAngles.x, playerTransform.localEulerAngles.y, pixelatedCamera.transform.eulerAngles.z);

            SetInspectingRotation();
            canRotateArtifact = true;
            hasSetNewCameraView = true;
        }
    }
    
    // Sets the camera back to it's previous position and rotation
    public void ResetCameraView()
    {
        if (hasSetNewCameraView)
        {
            pixelatedCamera.position = cameraOrigPosition;
            pixelatedCamera.eulerAngles = cameraOrigrotation;

            characterDialogueScript.isInteractingWithArtifact = false;
            cameraScript.canMoveToArtifactView = false;
            isInspectingArtifact = false;

            SetDefaultRotation();
            canRotateArtifact = false;
            hasSetNewCameraView = false;
        }
    }

    // Rotates the artifact to look at the new camera view
    private void SetInspectingRotation()
    {
        artifactHolder.LookAt(pixelatedCamera);
        artifactTransform.localRotation = Quaternion.Euler(inspectingArtifactRotation);
    }

    // Resets the artifact to its default rotation
    private void SetDefaultRotation()
    {
        if (artifactHolder.eulerAngles != originalArtifactHolderRotation)
            artifactHolder.eulerAngles = originalArtifactHolderRotation;

        artifactTransform.localRotation = Quaternion.Euler(originalArtifactRotation);
    }

    // Checks to see if the artifact can move with the arrow keys
    private void MoveObjectWithKeysCheck()
    {
        if (canRotateArtifact && !Input.GetMouseButton(0))
        {
            horizontalAxis = Input.GetAxis("Horizontal") * rotationSpeedWithKeys * Time.deltaTime;
            verticalAxis = Input.GetAxis("Vertical") * rotationSpeedWithKeys * Time.deltaTime;

            artifactTransform.Rotate(Vector3.up, -horizontalAxis);
            artifactTransform.Rotate(Vector3.right, verticalAxis);
        }
    }

    // Checks to see if the artifact can move with the mouse
    private void MoveObjectWithMouseCheck()
    {
        if (canRotateArtifact && Input.GetMouseButton(0))
        {
            horizontalAxis = Input.GetAxis("Mouse X") * rotationSpeedWithMouse * Time.deltaTime;
            verticalAxis = Input.GetAxis("Mouse Y") * rotationSpeedWithMouse * Time.deltaTime;

            artifactTransform.Rotate(Vector3.up, -horizontalAxis);
            artifactTransform.Rotate(Vector3.right, verticalAxis);
        }
    }
    
    // Transitions the the close up camera view of the artifact
    private IEnumerator InspectArtifact()
    {
        characterDialogueScript.fadeTransition.SetActive(true);
        pauseMenuScript.canPause = false;
        yield return new WaitForSeconds(1f);

        if (canInspectArtifact)
            SetNewCameraView();
        else
            ResetCameraView();

        yield return new WaitForSeconds(1f);
        pauseMenuScript.canPause = true;
        characterDialogueScript.fadeTransition.SetActive(false);
        canTransitionFade = true;
    }

}
