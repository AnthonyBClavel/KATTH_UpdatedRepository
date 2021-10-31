using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class CameraController : MonoBehaviour
{
    private float cameraSpeed; // 3f
    private int cameraIndex;
    private float cameraLerpLength = 4f;
    private float valueToSubractZ;
    private float rayLength = 1f;

    private GameObject northDialogueView;
    private GameObject eastDialogueView;
    private GameObject southDialogueView;
    private GameObject westDialogueView;

    //[SerializeField]
    private Vector3[] puzzleViews;
    private GameObject[] checkpoints;

    private Vector3 currentDialogueView;
    private Vector3 originalCameraRotation;
    private Vector3 currentView;

    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);

    private GameHUD gameHUDScript;
    private TileMovementController playerScript;
    private GameManager gameManagerScript;
    private AudioManager audioManagerScript;
    private NotificationBubbles notificationBubblesScript;
    private PuzzleManager puzzleManagerScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalCameraRotation = transform.eulerAngles;
        gameHUDScript.UpdatePuzzleBubbleText((cameraIndex + 1) + "/" + checkpoints.Length);
    }

    // Returns or sets the cameraSpeed
    public float CameraSpeed
    {
        get
        {
            return cameraSpeed;
        }
        set
        {
            cameraSpeed = value;
        }
    }

    // Returns or sets the cameraIndex
    public int CameraIndex
    {
        get
        {
            return cameraIndex;
        }
        set
        {
            cameraIndex = value;
        }
    }

    // Sets the camera's initial position, determined by currentIndex - For save manager script ONLY
    public void SetCameraPosition()
    {
        if (transform.position != puzzleViews[cameraIndex])
            transform.position = puzzleViews[cameraIndex];
    }

    // Moves the camera to the next puzzle view
    public void NextPuzzleView()
    {
        int nextPuzzleIndex = playerScript.PuzzleNumber;
        cameraIndex++;

        if (cameraIndex > puzzleViews.Length - 1)
            cameraIndex = 0;

        // Sets the "correct" next puzzle view if applicable
        if (cameraIndex != nextPuzzleIndex)
            cameraIndex = nextPuzzleIndex;

        // Only lerps the camera if its not already at the next puzzle view
        if (currentView != puzzleViews[cameraIndex])
        {
            //Debug.Log("Moved to next puzzle view");
            audioManagerScript.ChangeLoopingAmbienceSFX();
            audioManagerScript.PlayWindGushSFX();
            currentView = puzzleViews[cameraIndex];
            StopAllCoroutines();
            StartCoroutine(LerpCamera(currentView));
        }

        puzzleManagerScript.ResetGeneratorCheck();
        SetPuzzleNumber(); // Must be called last
    }

    // Moves the camera to the previous puzzle view
    public void PreviousPuzzleView()
    {
        int previousPuzzleIndex = playerScript.PuzzleNumber - 2;
        cameraIndex--;

        if (cameraIndex < 0)
            cameraIndex = puzzleViews.Length - 1;

        // Sets the "correct" previous puzzle view if applicable
        if (cameraIndex != previousPuzzleIndex)
            cameraIndex = previousPuzzleIndex;

        // Only lerps the camera if its not already at the previous puzzle view
        if (currentView != puzzleViews[cameraIndex])
        {
            //Debug.Log("Moved to next puzzle view");
            audioManagerScript.ChangeLoopingAmbienceSFX();
            audioManagerScript.PlayWindGushSFX();
            currentView = puzzleViews[cameraIndex];
            StopAllCoroutines();
            StartCoroutine(LerpCamera(currentView));
        }

        puzzleManagerScript.ResetGeneratorCheck();
        SetPuzzleNumber(); // Must be called last
    }

    // Sets the camera to the current dialogue view
    public void SetCameraToCurrentDialogueView()
    {
        transform.position = currentDialogueView;
        transform.eulerAngles = originalCameraRotation;
    }

    // Lerps the camera's position to the current puzzle view
    public void LerpCameraToCurrentPuzzleView()
    {
        currentView = puzzleViews[cameraIndex];
        StopAllCoroutines();
        StartCoroutine(LerpCamera(currentView));
    }

    // Checks which dialogue view the camera should move to - for character/artifact dialogue
    public void LerpCameraToDialogueView()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        if (playerDirection == up)
            currentDialogueView = southDialogueView.transform.position;

        else if (playerDirection == right)
            currentDialogueView = westDialogueView.transform.position;

        else if (playerDirection == down)
            currentDialogueView = northDialogueView.transform.position;

        else if (playerDirection == left)
            currentDialogueView = eastDialogueView.transform.position;

        StopAllCoroutines();
        StartCoroutine(LerpCamera(currentDialogueView));
    }

    // Lerps the camera to the next puzzle view
    private void LerpCameraToNextPuzzleView()
    {
        cameraIndex++;

        if (cameraIndex > puzzleViews.Length - 1)
            cameraIndex = 0;

        currentView = puzzleViews[cameraIndex];
        StopAllCoroutines();
        StartCoroutine(LerpCamera(currentView));
    }

    // Lerps the camera to the previous puzzle view
    private void LerpCameraToPreviousPuzzleView()
    {
        cameraIndex--;

        if (cameraIndex < 0)
            cameraIndex = puzzleViews.Length - 1;

        currentView = puzzleViews[cameraIndex];
        StopAllCoroutines();
        StartCoroutine(LerpCamera(currentView));
    }

    // Creates an array of puzzle views - one for each puzzle
    private void SetPuzzleViews()
    {
        List<Vector3> cameraViews = new List<Vector3>();

        for (int i = 0; i < checkpoints.Length; i++)
        {
            GameObject checkpoint = checkpoints[i];
            GameObject tileHolder = checkpoint.transform.parent.gameObject;
            List<float> xValues = new List<float>();
            List<float> zValues = new List<float>();

            // Find the center of a puzzle by going through all of its tiles and finding the highest/lowest x and z values
            for (int h = 0; h < tileHolder.transform.childCount; h++)
            {
                GameObject tileBlock = tileHolder.transform.GetChild(h).gameObject;
                float tileBlockPosX = tileBlock.transform.position.x;
                float tileBlockPosZ = tileBlock.transform.position.z;

                // Checks for another tile block to the north/south - returns true if so, false otherwise
                if (TileEdgeCheck(tileBlock, 0))
                    xValues.Add(tileBlockPosX); // Adds x position to list if so

                // Checks for another tile block to the east/west - returns true if so, false otherwise
                if (TileEdgeCheck(tileBlock, 90))
                    zValues.Add(tileBlockPosZ); // Adds z position to list if so       
            }

            // Sets arrays equal to the corresponding lists, and then sorts it from lowest to highest
            float[] xPositions = xValues.ToArray();
            Array.Sort(xPositions);
            float[] zPositions = zValues.ToArray();
            Array.Sort(zPositions);

            // Sets the highest/lowest x and z positions
            float lowestPosX = xPositions[0];
            float lowestPosZ = zPositions[0];
            float highestPosX = xPositions[xPositions.Length - 1];
            float highestPosZ = zPositions[zPositions.Length - 1];

            // Sets the length and width of the puzzle (Note: ignores the single tile blocks on the edges of puzzles - determined by TileEdgeCheck())
            float puzzleLength = highestPosX - lowestPosX;
            float puzzleWidth = highestPosZ - lowestPosZ;

            if (puzzleWidth <= 3f) // If the puzzle width is 4 blocks long (or less)
                valueToSubractZ = 5f;
            else if (puzzleWidth <= 4f && puzzleWidth > 3f) // If the puzzle width is 5 blocks long
                valueToSubractZ = 5.25f;
            else if (puzzleWidth <= 5f && puzzleWidth > 4f) // If the puzzle width is 6 blocks long
                valueToSubractZ = 5.5f;
            else if (puzzleWidth > 5f) // If the puzzle width is 7 blocks long (or greater)
                valueToSubractZ = 6f; // 5.75f

            float xPos = lowestPosX + (puzzleLength / 2f); // X position for next puzzle view
            float zPos = lowestPosZ + (puzzleWidth / 2f) - valueToSubractZ; // Z position for next puzzle view
            float yPos = 7f; // Y position for next puzzle view

            Vector3 puzzleView = new Vector3(xPos, yPos, zPos);
            cameraViews.Add(puzzleView);
        }

        puzzleViews = cameraViews.ToArray();
    }

    // Checks for other tile blocks along the same axis (set intialRayDirection to 0 to check north/south of block, 90 to check east/west)
    private bool TileEdgeCheck(GameObject tileBlock, int intialRayDirection)
    {
        for (int i = 0; i < tileBlock.transform.childCount; i++)
        {
            GameObject child = tileBlock.transform.GetChild(i).gameObject;

            if (child.name == "EdgeCheck")
            {
                GameObject tileEdgeCheck = child;

                for (int j = intialRayDirection; j <= 270; j += 180)
                {
                    //Debug.Log("Checked for tile block at direction " + intialRayDirection);
                    tileEdgeCheck.transform.localEulerAngles = new Vector3(0, intialRayDirection, 0);

                    Ray myRay = new Ray(tileEdgeCheck.transform.position + new Vector3(0, -0.5f, 0), tileEdgeCheck.transform.TransformDirection(Vector3.forward));
                    RaycastHit hit;
                    Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

                    if (Physics.Raycast(myRay, out hit, rayLength))
                    {
                        if (hit.collider == true)
                        {
                            //Debug.Log("Found tile block at direction " + intialRayDirection);
                            return true;
                        }
                    }
                    intialRayDirection += 180;
                }
            }            
        }
        //Debug.Log("No tile block found");
        return false;
    }

    // Updates and checks to play the puzzle notification bubble
    private void SetPuzzleNumber()
    {
        gameHUDScript.UpdatePuzzleBubbleText((cameraIndex + 1) + "/" + checkpoints.Length);
        notificationBubblesScript.PlayPuzzleNotificationCheck();
    }

    // Lerps the position of the camera to a new position (endPosition = position to lerp to)
    private IEnumerator LerpCamera(Vector3 endPosition)
    {
        gameManagerScript.CheckForCameraScriptDebug();
        float duration = cameraLerpLength;
        float time = 0;

        // When the camera is approximately equal to the next position
        // Note: The transform.position in lerp will always get closer to endPosition, but never equal it, so the coroutine would endlessly play
        while (time < duration)
        {
            transform.position = Vector3.Lerp(transform.position, endPosition, cameraSpeed * Time.deltaTime);
            time += Time.deltaTime;
            yield return null;
        }

        //Debug.Log("Finished lerping camera");
        transform.position = endPosition;
    }

    // Sets the puzzle views array and camera transition speed
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            GameObject child = transform.parent.GetChild(i).gameObject;

            if (child.name == "DialogueViewsHolder")
            {
                GameObject dialogueViewsHolder = child;

                for (int j = 0; j < dialogueViewsHolder.transform.childCount; j++)
                {
                    GameObject child02 = dialogueViewsHolder.transform.GetChild(j).gameObject;

                    if (child02.name == "NorthDV")
                        northDialogueView = child02;
                    if (child02.name == "EastDV")
                        eastDialogueView = child02;
                    if (child02.name == "SouthDV")
                        southDialogueView = child02;
                    if (child02.name == "WestDV")
                        westDialogueView = child02;
                }
            }
        }

        cameraSpeed = gameManagerScript.cameraSpeed;
        checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        SetPuzzleViews();
    }

    // Determines which scripts to find
    private void SetScripts()
    {
        gameHUDScript = FindObjectOfType<GameHUD>();
        playerScript = FindObjectOfType<TileMovementController>();
        gameManagerScript = FindObjectOfType<GameManager>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
    }

    // Enables debugging for the puzzle views and cameraSpeed - For Debugging Purposes ONLY
    public void DebuggingCheck(GameManager gameManager)
    {
        if (gameManager.isDebugging)
        {
            if (Input.GetKeyDown(KeyCode.Equals)) // =
            {
                audioManagerScript.ChangeLoopingAmbienceSFX();
                audioManagerScript.PlayWindGushSFX();
                LerpCameraToNextPuzzleView();
                StopAllCoroutines();
                StartCoroutine(LerpCamera(currentView));
                gameHUDScript.UpdatePuzzleBubbleText((cameraIndex + 1) + "/" + checkpoints.Length);
                Debug.Log("Debugging: Moved Camera To Puzzle " + (cameraIndex + 1));
            }

            if (Input.GetKeyDown(KeyCode.Minus)) // -
            {
                audioManagerScript.ChangeLoopingAmbienceSFX();
                audioManagerScript.PlayWindGushSFX();
                LerpCameraToPreviousPuzzleView();
                StopAllCoroutines();
                StartCoroutine(LerpCamera(currentView));
                gameHUDScript.UpdatePuzzleBubbleText((cameraIndex + 1) + "/" + checkpoints.Length);
                Debug.Log("Debugging: Moved Camera To Puzzle " + (cameraIndex + 1));
            }
        }
    }

    // Lerps the position of the camera to a new position (endPosition = position to lerp to, duration = seconds) - FOR REFERENCE
    /*private IEnumerator LerpCamera(Vector3 endPosition, float duration)
    {
        gameManagerScript.CheckForCameraScriptDebug();
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
    }*/

    // Sets the current/next puzzle view - OLD VERSION
    /*private void SetCurrentView()
    {
        currentCheckpoint = checkpoints[cameraIndex];
        currentTileHolder = currentCheckpoint.transform.parent.gameObject;
        List<float> xValues = new List<float>();
        List<float> zValues = new List<float>();

        // Find the center of a puzzle by going through all of its tiles and finding the highest/lowest x and z values
        for (int i = 0; i < currentTileHolder.transform.childCount; i++)
        {
            GameObject tileBlock = currentTileHolder.transform.GetChild(i).gameObject;
            float tileBlockPosX = tileBlock.transform.position.x;
            float tileBlockPosZ = tileBlock.transform.position.z;

            // Checks for another tile block to the north/south - returns true if so, false otherwise
            if (TileEdgeCheck(tileBlock, 0))
                xValues.Add(tileBlockPosX); // Adds x position to list if so

            // Checks for another tile block to the east/west - returns true if so, false otherwise
            if (TileEdgeCheck(tileBlock, 90))
                zValues.Add(tileBlockPosZ); // Adds z position to list if so       
        }

        // Sets the array equal to the corresponding list, and then sorts it
        xPositions = xValues.ToArray();
        Array.Sort(xPositions);
        zPositions = zValues.ToArray();
        Array.Sort(zPositions);

        // Sets the highest/lowest x and z positions
        float lowestPosX = xPositions[0];
        float lowestPosZ = zPositions[0];
        float highestPosX = xPositions[xPositions.Length - 1];
        float highestPosZ = zPositions[zPositions.Length - 1];

        // Find the length and width of the puzzle (Note: ignores the single tile blocks on the edges of puzzles - determined by TileEdgeCheck())
        puzzleLength = highestPosX - lowestPosX;
        puzzleWidth = highestPosZ - lowestPosZ;

        if (puzzleWidth <= 3f) // If the puzzle width is 4 blocks long (or less)
            valueToSubractZ = 5f;
        else if (puzzleWidth <= 4f && puzzleWidth > 3f) // If the puzzle width is 5 blocks long
            valueToSubractZ = 5.25f;
        else if (puzzleWidth > 4f) // If the puzzle width is 6 blocks long (or greater)
            valueToSubractZ = 5.5f;

        float xPos = lowestPosX + (puzzleLength / 2f); // X position for next puzzle view
        float zPos = lowestPosZ + (puzzleWidth / 2f) - valueToSubractZ; // Z position for next puzzle view
        float yPos = 7f; // Y position for next puzzle view

        currentView = new Vector3(xPos, yPos, zPos);
    }*/

    // Lerps the position of the camera to a new position (endPosition = position to lerp to) - OLD VERSION
    /*private IEnumerator LerpCamera(Vector3 endPosition)
    {
        gameManagerScript.CheckForCameraScriptDebug();

        // When the camera is approximately equal to the next position
        // Note: The transform.position in lerp will always get closer to endPosition, but never equal it, so the coroutine would endlessly play

        // If the next x position is the same as the current x position
        if (transform.position.x == endPosition.x)
        {
            Debug.Log("Lepred x axis only");
            while (Mathf.Abs(transform.position.z - endPosition.z) > 0.0001f)
            {
                transform.position = Vector3.Lerp(transform.position, endPosition, cameraSpeed * Time.deltaTime);
                yield return null;
            }
        }
        // If the next z position is the same as the current z position
        else if (transform.position.z == endPosition.z)
        {
            Debug.Log("Lepred z axis only");
            while (Mathf.Abs(transform.position.x - endPosition.x) > 0.0001f)
            {
                transform.position = Vector3.Lerp(transform.position, endPosition, cameraSpeed * Time.deltaTime);
                yield return null;
            }
        }
        // If niether the x or z position is the same
        else
        {
            Debug.Log("Lepred both axis");
            while (Mathf.Abs(transform.position.x - endPosition.x) > 0.0001f && Mathf.Abs(transform.position.z - endPosition.z) > 0.0001f)
            {
                transform.position = Vector3.Lerp(transform.position, endPosition, cameraSpeed * Time.deltaTime);
                yield return null;
            }
        }

        Debug.Log("Finished Lerping Camera");
        transform.position = endPosition;
    }*/

}
