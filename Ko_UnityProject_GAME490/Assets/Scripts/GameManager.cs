using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Floats")]
    public float typingDelay = 0.03f;
    public float blackBarsSpeed = 400f;

    [Header("Game Objects")]
    public GameObject firstBlock;

    [Header("Materials")]
    public Material grassMaterial;
    public Material iceMaterial;

    [Header("HUD Elements")]
    public GameObject frostedBorder;

    [Header("Transform Arrays")]
    public Transform[] puzzleViews;
    public Transform[] checkpoints;


    void Awake()
    {

    }
    
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Sets certain elements for other scripts - MUST BE CALLED IN AWAKE()
    private void SetElements()
    {
        
    }

}
