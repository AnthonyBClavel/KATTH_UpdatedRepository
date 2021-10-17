using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassMaterial : MonoBehaviour
{
    private Material grassMaterial;
    private Transform playerTransform;
    private Vector3 matPosition;
    private GameManager gameManagerScript;

    void Awake()
    {
        gameManagerScript = FindObjectOfType<GameManager>();
        playerTransform = FindObjectOfType<TileMovementController>().transform;
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
            if (matPosition != playerTransform.position)
            {
                matPosition = playerTransform.position;
                grassMaterial.SetVector("_position", matPosition);
            }

            // Use this if you have more than one grass material (baby mammoth)
            /*for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetVector("_position", matPosition);
            }*/

            return;
        }
    }

}
