using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveGrassForBM : MonoBehaviour
{
    public Material[] materials;
    public Transform theBabyMammoth;
    Vector3 thePosition02;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("writeToMaterial");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator writeToMaterial()
    {
        while(true)
        {
            thePosition02 = theBabyMammoth.transform.position;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetVector("_position02", thePosition02);
            }

            yield return null;
        }      
    }

}
