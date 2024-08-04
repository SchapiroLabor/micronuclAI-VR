using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Unity.VisualScripting;
using System.Diagnostics.CodeAnalysis;

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
        transform.GetComponentInChildren<whole_image>().Initialize();
        transform.GetComponentInChildren<GridMaker>().Initialize();
    }

    void PositionCanvas()
    {
        Vector3 cameraPosition = userCamera.transform.position;

        Vector3 cameraForward = userCamera.transform.forward;

        // Calculate the new position for the Canvas to max clipping distance
        Vector3 newPosition = cameraPosition + cameraForward * userCamera.farClipPlane;

        // Set the position and rotation of the Canvas
        transform.position = newPosition;
        transform.rotation = Quaternion.LookRotation(cameraForward);

        transform.localScale = new Vector3(1, 1, 1);
        float focalLength = userCamera.focalLength;

        float width = (userCamera.sensorSize.x/focalLength) * Screen.width;
        float height = (userCamera.sensorSize.y/focalLength) * Screen.height;

        // Set width and height of the Canvas
        RectTransform rectTransform = GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2(width, height);


    }

    public GameObject CreateGameObject(Transform parent, string prefabPath)
    {
        // Create a new RawImage GameObject from the prefab
        GameObject instance= Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), transform.position, transform.rotation);
        instance.transform.SetParent(parent);
        return instance;
    }


}
