using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class GridMaker : MonoBehaviour
{
    private Camera userCamera;
    private RectTransform rectTransform;

    // Start is called before the first frame update
    public void Initialize()
    {
        // Get rect transform of the grid
        rectTransform = GetComponent<RectTransform>();

        // Get the user's camera
        userCamera = Camera.main;

        // Position the grid
        PositionGrid();

        // Set the size of the grid to the size of the canvas
        SetSize2Canvas();

        // Init children
        transform.GetComponentInChildren<Trash>().Initialize();
        transform.GetComponentInChildren<ClickNextImage>().Initialize();
    }

    void PositionGrid()
    {

        // Set side lengths of the rect transform
        rectTransform.localScale = new UnityEngine.Vector3(1, 1, 1);
        UnityEngine.Vector2 LocalPlanePosition = transform.parent.GetComponent<RectTransform>().pivot;
        //float z = transform.parent.GetComponent<RectTransform>().sizeDelta.x * transform.parent.GetComponent<RectTransform>().sizeDelta.y *
        rectTransform.localPosition = new UnityEngine.Vector3(LocalPlanePosition.x, LocalPlanePosition.y, 0);
    }


    void SetSize2Canvas()
    {
        // Set anchors to the corners of the canvas


        // Set anchor preset centre
        rectTransform.anchorMin = new UnityEngine.Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new UnityEngine.Vector2(0.5f, 0.5f);


        // Set width and height to the size of the canvas
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, transform.parent.GetComponent<RectTransform>().sizeDelta.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, transform.parent.GetComponent<RectTransform>().sizeDelta.y);
    }
}
