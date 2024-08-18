using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Transform canvas = InitializeCanvas();   
    }

    Transform InitializeCanvas()
    {
        // Load the canvas
        gameObject.name = "MenuPanel";

        // Canvas anchors to left bottom
        GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

        // Set Pivot to centre
        GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        // Set Canvas same size as fov
        GetComponent<RectTransform>().sizeDelta = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);

        // Position canvas at near clip plane
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * Camera.main.nearClipPlane;

        // Face Canvas to player
        transform.LookAt(Camera.main.transform);

        return transform;
    }





}
