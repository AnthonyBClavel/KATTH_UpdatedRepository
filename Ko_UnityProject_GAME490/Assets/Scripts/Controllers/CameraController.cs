using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraController : MonoBehaviour
{
    private float cameraSpeed = 3f; // Original Value = 3f
    private float rayLength = 1f;

    private bool hasMovedPuzzleView = false;
    private int puzzleViewIndex;
    private int currentPuzzleIndex;

    private GameObject northDialogueView;
    private GameObject eastDialogueView;
    private GameObject southDialogueView;
    private GameObject westDialogueView;

    private Vector3 currentDialogueView;
    private Vector3 currentPuzzleView;
    private Vector3 currentPuzzleAngle;
    private Vector3 defaultPuzzleAngle = new Vector3(54, 0, 0);

    List<GameObject> checkpoints = new List<GameObject>();
    List<Vector3> puzzleViews = new List<Vector3>();
    List<Vector3> puzzleAngles = new List<Vector3>();
    List<float> xPositions = new List<float>();
    List<float> zPositions = new List<float>();
    List<int> neighboringTiles = new List<int>();

    private NotificationBubbles notificationBubblesScript;
    private CharacterDialogue characterDialogueScript;
    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private SaveManager saveManagerScript;
    private GameManager gameManagerScript;

    public int PuzzleViewIndex
    {
        get { return puzzleViewIndex; }
        set { SetPuzzleViewIndex(value); }
    }
    
    public bool HasMovedPuzzleView
    {
        get { return hasMovedPuzzleView; }
        set { hasMovedPuzzleView = value; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetToPuzzleView();
    }

    // Checks to move the camera to the next/previous puzzle view
    public void LerpToNextPuzzleVew()
    {
        if (hasMovedPuzzleView || playerScript.OnFirstOrlastBridge()) return;

        // Note: the value of currentPuzzleIndex is changed due to the "+=" operator
        puzzleViewIndex = currentPuzzleIndex += playerScript.HasFinishedPuzzle ? 1 : -1;
        saveManagerScript.CameraIndex = puzzleViewIndex;

        // Note: the camera doesn't lerp if its already at the intended next/previous puzzle view
        if (currentPuzzleView != puzzleViews[puzzleViewIndex]) LerpToPuzzleView();
        notificationBubblesScript.SetPuzzleNotificationText(puzzleViewIndex + 1);
        audioManagerScript.FadeOutGeneratorSFX();
        hasMovedPuzzleView = true;

        //Debug.Log($"Moved camera to puzzle {puzzleViewIndex + 1}");
    }

    // Lerps the camera's position to the current puzzle view
    public void LerpToPuzzleView()
    {
        if (!characterDialogueScript.InDialogue)
        {
            audioManagerScript.PlayShortWindGushSFX();
            audioManagerScript.PlayAmbientWindSFX();
        }

        currentPuzzleView = puzzleViews[puzzleViewIndex];
        currentPuzzleAngle = puzzleAngles[puzzleViewIndex];

        StopAllCoroutines();       
        StartCoroutine(LerpCameraPosition(currentPuzzleView));
        StartCoroutine(LerpCameraRotation(currentPuzzleAngle));
    }

    // Lerps the camera's position to the current dialogue view
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
        StartCoroutine(LerpCameraPosition(currentDialogueView));
        StartCoroutine(LerpCameraRotation(defaultPuzzleAngle));
    }

    // Sets the camera's position to the current puzzle view
    private void SetToPuzzleView()
    {
        transform.position = puzzleViews[puzzleViewIndex];
        transform.eulerAngles = puzzleAngles[puzzleViewIndex];

        currentPuzzleIndex = puzzleViewIndex;
    }

    // Sets the camera's position to the current dialogue view
    public void SetToDialogueView()
    {
        transform.position = currentDialogueView;
        transform.eulerAngles = defaultPuzzleAngle;
    }

    // Finds the camera position for a puzzle (checkpoint = the puzzle's checkpoint)
    private Vector3 FindPuzzleView(GameObject checkpoint)
    {
        // Note: tileHolder is the parent object that holds all of the tiles within a puzzle
        Transform tileHolder = checkpoint.transform.parent;
        xPositions.Clear();
        zPositions.Clear();

        // Adds each tile's x and z position to the appropriate list IF applicable
        foreach (Transform tile in tileHolder)
        {
            float tilePosX = tile.position.x;
            float tilePosZ = tile.position.z;
            FindNeighboringTiles(tile);

            // If there's a neighboring tile to the north/south...
            if (neighboringTiles.Contains(0) || neighboringTiles.Contains(180))
                if (!xPositions.Contains(tilePosX)) xPositions.Add(tilePosX);

            // If there's a neighboring tile to the east/west...
            if (neighboringTiles.Contains(90) || neighboringTiles.Contains(270))
                if (!zPositions.Contains(tilePosZ)) zPositions.Add(tilePosZ);
        }

        // Note: sorts the lists from lowest to highest
        xPositions.Sort();
        zPositions.Sort();

        // Note: subtracts highest value from lowest value
        float puzzleLength = xPositions.Last() - xPositions.First();
        float puzzleWidth = zPositions.Last() - zPositions.First();

        float xPos = xPositions.First() + (puzzleLength / 2f);  // center of the puzzle                    
        float yPos = 7f;                                        // constant height                           
        float zPos = zPositions.First() - OffsetZ(puzzleWidth); // lowest z pos minus offset

        puzzleAngles.Add(FindPuzzleAngle(puzzleWidth));
        return new Vector3(xPos, yPos, zPos);
    }

    // Finds the camera rotation for a puzzle (width = the width of the puzzle)
    private Vector3 FindPuzzleAngle(float width)
    {
        float puzzleWidthInBlocks = width + 1;
        return new Vector3(50f + puzzleWidthInBlocks, 0, 0);
    }

    // Determines the value to subract from the camera's z position
    private float OffsetZ(float puzzleWidth)
    {
        // If the puzzle width is 6-7 blocks long
        if (puzzleWidth > 4)
            return (puzzleWidth % 2 == 0) ? 2.1f : 2.6f;

        // If the puzzle width is 4-5 blocks long
        else if (puzzleWidth > 2)
            return (puzzleWidth % 2 == 0) ? 3.1f : 3.6f;

        // If the puzzle width is 3 blocks or less
        else
            return (puzzleWidth % 2 == 0) ? 4.1f : 4.6f;
    }

    // Finds the rotation/direction of the tiles that neighbor another (tile = the tile to check)
    // Note: 0 = north, 90 = east, 180 = south, 270 = west
    private void FindNeighboringTiles(Transform tile)
    {
        neighboringTiles.Clear(); 

        foreach (Transform child in tile)
        {
            // Looks for the EdgeCheck within the tile's children
            if (child.name != "EdgeCheck") continue;

            for (int j = 0; j < 360; j += 90)
            {
                child.localEulerAngles = new Vector3(0, j, 0);

                Ray myRay = new Ray(child.position + new Vector3(0, -0.5f, 0), child.TransformDirection(Vector3.forward));
                RaycastHit hit;
                //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

                // If the ray doesn't hit annything or if it hits a bridge tile, then CONTINUE the loop
                if (!Physics.Raycast(myRay, out hit, rayLength) || hit.collider.CompareTag("BridgeTile")) continue;
                neighboringTiles.Add(j);
            }
        }
    }

    // Lerps the position of the camera to another over a duration (endPosition = position to lerp to)
    // Note: transform.position will always lerp closer to the endPosition, but never equal it
    private IEnumerator LerpCameraPosition(Vector3 endPosition)
    {
        float camSpeed = CameraSpeed();

        while (Vector3.Distance(transform.position, endPosition) > 0.001f)
        {           
            transform.position = Vector3.Lerp(transform.position, endPosition, camSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = endPosition;
        //Debug.Log("The camera has lerped to the next position");
    }

    // Lerps the rotation of the camera to another over a duration (endRotation = rotation to lerp to)
    // Note: transform.eulerAngles will always lerp closer to the endRotation, but never equal it
    private IEnumerator LerpCameraRotation(Vector3 endRotation)
    {
        float camSpeed = CameraSpeed();

        while (Vector3.Distance(transform.eulerAngles, endRotation) > 0.001f)
        {       
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, endRotation, camSpeed * Time.deltaTime);
            yield return null;
        }

        transform.eulerAngles = endRotation;
        //Debug.Log("The camera has lerped to the next rotation");
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
    }

    // Sets the puzzle views - creates a puzzle view for each puzzle in the zone
    private void SetPuzzleViews()
    {
        checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint").OrderBy(x => x.transform.parent.parent.name).ToList();
        //Debug.Log($"Camera Controller: order of the checkpoints/puzzles found: {checkpoints.ConvertGameObjectListToString()}");

        foreach (GameObject checkpoint in checkpoints)
            puzzleViews.Add(FindPuzzleView(checkpoint));
    }

    // Checks to set the value of puzzleViewIndex
    private void SetPuzzleViewIndex(int value)
    {
        // Note: "maxPVI > minPVI" checks if the puzzle count is greater than zero
        // Note: the saveManagerScript gets called before this script, so when it calls this function, puzzleViews.count will be zero
        // Note: changing the "script execution order" settings can solve this, but it's best to avoid changing this setting

        int maxPVI = puzzleViews.Count - 1;
        int minPVI = 0;

        if (maxPVI > minPVI && value > maxPVI)
            puzzleViewIndex = minPVI;

        else if (maxPVI > minPVI && value < minPVI)
            puzzleViewIndex = maxPVI;

        else
            puzzleViewIndex = value;
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "NorthDV":
                    northDialogueView = child.gameObject;
                    break;
                case "EastDV":
                    eastDialogueView = child.gameObject;
                    break;
                case "SouthDV":
                    southDialogueView = child.gameObject;
                    break;
                case "WestDV":
                    westDialogueView = child.gameObject;
                    break;
                default:
                    break;
            }

            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetPuzzleViews();
        SetVariables(transform.parent);

        cameraSpeed = CameraSpeed();
    }

    // Lerps the camera to the current/next/previous puzzle view - For Debugging Purposes Only
    // Note: no value will be added to the puzzleViewIndex if the int parameter is not set
    private void LerpCameraDebug(int addToIndex)
    {
        // Note: must use "PuzzleViewIndex" (the one with a capital "P"), not puzzleViewIndex
        PuzzleViewIndex += addToIndex;
        LerpToPuzzleView();

        notificationBubblesScript.SetPuzzleNotificationText(puzzleViewIndex + 1, false);
        Debug.Log($"Debugging: moved camera to puzzle {puzzleViewIndex + 1}");
    }

    // Checks to lerp the camera to the next/previous puzzle view - For Debugging Purposes ONLY
    public void DebuggingCheck()
    {
        if (Input.GetKeyDown(KeyCode.Equals)) // Debug key is "=" (equal)
            LerpCameraDebug(1);

        if (Input.GetKeyDown(KeyCode.Minus)) // Debug key is "-" (minus)
            LerpCameraDebug(-1);
    }

    // Checks to return the debug value for camera speed - For Debugging Purposes ONLY
    private float CameraSpeed()
    {
        if (!gameManagerScript.isDebugging) return cameraSpeed;

        return gameManagerScript.CameraSpeed;
    }

}
