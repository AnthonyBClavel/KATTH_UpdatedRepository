using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(StandaloneInputModule))]

public class ReselectLastSelectedOnInput : MonoBehaviour
{
    private StandaloneInputModule standaloneInputModule;
    private GameObject lastSelectedObject;
    public static ReselectLastSelectedOnInput instance;

    void Awake()
    {
        instance = this;
        standaloneInputModule = GetComponent<StandaloneInputModule>();
    }

    void Update()
    {
        CacheLastSelectedObject();

        if (EventSystemHasObjectSelected())
            return;

        // If any axis/submit/cancel is pressed...
        // Looks at the input names defined in the attached StandaloneInputModule
        if ((Input.GetAxisRaw(standaloneInputModule.horizontalAxis) != 0) ||
             (Input.GetAxisRaw(standaloneInputModule.verticalAxis) != 0) ||
             (Input.GetButtonDown(standaloneInputModule.submitButton)) ||
             (Input.GetButtonDown(standaloneInputModule.cancelButton)) ||
             (Input.GetMouseButton(0)) || (Input.GetMouseButton(1)) || (Input.GetMouseButton(2)))        
        {
            // Reselect the cached 'lastSelectedObject'
            ReselectLastObject();
            return;
        }
    }

    // Called whenever a UI navigation/submit/cancel button is pressed.
    public static void ReselectLastObject()
    {
        // Do nothing if this is not active
        if (!instance.isActiveAndEnabled || !instance.gameObject.activeInHierarchy)
            return;

        // Set current to lastSelectedObject
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(instance.lastSelectedObject);
    }

    // Checks to see if anything has anything has been selected
    static bool EventSystemHasObjectSelected()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
            return false;
        else
            return true;
    }

    // Caches last selected object for later use
    void CacheLastSelectedObject()
    {
        // Dont cache if nothing is selected
        if (EventSystemHasObjectSelected() == false)
            return;

        lastSelectedObject = EventSystem.current.currentSelectedGameObject.gameObject;
    }

}