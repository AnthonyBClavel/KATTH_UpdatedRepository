using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraController : MonoBehaviour
{
    private float cameraSpeed; // 3f
    private int puzzleViewIndex;
    private float cameraLerpLength = 4f;
    private float rayLength = 1f;

    private GameObject northDialogueView;
    private GameObject eastDialogueView;
    private GameObject southDialogueView;
    private GameObject westDialogueView;

    List<GameObject> checkpoints = new List<GameObject>();
    List<Vector3> puzzleViews = new List<Vector3>();
    List<float> xValues = new List<float>();
    List<float> zValues = new List<float>();
    List<int> neighboringTiles = new List<int>();

    private Vector3 currentDialogueView;
    private Vector3 originalCameraRotation;
    private Vector3 currentPuzzleView;

    private GameHUD gameHUDScript;
    private TileMovementController playerScript;
    private GameManager gameManagerScript;
    private AudioManager audioManagerScript;
    private NotificationBubbles notificationBubblesScript;
    private PuzzleManager puzzleManagerScript;
    private CharacterDialogue characterDialogueScript;

    public float CameraSpeed
    {
        get { return cameraSpeed; }
        set { cameraSpeed = value; }
    }

    public int PuzzleViewIndex
    {
        get { return puzzleViewIndex; }
        set { puzzleViewIndex = value; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
        SetPuzzleViews();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalCameraRotation = transform.eulerAngles;
        SetInitialPosition();
    }

    // Moves the camera to the next puzzle view
    public void NextPuzzleView()
    {
        int nextPuzzleIndex = playerScript.PuzzleNumber;
        puzzleViewIndex++;

        if (puzzleViewIndex > puzzleViews.Count - 1)
            puzzleViewIndex = 0;

        // Sets the "correct" next puzzle view if applicable
        if (puzzleViewIndex != nextPuzzleIndex)
            puzzleViewIndex = nextPuzzleIndex;

        // Dont lerp if the camera is already at the next puzzle view
        if (currentPuzzleView != puzzleViews[puzzleViewIndex])
            LerpToPuzzleView();

        puzzleManagerScript.ResetGeneratorCheck();
        gameHUDScript.UpdatePuzzleBubbleText($"{puzzleViewIndex + 1}/{checkpoints.Count}");
        notificationBubblesScript.PlayPuzzleNotificationCheck();

        //Debug.Log("Camera has moved to the NEXT puzzle view");
    }

    // Moves the camera to the previous puzzle view
    public void PreviousPuzzleView()
    {
        int previousPuzzleIndex = playerScript.PuzzleNumber - 2;
        puzzleViewIndex--;

        if (puzzleViewIndex < 0)
            puzzleViewIndex = puzzleViews.Count - 1;

        // Sets the "correct" previous puzzle view if applicable
        if (puzzleViewIndex != previousPuzzleIndex)
            puzzleViewIndex = previousPuzzleIndex;

        // Dont lerp if the camera is already at the previous puzzle view
        if (currentPuzzleView != puzzleViews[puzzleViewIndex])
            LerpToPuzzleView();

        puzzleManagerScript.ResetGeneratorCheck();
        gameHUDScript.UpdatePuzzleBubbleText($"{puzzleViewIndex + 1}/{checkpoints.Count}");
        notificationBubblesScript.PlayPuzzleNotificationCheck();

        //Debug.Log("Camera has moved to the PREVIOUS puzzle view");
    }

    // Lerps the camera's position to the current puzzle view
    public void LerpToPuzzleView()
    {
        // Only plays the sfx when not in dialogue
        if (!characterDialogueScript.InDialogue)
        {
            audioManagerScript.ChangeLoopingAmbienceSFX();
            audioManagerScript.PlayWindGushSFX();
        }

        currentPuzzleView = puzzleViews[puzzleViewIndex];
        StopAllCoroutines();
        StartCoroutine(LerpCamera(currentPuzzleView));
    }

    // Sets the camera to the current dialogue view
    public void SetToDialogueView()
    {
        transform.position = currentDialogueView;
        transform.eulerAngles = originalCameraRotation;
    }

    // Checks which dialogue view the camera should move to
    public void LerpToDialogueView()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        switch (playerDirection.y)
        {
            case 0: // Looking north
                currentDialogueView = southDialogueView.transform.position;
                break;
            case 90: // Looking east
                currentDialogueView = westDialogueView.transform.position;
                break;
            case 180: // Looking south
                currentDialogueView = northDialogueView.transform.position;
                break;
            case 270: // Looking west
                currentDialogueView = eastDialogueView.transform.position;
                break;
            default:
                //Debug.Log("Unrecognizable direction");
                break;
        }

        StopAllCoroutines();
        StartCoroutine(LerpCamera(currentDialogueView));
    }

    // Sets the camera's initial position - determined by puzzleViewIndex
    private void SetInitialPosition()
    {
        if (transform.position != puzzleViews[puzzleViewIndex])
            transform.position = puzzleViews[puzzleViewIndex];

        gameHUDScript.UpdatePuzzleBubbleText($"{puzzleViewIndex + 1}/{checkpoints.Count}");
    }

    // Calculates the puzzle view position for a puzzle - determined by checkpoint
    private Vector3 CalculatePuzzleView(GameObject checkpoint)
    {
        // Note: tileHolder is the game object that holds all of the tiles in a puzzle
        GameObject tileHolder = checkpoint.transform.parent.gameObject;

        xValues.Clear();
        zValues.Clear();

        // Find the highest/lowest x and z values
        for (int j = 0; j < tileHolder.transform.childCount; j++)
        {
            GameObject tile = tileHolder.transform.GetChild(j).gameObject;
            Vector3 tilePosition = tile.transform.position;
            FindNeighboringTiles(tile);

            // If the tile has a neighboring tile to the north/south...
            if (neighboringTiles.Contains(0) || neighboringTiles.Contains(180))
                xValues.Add(tilePosition.x); // Add its x position to the appropriate list

            // If the tile has a neighboring tile to the east/west...
            if (neighboringTiles.Contains(90) || neighboringTiles.Contains(270))
                zValues.Add(tilePosition.z); // Add its z position to the appropriate list
        }

        // Sorts the list from lowest to highest values
        xValues.Sort();
        zValues.Sort();

        // Sets the length and width of the puzzle (highest x/z position minus the lowest x/y position)
        float puzzleLength = xValues.Last() - xValues.First();
        float puzzleWidth = zValues.Last() - zValues.First();

        // Finds the x,y,z position for the calculated puzzle view
        float xPos = xValues.First() + (puzzleLength / 2f);                                 // The center of the puzzle (x)
        float yPos = 7f;                                                                    // Constant for all puzzle views (y)
        float zPos = zValues.First() + (puzzleWidth / 2f) - ValueToSubractZ(puzzleWidth);   // How far the camera is from the puzzle's center (z)

        return new Vector3(xPos, yPos, zPos);
    }

    // Adds/removes the rotation/direction at which a neighboring tile was found/not-found (0 = north, 180 = south, 90 = east, 270 = west)
    private void FindNeighboringTiles(GameObject tile)
    {
        for (int i = 0; i < tile.transform.childCount; i++)
        {
            GameObject child = tile.transform.GetChild(i).gameObject;

            if (child.name == "EdgeCheck")
            {
                GameObject tileEdgeCheck = child;

                for (int j = 0; j < 360; j += 90)
                {
                    tileEdgeCheck.transform.localEulerAngles = new Vector3(0, j, 0);

                    Ray myRay = new Ray(tileEdgeCheck.transform.position + new Vector3(0, -0.5f, 0), tileEdgeCheck.transform.TransformDirection(Vector3.forward));
                    RaycastHit hit;
                    Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

                    if (Physics.Raycast(myRay, out hit, rayLength) && hit.collider.tag != "BridgeTile")
                    {
                        if (!neighboringTiles.Contains(j))
                            neighboringTiles.Add(j);

                        //Debug.Log("Found tile at direction " + j);
                    }
                    else
                    {
                        if (neighboringTiles.Contains(j))
                            neighboringTiles.Remove(j);

                        //Debug.Log("No tile was found at direction " + j);
                    }
                }
            }
        }
    }

    // Determines the value to subract from a calculated puzzle view's z position
    private float ValueToSubractZ(float widthOfPuzzle)
    {
        //If the puzzle width is 7 tiles long (or greater)
        if (widthOfPuzzle > 5f)
            return 6f;

        // If the puzzle width is 6 tiles long
        else if (widthOfPuzzle > 4f)
            return 5.5f;

        // If the puzzle width is 5 tiles long
        else if (widthOfPuzzle > 3f)
            return 5.25f;

        // If the puzzle width is 4 tiles long (or less)
        else
            return 5f;
    }

    // Lerps the camera to the next/previous puzzle view (viewIndex = the index of the puzzle view to lerp to)
    private void LerpCameraDebug(int viewIndex)
    {
        // Checks if the index is out of the puzzleViews range
        if (viewIndex > puzzleViews.Count - 1)
            puzzleViewIndex = 0;
        else if (viewIndex < 0)
            puzzleViewIndex = puzzleViews.Count - 1;

        LerpToPuzzleView();
        gameHUDScript.UpdatePuzzleBubbleText($"{puzzleViewIndex + 1}/{checkpoints.Count}");

        Debug.Log("Debugging: Moved Camera To Puzzle " + (puzzleViewIndex + 1));
    }

    // Lerps the position of the camera (endPosition = position to lerp to)
    private IEnumerator LerpCamera(Vector3 endPosition)
    {
        gameManagerScript.CheckForCameraScriptDebug();
        float duration = cameraLerpLength;
        float time = 0;

        while (time < duration)
        {
            // Note: transform.position will always lerp closer to the endPosition, but never equal it
            transform.position = Vector3.Lerp(transform.position, endPosition, cameraSpeed * Time.deltaTime);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        //Debug.Log("The camera has lerped to the next position");
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        gameHUDScript = FindObjectOfType<GameHUD>();
        playerScript = FindObjectOfType<TileMovementController>();
        gameManagerScript = FindObjectOfType<GameManager>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
    }

    // Sets the puzzle views array and camera transition speed
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            GameObject child = transform.parent.GetChild(i).gameObject;

            if (child.name == "DialogueViewsHolder")
            {
                GameObject dialogueViewsHolder = child;

                for (int j = 0; j < dialogueViewsHolder.transform.childCount; j++)
                {
                    GameObject child02 = dialogueViewsHolder.transform.GetChild(j).gameObject;
                    string childName02 = child02.name;

                    if (childName02 == "NorthDV")
                        northDialogueView = child02;
                    if (childName02 == "EastDV")
                        eastDialogueView = child02;
                    if (childName02 == "SouthDV")
                        southDialogueView = child02;
                    if (childName02 == "WestDV")
                        westDialogueView = child02;
                }
            }
        }

        cameraSpeed = gameManagerScript.cameraSpeed;
    }

    // Creates a puzzle view for each puzzle and adds them to a list
    private void SetPuzzleViews()
    {
        checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint").ToList();

        for (int i = 0; i < checkpoints.Count; i++)
        {
            GameObject checkpoint = checkpoints[i];
            puzzleViews.Add(CalculatePuzzleView(checkpoint));
        }
    }

    // Enables debugging for the puzzle views and cameraSpeed - For Debugging Purposes ONLY
    public void DebuggingCheck(GameManager gameManager)
    {
        if (gameManager.isDebugging)
        {
            if (Input.GetKeyDown(KeyCode.Equals)) // Debug key is "=" (equal)
                LerpCameraDebug(++puzzleViewIndex);

            else if (Input.GetKeyDown(KeyCode.Minus)) // Debug key is "-" (minus)
                LerpCameraDebug(--puzzleViewIndex);
        }
    }

}
