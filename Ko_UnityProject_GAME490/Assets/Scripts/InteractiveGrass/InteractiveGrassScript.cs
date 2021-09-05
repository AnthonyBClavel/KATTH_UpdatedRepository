using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveGrassScript : MonoBehaviour
{
    private Material grassMaterial;
    private Transform playerTransform;
    private Vector3 thePosition;
    private GameManager gameManagerScript;

    void Awake()
    {
        playerTransform = FindObjectOfType<TileMovementController>().transform;
        gameManagerScript = FindObjectOfType<GameManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        grassMaterial = gameManagerScript.grassMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        WriteToGrassMaterial();
    }

    // Sets the player's position to the material's vector position
    private void WriteToGrassMaterial()
    {
        while (true)
        {
            if (thePosition != playerTransform.position)
            {
                thePosition = playerTransform.position;
                grassMaterial.SetVector("_position", thePosition);
            }

            // Use this if you have more than one grass material (baby mammoth)
            /*for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetVector("_position", thePosition);
            }*/

            return;
        }
    }

}
