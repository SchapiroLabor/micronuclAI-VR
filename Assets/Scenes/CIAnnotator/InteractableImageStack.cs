using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;

public class InteractableImageStack : MonoBehaviour
{
    public List<Texture2D> images;
    public GameObject rawImagecurrent;
    public int current_img_indx;
    public int N_image;
    public Vector3 start_position;
    public Quaternion start_rotation;
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
    }


    public void SetupAnchorsAndPivots(RectTransform rectTransform)
    {
        // Set the anchors and pivots of the Canvas
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    public List<float> GetFOVatWD(float WD)
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


    public Vector3 FacePlayer(float scaler)
    {
        // Face the player
        Vector3 cameraPosition = userCamera.transform.position;
        Vector3 cameraForward = userCamera.transform.forward;
        return cameraPosition + cameraForward * scaler;
    }

    void PositionCanvas()
    {

        // Setup anchors and pivots
        RectTransform rectTransform = GetComponent<RectTransform>();
        SetupAnchorsAndPivots(rectTransform);

        // Calculate the new position for the Canvas to minimum clipping distance
        transform.position = FacePlayer(userCamera.nearClipPlane);

        // Set rotation of the Canvas to face the camera
        transform.rotation = Quaternion.Euler(Vector3.zero);

        // Set scale to 1
        transform.localScale = new Vector3(1, 1, 1);

        List<float> outputs = GetFOVatNearClipping();

        rectTransform.sizeDelta = new Vector2(outputs[1], outputs[0]);


    }

    public GameObject CreateGameObject(Transform parent, string prefabPath)
    {
        // Create a new RawImage GameObject from the prefab
        GameObject instance= Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), transform.position, transform.rotation);
        instance.transform.SetParent(parent);
        return instance;
    }


}
