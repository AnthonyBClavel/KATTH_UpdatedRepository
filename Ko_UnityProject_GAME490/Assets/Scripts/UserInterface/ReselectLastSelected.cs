using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReselectLastSelected : MonoBehaviour
{
    public static ReselectLastSelected instance;
    private GameObject lastSelectedObject;
    private EventSystem eventSystemScript;

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        ReselectCheck();
    }

    // Checks to reselect the last select object
    private void ReselectCheck()
    {
        if (HasSelectedObject()) return;

        ReselectLastSelectedObject();
    }

    // Checks if the event system has selected an object - returns true if so, false otherwise
    private bool HasSelectedObject()
    {
        GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;

        if (currentSelectedGameObject == null) return false;
        lastSelectedObject = currentSelectedGameObject;
        return true;
    }

    // Checks to set the last selected object as the current selected object
    private void ReselectLastSelectedObject()
    {
        if (!gameObject.activeInHierarchy) return;

        eventSystemScript.SetSelectedGameObject(null);
        eventSystemScript.SetSelectedGameObject(lastSelectedObject);  
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        eventSystemScript = FindObjectOfType<EventSystem>();
        instance = this;
    }

    /*// Starts the coroutine that checks to reselect the last selected object
    public void StartLastSelectedCorouitne()
    {
        if (lastSelectedCoroutine != null) StopCoroutine(lastSelectedCoroutine);

        lastSelectedCoroutine = ReselectWhileObjectIsActive();
        StartCoroutine(lastSelectedCoroutine);

    }
    // Checks to reselect the last selected object while another game object is active
    private IEnumerator ReselectWhileObjectIsActive()
    {
        while (isActiveAndEnabled)
        {
            ReselectCheck();
            yield return null;
        }
        //Debug.Log("Reselect check has now ended");
    }*/

}