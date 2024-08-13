using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
// Import functions from another script
using static InteractableImageStack;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit; // With a static directive, you can access the members of the class by using the class name itself

public class ClickNextImage : MonoBehaviour
{
    public GameObject rawImagesubsequentGO;
    private GameObject CanvasUI;
    private int subsequent_img;
    public List<String> imagePaths;
    public int current_img_indx;
    private RawImage rawImage;
    public List<Texture2D> images = new List<Texture2D>();
    public int N_image;
    public Vector3 start_position;
    public Quaternion start_rotation;
    public InteractableImageStack Canvas_script;

   public void Initialize()
    {

        if (gameObject == null)
        {   
            string prefabPath = "Assets/Scenes/CIAnnotator/Image.prefab";
            Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), transform);
            gameObject.SetActive(true);
        }

        gameObject.name = "Image";

        PopulateVariables();

        // Position and populate the list dedicated to image textures
        getImageTextures();

        // Initialize the image
        InitializeCurrentImage(current_img_indx);
        
        // Create and display second image
        CreateGameObjectForSecondImage(N_image);


        // Add function to select entered listener
        GetComponent<XRGrabInteractable>().selectEntered.AddListener((args) => DisplaySecondImage());




    }


void PositionImageStack()
{   
    // Set the anchors and pivots of the Image
    SetupAnchorsAndPivots(transform.GetComponent<RectTransform>());

    // Set the anchors and pivots of the Canvas as sizeDelta requires absolute difference
    transform.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
    transform.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

    // Set side lengths of the rect transform
    transform.localScale = new UnityEngine.Vector3(1, 1, 1);

    // Face Image to player
    start_position =  transform.parent.position + transform.parent.forward * 0.01f;
    start_rotation = transform.parent.rotation;

}

public void PositionResizeText(RectTransform rectTransform, int current_img_indx, int N_image)
{
    // Set the anchors and pivots of the Text
    RectTransform textRectTransform = rectTransform.GetChild(0).GetComponent<RectTransform>();
    SetupAnchorsAndPivots(textRectTransform);

    // Set the anchors and pivots of the Text as sizeDelta requires absolute difference
    textRectTransform.anchorMin = new Vector2(0, 0);
    textRectTransform.anchorMax = new Vector2(0, 0);

    // Set pivot to bottom center of the Text
    textRectTransform.pivot = new Vector2(0.5f, 0);

    // Set side lengths of the rect transform
    textRectTransform.localScale = Vector3.one;

    // Set the position and rotation of the child transform
    textRectTransform.SetPositionAndRotation(rectTransform.position, rectTransform.rotation);

    // Set the size of the Text to be 1/3 of the width of the image
    textRectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y/3);

    // Set the font size of the Text same to width of image
    textRectTransform.GetComponent<TextMeshProUGUI>().fontSize = rectTransform.sizeDelta.x * 0.1f;

    // Set the text of the Text
    textRectTransform.GetComponent<TextMeshProUGUI>().text = string.Format("Patch {0}/{1}", current_img_indx + 1, N_image);

    // Position to top center of the RawImage
    textRectTransform.position = new Vector3(rectTransform.position.x, rectTransform.sizeDelta.y/2, rectTransform.position.z);

    // Centre text in the Text
    textRectTransform.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Bottom;


}

private void ResizeImgtobewithin40percentofFOV(float WD)
{

    // Get the FOV at the panel height
    List<float> outputs = GetFOVatWD(WD);
    
    float newWidth = outputs[0]*0.4f; // Height
    float newHeight = outputs[1]*0.4f; // Width

    // Get the width and height of the RawImage
    float width = GetComponent<RawImage>().texture.width;
    float height = GetComponent<RawImage>().texture.height;

    // Reduce image size whilst keeping the image aspect ratio
    float aspect_ratio = width/height;

    // Adjust the dimensions to maintain the aspect ratio
    if (newWidth > newHeight * aspect_ratio)
    {
        newWidth = newHeight * aspect_ratio; // Aspect ratio is 1, so newWidth = newHeight
    }
    else
    {
        newHeight = newWidth / aspect_ratio; // Aspect ratio is 1, so newHeight = newWidth
    }

    Debug.Log("New Width: " + newWidth + " New Height: " + newHeight);
    // Set width and height of the Canvas
    RectTransform rectTransform = GetComponent<RectTransform>();

    rectTransform.sizeDelta = new UnityEngine.Vector2(newWidth, newHeight);

}

    private void PopulateVariables()
    {

    // Get the start position and rotation of the RawImage
    current_img_indx = 0;

    // Get the RawImage component
    rawImage = GetComponent<RawImage>();

    }


    private void getImageTextures()
    {
        PositionImageStack();

        var ext = new List<string> { "jpg", "gif", "png" };
        imagePaths = Directory.EnumerateFiles(Application.dataPath + "/Resources/test_imgs", "*", SearchOption.AllDirectories).ToList();
        imagePaths = imagePaths.Where(path =>
        {
            string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            return ext.Contains(extension);
        }).ToList();

        foreach (string imagePath in imagePaths)
        {
            images.Add(Resources.Load<Texture2D>("test_imgs/" + Path.GetFileNameWithoutExtension(imagePath)));
        }

        N_image = images.Count;
    }


    public void InitializeCurrentImage(int current_img_indx)
    {

        transform.position = start_position;
        transform.rotation = start_rotation;

        rawImage.texture = images[current_img_indx];

        // Resize the image to be within 40% of the FOV
        ResizeImgtobewithin40percentofFOV(rawImage.transform.position.z);

        // Set the collider size
        SetColliderSize(rawImage);

        // Position and resize the text
        PositionResizeText(rawImage.transform.GetComponent<RectTransform>(), current_img_indx, N_image);

        // Set non maskable to true
        rawImage.GetComponent<RawImage>().maskable = false;
        
    }

    private void SetColliderSize(RawImage rawImage)
    {
        rawImage.GetComponent<BoxCollider>().size = GetComponent<RectTransform>().sizeDelta;

    }





private void CreateGameObjectForSecondImage(int N_images)
    {

        // Create subsequent image only when there are more than one images
        if (N_images > 1)
        {   
            // Step 1: Define the path to the prefab within the Resources folder
            string prefabPath = "Assets/Scenes/CIAnnotator/SubsequentImage.prefab";
            // Create a new RawImage GameObject from the prefab

            if (rawImagesubsequentGO == null)
            {
                rawImagesubsequentGO = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), transform.position, transform.rotation);
                rawImagesubsequentGO.GetComponent<RawImage>().SetNativeSize();
                
            }

            else
            {
                rawImagesubsequentGO.transform.position = transform.position;
                rawImagesubsequentGO.transform.rotation = transform.rotation;
            }
            rawImagesubsequentGO.name = "SubsequentImage";
            rawImagesubsequentGO.transform.SetParent(transform.parent);
            rawImagesubsequentGO.transform.position = start_position;
            rawImagesubsequentGO.transform.rotation = start_rotation;
            rawImagesubsequentGO.SetActive(false);
            rawImagesubsequentGO.GetComponent<RawImage>().maskable = false;
            rawImagesubsequentGO.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
            rawImagesubsequentGO.GetComponent<RectTransform>().localScale = GetComponent<RectTransform>().localScale;
        }

        else
        {
            Debug.Log("Only one image available");
        }
    }

private void DisplaySecondImage()
{
        if (this.gameObject != null && rawImagesubsequentGO != null)
        {
            
            subsequent_img = current_img_indx;

            if (subsequent_img < (images.Count))
            {
                subsequent_img += 1;
            }
            else
            {
                subsequent_img = 0;
            }

            rawImagesubsequentGO.GetComponent<RawImage>().texture = images[subsequent_img];
            PositionResizeText(rawImagesubsequentGO.transform.GetComponent<RectTransform>(), subsequent_img, N_image);
            rawImagesubsequentGO.SetActive(true);
            

        }
        else
        {
            Debug.Log("RawImageCurrent is null");
        }
    }




}


