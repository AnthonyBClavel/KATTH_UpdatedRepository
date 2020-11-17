using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixelatedCamera : MonoBehaviour
{
    public enum PixelScreenMode { Resize, Scale }

    [System.Serializable]
    public struct ScreenSize
    {
        //integers to store screen size info
        public int width;
        public int height;
    }

    public static PixelatedCamera main;

    private Camera renderCamera;
    private RenderTexture renderTexture;
    private int screenWidth, screenHeight;

    [Header("Screen scaling settings")]
    public PixelScreenMode mode;
    public ScreenSize targetScreenSize = new ScreenSize { width = 256, height = 144 };  // Only used with PixelScreenMode.Resize
    public uint screenScaleFactor = 1;  // Only used with PixelScreenMode.Scale


    [Header("Display")]
    public RawImage display;

    private void Awake()
    {
        //set as main pixel camera
        if (main == null) main = this;
    }

    private void Start()
    {
        //initialize the system
        Init();
    }

    private void Update()
    {
        //re-initialize system if the screen has been resized
        if (CheckScreenResize()) Init();
    }

    public void Init()
    {

        //initialize the camera and get screen size values
        if (!renderCamera) renderCamera = GetComponent<Camera>();
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        //prevent any errors
        if (screenScaleFactor < 1) screenScaleFactor = 1;
        if (targetScreenSize.width < 1) targetScreenSize.width = 1;
        if (targetScreenSize.height < 1) targetScreenSize.height = 1;

        //calculate the render texture size
        int width = mode == PixelScreenMode.Resize ? (int)targetScreenSize.width : screenWidth / (int)screenScaleFactor;
        int height = mode == PixelScreenMode.Resize ? (int)targetScreenSize.height : screenHeight / (int)screenScaleFactor;

        //initialize the render texture
        renderTexture = new RenderTexture(width, height, 24)
        {
            filterMode = FilterMode.Point,
            antiAliasing = 1
        };

        //set the render texture as the camera's output
        renderCamera.targetTexture = renderTexture;

        //attaching texture to the display UI RawImage
        display.texture = renderTexture;
    }

    public bool CheckScreenResize()
    {
        //check whether the screen has been resized
        return Screen.width != screenWidth || Screen.height != screenHeight;
    }
}