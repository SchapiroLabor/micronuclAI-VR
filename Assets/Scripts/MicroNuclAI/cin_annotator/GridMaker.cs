using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
// Import functions from another script
using NonGOSripts; // With a static directive, you can access the members of the class by using the class name itself

public class GridMaker : MonoBehaviour
{
    private Camera userCamera;
    private RectTransform rectTransform;
    public float raycast_distance = 10f; // Default distance to raycast from the camera, please do not change this !!



    void Awake()
    {

        // Get rect transform of the grid
        rectTransform = GetComponent<RectTransform>();

        // Get the user's camera
        userCamera = Camera.main;

        // Positioning should be identical to the canvas
        // Also, local position is only accurate when used after Start() or Awake()
        PositionGrid(rectTransform, userCamera, raycast_distance);


    }

    void PositionGrid(RectTransform rectTransform, Camera userCamera, float WD)
    {

        // Set anchor to the centre of the screen
        rectTransform.anchorMin = new UnityEngine.Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new UnityEngine.Vector2(0.5f, 0.5f);

        // Set pivot to the centre of the screen
        rectTransform.pivot = new UnityEngine.Vector2(0.5f, 0.5f);

        rectTransform.localPosition = new UnityEngine.Vector3(0, 0, WD);

        // Set size of Grid to FOV at the maximum raycast distance
        List<float> outputs = GetFOVatWD(WD, userCamera);

        rectTransform.sizeDelta = new UnityEngine.Vector2(outputs[1], outputs[0]);

    }


    private List<float> GetFOVatWD(float WD, Camera userCamera)
    {
        // Pythagoras theorem to calculate the distance
        List<float> holder = new List<float>();
        float vertical_fov = userCamera.fieldOfView;
        float fov_height = (WD * Mathf.Tan(vertical_fov * 0.5f)) * 2;
        float fov_width = userCamera.aspect * fov_height; // Aspect ratio of the camera is width/height

        holder.Add(fov_height);
        holder.Add(fov_width);
        holder.Add(WD);

        SchapiroLabLog.Log($"FOV at working distance {WD}: {fov_height}, {fov_width}");

        return holder;
    }
}
