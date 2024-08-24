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

    // Start is called before the first frame update
    public void Initialize()
    {   



        // Init children
        transform.GetComponentInChildren<ClickNextImage>().Initialize(transform);
        transform.GetComponentInChildren<Trash>().Initialize(transform, transform.GetComponentInChildren<ClickNextImage>().transform, userCamera);
    }



    void PositionGrid(RectTransform rectTransform, Camera userCamera, float WD)
    {   
        // Setup anchors and pivots
        SetupAnchorsAndPivots(rectTransform);

        // Set anchor to the centre of the screen
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        // Set pivot to the centre of the screen
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Set side lengths of the rect transform
        rectTransform.localScale = new UnityEngine.Vector3(1, 1, 1);

        rectTransform.localPosition = Vector3.zero;

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
        float fov_width =  userCamera.aspect * fov_height; // Aspect ratio of the camera is width/height

        holder.Add(fov_height);
        holder.Add(fov_width);
        holder.Add(WD);

        Debug.Log($"FOV at working distance {WD}: {fov_height}, {fov_width}");

        return holder;
    }
}
