using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;

public class InteractableImageStack : MonoBehaviour
{
    public Camera userCamera;  // Reference to the user's camera

    private void Start()
    {
        if (userCamera == null)
        {
            userCamera = Camera.main;  // Use the main camera if no camera is assigned
        }

        // Ensure the Canvas is using World Space
        Canvas canvas = GetComponent<Canvas>();
        if (canvas.renderMode != RenderMode.WorldSpace)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }

        // Position the Canvas in front of the camera
        PositionCanvas();

        // Initialize the Canvas
        Initialize();

    }

    private void Initialize()
    {
        // Initialize all children using their Initialize method
        transform.GetComponentInChildren<GridMaker>().Initialize();
        transform.GetComponentInChildren<whole_image>().Initialize();
        InstantiateCanvasUI(transform.GetComponentInChildren<ClickNextImage>().transform);
        
    }


    public static void SetupAnchorsAndPivots(RectTransform rectTransform)
    {
        // Set the anchors and pivots of the Canvas
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    public static List<float> GetFOVatWD(float WD)
    {
        // Pythagoras theorem to calculate the distance
        List<float> holder = new List<float>();
        float vertical_fov = Camera.main.fieldOfView;
        float fov_height = (WD * Mathf.Tan(vertical_fov * 0.5f)) * 2;
        float fov_width =  Camera.main.aspect * fov_height;     // Aspect ratio of the camera is width/height

        holder.Add(fov_height);
        holder.Add(fov_width);
        holder.Add(WD);

        return holder;
    }

    public List<float> GetFOVatNearClipping()
    {   // Must be near or else the child elements of canvas will not be visible

        // Pythagoras theorem to calculate the distance
        List<float> holder = new List<float>();
        float vertical_fov = Camera.main.fieldOfView;
        float clipping_distance = Camera.main.nearClipPlane;
        float fov_height = (clipping_distance * Mathf.Tan(vertical_fov * 0.5f)) * 2;
        float fov_width =  Camera.main.aspect * fov_height; // Aspect ratio of the camera is width/height

        holder.Add(fov_height);
        holder.Add(fov_width);
        holder.Add(clipping_distance);

        return holder;
    }


    public static Vector3 FacePlayer(float scaler)
    {
        // Face the player
        Vector3 cameraPosition = new Vector3(0,0,0);
        Vector3 cameraForward = new Vector3(0,0,1);

        return cameraPosition + cameraForward * scaler;
    }

    void PositionCanvas()
    {

        // Setup anchors and pivots
        RectTransform rectTransform = GetComponent<RectTransform>();
        SetupAnchorsAndPivots(rectTransform);

        // Calculate the new position for the Canvas to minimum clipping distance
        transform.position = FacePlayer(userCamera.nearClipPlane);

        Debug.Log("The position of the canvas is the following: " + transform.position);

        // Set rotation of the Canvas to face the camera
        transform.rotation = Quaternion.Euler(Vector3.zero);

        // Set scale to 1
        transform.localScale = new Vector3(1, 1, 1);

        List<float> outputs = GetFOVatNearClipping();

        rectTransform.sizeDelta = new Vector2(outputs[1], outputs[0]);


    }

    public static GameObject CreateGameObject(Transform parent, string prefabPath, Transform transform)
    {
        // Create a new RawImage GameObject from the prefab
        GameObject instance= Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), transform.position, transform.rotation);
        instance.transform.SetParent(parent);
        return instance;
    }

public static void ChildIdenticalToParent(GameObject parent, GameObject child)
{
    child.transform.SetParent(parent.transform);
    // Anchors and pivots are the same as the parent
    child.GetComponent<RectTransform>().anchorMin = parent.GetComponent<RectTransform>().anchorMin;
    child.GetComponent<RectTransform>().anchorMax = parent.GetComponent<RectTransform>().anchorMax;
    child.GetComponent<RectTransform>().pivot = parent.GetComponent<RectTransform>().pivot;

    child.transform.position = parent.transform.position;
    child.transform.rotation = parent.transform.rotation;
    child.transform.localScale = parent.transform.localScale;

    // Set the size of the child to be the same as the parent
    child.GetComponent<RectTransform>().sizeDelta = parent.GetComponent<RectTransform>().sizeDelta;


}

private void InstantiateCanvasUI(Transform rawImageTransform)
{   
    // Create a new Canvas UI GameObject
    GameObject CanvasUI = CreateGameObject(transform, "Assets/Scenes/CIAnnotator/Canvas UI.prefab", transform);

    PositionandResizeCanvasUI(CanvasUI, rawImageTransform);

    CanvasUI.GetComponent<SetupButtons>().Initialize(rawImageTransform, CanvasUI.transform.position, CanvasUI.transform.rotation);
    
}

private void PositionandResizeCanvasUI(GameObject CanvasUI, Transform rawImageTransform)
{
    // Set the anchors and pivots of the Canvas UI
    SetupAnchorsAndPivots(CanvasUI.GetComponent<RectTransform>());

    // Set to bottom left corner the anchors
    CanvasUI.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
    CanvasUI.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

    // Face the Canvas UI to the player
    CanvasUI.transform.rotation = Quaternion.Euler(Vector3.zero);

    // Change pivot to top left corner of the Canvas UI, so no overlap with the RawImage
    CanvasUI.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

    // Set location of the Canvas UI to the top right corner of the RawImage
    CanvasUI.transform.position = new Vector3((rawImageTransform.position.x + rawImageTransform.GetComponent<RectTransform>().sizeDelta.x/2)*1.1f,
    rawImageTransform.position.y + rawImageTransform.GetComponent<RectTransform>().sizeDelta.y/2, rawImageTransform.position.z);

    // Set scale of the Canvas
    CanvasUI.transform.localScale = Vector3.one;
}

}
