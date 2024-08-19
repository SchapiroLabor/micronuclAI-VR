using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
// Import functions from another script
using static InteractableImageStack; // With a static directive, you can access the members of the class by using the class name itself

public class GridMaker : MonoBehaviour
{
    private Camera userCamera;
    private RectTransform rectTransform;
    private float raycast_distance = 10f; // Default distance to raycast from the camera, please do not change this !!



    // Start is called before the first frame update
    public void Initialize(string dataFolder)
    {   

        // Get rect transform of the grid
        rectTransform = GetComponent<RectTransform>();

        // Get the user's camera
        userCamera = Camera.main;

        // Position the grid
        PositionGrid();

        // Init children
        transform.GetComponentInChildren<ClickNextImage>().Initialize(transform, dataFolder);
        transform.GetComponentInChildren<Trash>().Initialize(transform);
    }

    void PositionGrid()
    {   
        // Setup anchors and pivots
        RectTransform rectTransform = GetComponent<RectTransform>();
        SetupAnchorsAndPivots(rectTransform);

        // Set side lengths of the rect transform
        rectTransform.localScale = new UnityEngine.Vector3(1, 1, 1);

        // Set distance from the camera at maximum raycast distance
        Vector3 newPosition = FacePlayer((raycast_distance * 0.9f)); // 0.9f provides a buffer, incase player moves without knowing and believes interaction is not feasible
        rectTransform.position = newPosition;

        // Set size of Grid to FOV at the maximum raycast distance
        List<float> outputs = GetFOVatWD((raycast_distance * 0.9f));

        rectTransform.sizeDelta = new UnityEngine.Vector2(outputs[1], outputs[0]);

    }


    private List<float> GetFOVatWD(float WD)
    {
        // Pythagoras theorem to calculate the distance
        List<float> holder = new List<float>();
        float vertical_fov = Camera.main.fieldOfView;
        float fov_height = (WD * Mathf.Tan(vertical_fov * 0.5f)) * 2;
        float fov_width =  Camera.main.aspect * fov_height; // Aspect ratio of the camera is width/height

        holder.Add(fov_height);
        holder.Add(fov_width);
        holder.Add(WD);

        return holder;
    }
}
