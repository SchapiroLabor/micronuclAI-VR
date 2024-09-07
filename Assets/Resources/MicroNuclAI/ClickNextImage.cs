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

using System.Web;
using Unity.Mathematics; // With a static directive, you can access the members of the class by using the class name itself


public class ClickNextImage : MonoBehaviour
{
    public GameObject rawImagesubsequentGO;
    private int subsequent_img;
    public List<String> imagePaths;
    public List<string> img_names;
    public int current_img_indx;
    private RawImage rawImage;
    public GameObject GameManager;
    public List<Texture2D> images = new List<Texture2D>();
    public int N_image;
    public Vector3 start_position = new Vector3(0, 0, 0);
    public Quaternion start_rotation = Quaternion.Euler(0, 0, 0);
    private Camera userCamera;
    private float raycast_distance = 10f; // Default distance to raycast from the camera, please do not change this !!



    // All functions independet of other objects can be placed in even functions Awake, OnEnable, Start

    void Awake()
    {
        // Function plays when the script is loaded

        if (gameObject == null)
        {   
            string prefabPath = Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/Image.prefab"));
            Instantiate(Resources.Load<GameObject>(prefabPath));
        }

        gameObject.name = "Image";

                // Load the Game Manager
        if (GameManager == null)
        {
            // Load from path
            GameManager = Resources.Load<GameObject>(Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/SceneManager.prefab")));
        }

    userCamera = Camera.main;

    // Function plays when the script is loaded
    PopulateVariables();

    // Start the coroutine
    StartCoroutine(MyCoroutine(GameManager.GetComponent<GameManaging>().InputFolder));


    // Initialize the image
    InitializeCurrentImage(current_img_indx, userCamera, start_position, start_rotation);

    }

private System.Collections.IEnumerator MyCoroutine(string data_dir)
{
    // Remove the call to WaitForWholeImage since it is not being used
    getImageTextures(data_dir);
    yield return null; // Wait for the next frame
}




   public void Initialize(Transform parent)
    {

        gameObject.transform.SetParent(parent);
        gameObject.SetActive(true);

        // Do not trust child to be initialized during Start()

        // Create and display second image
        CreateGameObjectForSecondImage(N_image, transform);

        // Add function to select entered listener
        GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>().selectEntered.AddListener((args) => DisplaySecondImage());

    }




void PositionImageStack()
{   
    // Set the anchors and pivots of the Image
    SetupAnchorsAndPivots(transform.GetComponent<RectTransform>());

    // Set the anchors to the centre of the screen
    transform.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
    transform.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);

    // Set the pivot to the centre of the screen
    transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

    // Set side lengths of the rect transform
    transform.localScale = new UnityEngine.Vector3(1, 1, 1);

}

public void PositionResizeText(RectTransform CurrentImage, int current_img_indx, int N_image)
{
    // Set the anchors and pivots of the Text
    RectTransform textRectTransform = CurrentImage.Find("Image_ID").GetComponent<RectTransform>();

    // Set the anchors and pivots of the Text as sizeDelta requires absolute difference
    textRectTransform.anchorMin = new Vector2(0, 0);
    textRectTransform.anchorMax = new Vector2(0, 0);

    // Set pivot to bottom center of the Text
    textRectTransform.pivot = new Vector2(0.5f, 0.0f);

    // Set side lengths of the rect transform
    textRectTransform.localScale = Vector3.one;

    // Set the position and rotation of the child transform
    textRectTransform.SetPositionAndRotation(CurrentImage.position, CurrentImage.rotation);

    // Set the size of the Text to be 1/3 of the width of the image
    textRectTransform.sizeDelta = new Vector2(CurrentImage.sizeDelta.x, CurrentImage.sizeDelta.y/3);

    // Set the font size of the Text same to width of image
    textRectTransform.GetComponent<TextMeshProUGUI>().fontSize = CurrentImage.sizeDelta.x * 0.1f;

    // Set the text of the Text
    textRectTransform.GetComponent<TextMeshProUGUI>().text = string.Format("Patch {0}/{1}", current_img_indx + 1, N_image);

    // Centre text in the Text
    textRectTransform.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Bottom;

    // Position to top center of the RawImage
    // Uneexpected behaviour when using localPosition sizeDelta is Gemobject size + parent size from pivot
    textRectTransform.localPosition = new Vector3((CurrentImage.sizeDelta.x/2) - (CurrentImage.sizeDelta.x*(1 - textRectTransform.pivot.x)),
    CurrentImage.sizeDelta.y - ((CurrentImage.sizeDelta.y/2)*(1-textRectTransform.pivot.y)), 0);


}

private void ResizeImgtobewithin40percentofFOV(float WD, Camera userCamera)
{

    // Get the FOV at the panel height
    List<float> outputs = GetFOVatWD(WD, userCamera);
    
    float newWidth = outputs[0]*0.4f; // Height
    float newHeight = outputs[1]*0.4f; // Width

    // Get the width and height of the RawImage
    float width = rawImage.texture.width;
    float height = rawImage.texture.height;

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

    // Set width and height of the Canvas
    RectTransform rectTransform = GetComponent<RectTransform>();

    rectTransform.sizeDelta = new UnityEngine.Vector2(newWidth, newHeight);

    Debug.Log($"The size of the image is: {newWidth}, {newHeight}");

}

    private void PopulateVariables()
    {

    // Get the start position and rotation of the RawImage
    current_img_indx = 0;

    }
    public (int width, int height) GetDimensions(string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            stream.Seek(8, SeekOrigin.Begin);

            byte[] chunkLength = new byte[4];
            byte[] chunkType = new byte[4];
            stream.Read(chunkLength, 0, 4);
            stream.Read(chunkType, 0, 4);

            string type = System.Text.Encoding.ASCII.GetString(chunkType);
            if (type != "IHDR")
            {
                throw new Exception("IHDR chunk not found");
            }

            byte[] dimensions = new byte[8];
            stream.Read(dimensions, 0, 8);

            int width = BitConverter.ToInt32(new byte[] { dimensions[3], dimensions[2], dimensions[1], dimensions[0] }, 0);
            int height = BitConverter.ToInt32(new byte[] { dimensions[7], dimensions[6], dimensions[5], dimensions[4] }, 0);

            return (width, height);
        }
    }    private Texture2D LoadImgwithAbsolutePath(string path)
    {

        // Load the image with absolute path
        byte[] fileData = File.ReadAllBytes(path);
        (int width, int height) =GetDimensions(path);
        Texture2D tex = new Texture2D(width, height);
        bool isLoaded = tex.LoadImage(fileData);

        if (!isLoaded)
        {
            Debug.Log("Texture did not load !!!");


        }

        return tex;


    }

    private void getImageTextures(string dataFolder)
    {
        PositionImageStack();

        var ext = new List<string> { "jpg", "gif", "png" };
        imagePaths = Directory.EnumerateFiles(Path.Combine(dataFolder, "patches"),  "*", SearchOption.AllDirectories).ToList();
        imagePaths = imagePaths.Where(path =>
        {
            string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            return ext.Contains(extension);
        }).ToList();

        foreach (string imagePath in imagePaths)
        {
            images.Add(LoadImgwithAbsolutePath(imagePath));
            img_names.Add(Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension(imagePath)));
        }

        N_image = images.Count;
        Debug.Log(string.Format("Number of images: {0}", N_image));
    }


    public void InitializeCurrentImage(int current_img_indx, Camera userCamera, Vector3 StartPosition, quaternion StartRotation)
    {

        transform.localPosition = StartPosition;
        transform.rotation = StartRotation;

        // Get the RawImage component
        GetComponent<RawImage>().texture = images[current_img_indx];

        rawImage = GetComponent<RawImage>();

        // Resize the image to be within 40% of the FOV
        ResizeImgtobewithin40percentofFOV(raycast_distance, userCamera);

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





private void CreateGameObjectForSecondImage(int N_images, Transform CurrentImage)
    {

        // Create subsequent image only when there are more than one images
        if (N_images > 1)
        {   

            // Create a new RawImage GameObject from the prefab

            if (rawImagesubsequentGO == null)
            {
                rawImagesubsequentGO = Instantiate(Resources.Load<GameObject>(Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/SubsequentImage.prefab"))), transform.position, transform.rotation);
                rawImagesubsequentGO.GetComponent<RawImage>().SetNativeSize();
                
            }

            Debug.Log("RawImageSubsequent is not null");

            rawImagesubsequentGO.name = "SubsequentImage";
            rawImagesubsequentGO.transform.SetParent(CurrentImage.parent);
            rawImagesubsequentGO.transform.localPosition = start_position;
            rawImagesubsequentGO.transform.rotation = start_rotation;
            rawImagesubsequentGO.SetActive(false);
            rawImagesubsequentGO.GetComponent<RawImage>().maskable = false;
            rawImagesubsequentGO.GetComponent<RectTransform>().sizeDelta = CurrentImage.GetComponent<RectTransform>().sizeDelta;
            rawImagesubsequentGO.GetComponent<RectTransform>().localScale = CurrentImage.GetComponent<RectTransform>().localScale;
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

            if (subsequent_img < (images.Count - 1))
            {
                subsequent_img += 1;

            rawImagesubsequentGO.GetComponent<RawImage>().texture = images[subsequent_img];
            PositionResizeText(rawImagesubsequentGO.transform.GetComponent<RectTransform>(), subsequent_img, N_image);
            rawImagesubsequentGO.SetActive(true);
            }



        }
        else
        {
            Debug.Log("RawImageCurrent is null");
        }
    }




}


