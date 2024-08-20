using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static InteractableImageStack;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using System.IO;
using Vector4 = UnityEngine.Vector4;


public class SetupButtons : MonoBehaviour

{   float fontSize;
    UnityEngine.Vector2 buttonSize;
    UnityEngine.Vector3 buttonPosition;
    UnityEngine.Quaternion buttonRoation;

    Transform CanvasUI;

    // Start is called before the first frame update
     public void Initialize(Transform ImagePatch, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation,
     Transform WholeImage, Transform Trash)
    {
     fontSize = ImagePatch.GetComponent<RectTransform>().sizeDelta.x * 0.1f;
     buttonPosition = position;
     buttonRoation = rotation;

     // Add content size fitter componet
    gameObject.AddComponent<ContentSizeFitter>();
    gameObject.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
    gameObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

    // Add vertical layout group component
    gameObject.AddComponent<VerticalLayoutGroup>();
    gameObject.GetComponent<VerticalLayoutGroup>().childControlWidth = false;
    gameObject.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
    gameObject.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
    gameObject.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
    gameObject.GetComponent<VerticalLayoutGroup>().spacing = fontSize * 0.2f;

    buttonSize = ResizeButton(ImagePatch);

    setupLocatePatchButton(WholeImage);
    setupReverseButton(Trash);
    setupAddBinButton(Trash, ImagePatch);
    
        
    }

    private Vector2 ResizeButton(Transform ImagePatch)

    {
    
    // Get the width and height of the RawImage
    float width = ImagePatch.GetComponent<RectTransform>().rect.width;
    float height = ImagePatch.GetComponent<RectTransform>().rect.height;

    float scaled_width = width* ImagePatch.GetComponent<RectTransform>().localScale.x;
    float scaled_height = height * ImagePatch.GetComponent<RectTransform>().localScale.y;

    // Set the size of the Canvas UI to 1/3 of width of image with aspect ratio of 3:1
     return new UnityEngine.Vector2(scaled_width/3, scaled_width/6);

    }

    private void setupLocatePatchButton(Transform WholeImage)

    {

    GameObject LocatePatch = Resources.Load<GameObject>(Path.Combine("MicroNuclAI", Path.GetFileNameWithoutExtension("MicroNuclAI/Button.prefab")));

    // Instantiate
    LocatePatch = Instantiate(LocatePatch, transform);

    // Set name
    LocatePatch.name = "Locate Patch";

    standardiseButton(LocatePatch);

    // Set text of the button
    LocatePatch.GetComponentInChildren<TextMeshProUGUI>().text = "Locate Patch";

    // Add a listener to the button
    LocatePatch.GetComponent<Button>().onClick.AddListener(() => WholeImage.GetComponentInChildren<whole_image>().DisplayPatch());

    }


private void setupReverseButton(Transform Trash)

    {

    GameObject ReverseButton = Resources.Load<GameObject>(Path.Combine("MicroNuclAI", Path.GetFileNameWithoutExtension("MicroNuclAI/Button.prefab")));
    
    // Instantiate
    ReverseButton = Instantiate(ReverseButton, transform);

    // Add name
    ReverseButton.name = "Reverse";

    standardiseButton(ReverseButton);

    // Set text of the button
    ReverseButton.GetComponentInChildren<TextMeshProUGUI>().text = "Undo";

    // Add a listener to the button
    ReverseButton.GetComponent<Button>().onClick.AddListener(() => Trash.GetComponentInChildren<Trash>().ReverseDispose());
    
    }


private void setupAddBinButton(Transform Trash, Transform ImagePatch)

    {

    GameObject AddBin = Resources.Load<GameObject>(Path.Combine("MicroNuclAI", Path.GetFileNameWithoutExtension("MicroNuclAI/Button.prefab")));
    AddBin = Instantiate(AddBin, transform);

    // Add name
    AddBin.name = "Add Bin";

    standardiseButton(AddBin);

    // Set text of the button
    AddBin.GetComponentInChildren<TextMeshProUGUI>().text = "Add Bin";

    // Add a listener to the button
    AddBin.GetComponent<Button>().onClick.AddListener(() => Trash.GetComponentInChildren<Trash>().CreateBucket());
    
    }


private void standardiseButton(GameObject Button)
    {
    // Set the font size of the Button same to width of image
    Button.GetComponentInChildren<TextMeshProUGUI>().fontSize = fontSize;

    // Set location
    Button.transform.rotation = buttonRoation;

    // Set the size of the Canvas UI to 1/3 of width of image with aspect ratio of 3:1
    Button.GetComponent<RectTransform>().sizeDelta = buttonSize;

    // Set alginment of the text in the button
    Button.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Give button black background and white text
    Button.GetComponent<Image>().color = Color.black;
    Button.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

    // Set text margins to 0
    Button.GetComponentInChildren<TextMeshProUGUI>().margin = new Vector4(0, 0, 0, 0);

    SetupAnchorsAndPivots(Button.GetComponent<RectTransform>());

    // Setup anchors to bottom left corner
    Button.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
    Button.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

        // Set pivot to top right left corner
    Button.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

    // Scale all to one
    Button.transform.localScale = Vector3.one;

    Button.transform.localPosition =  new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y, 0);

    }





}
