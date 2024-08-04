using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

public class ClickNextImage : MonoBehaviour
{
    private InteractableImageStack Canvas_script;
    public GameObject rawImagesubsequentGO;
    private GameObject CanvasUI;
    private int subsequent_img;
    public List<String> imagePaths;
    public int current_img_indx;
    private RawImage rawImage;
    public List<Texture2D> images = new List<Texture2D>();
    public int N_image;
    private Vector3 start_position;
    private Quaternion start_rotation;

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

        // Populate the list dedicated to image textures
        getImageTextures();

        // Initialize the image
        InitializeCurrentImage(current_img_indx);

        SetNativeAndColliderSize(gameObject);
        
        // Create and display second image
        CreateGOForSubsequentImage(N_image);
    }

    private void SetNativeAndColliderSize(GameObject gameObject)
    {
        // SHould only be executed after first texture has been assigned as native size is not dnymaic
        // Set the collider size
        gameObject.GetComponent<RawImage>().SetNativeSize();
        SetColliderSize(gameObject);
    }

    private void getImageTextures()
    {
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


    private void PopulateVariables()
    {

    // Confirm what variables are in the script
    Canvas_script = transform.parent.parent.GetComponent<InteractableImageStack>();

    // Find object in scene
    CanvasUI = GameObject.Find("Canvas UI").gameObject;

    // Get the start position and rotation of the RawImage
    current_img_indx = 0;

    // Get the RawImage component
    rawImage = GetComponent<RawImage>();


    // Set Image to centre with same angle
    start_position = new Vector3(1, 1, 0);
    start_rotation = Canvas_script.GetComponent<RectTransform>().rotation;;


    }

    public void InitializeCurrentImage(int current_img_indx)
    {

        transform.localPosition = new Vector3(transform.localPosition.x * start_position.x, transform.localPosition.y * start_position.y, transform.localPosition.z * start_position.z);
        transform.rotation = start_rotation;

        rawImage.texture = images[current_img_indx];
        UpdateImageName(gameObject, current_img_indx);
        
    }

    private void SetColliderSize(GameObject gameObject)
    {
        float width = gameObject.GetComponent<RectTransform>().rect.width;
        float height = gameObject.GetComponent<RectTransform>().rect.height;
        gameObject.GetComponent<BoxCollider>().size = new Vector3(width, height, 0);

    }




    public void UpdateImageName(GameObject rawImagecurrent, int current_img_indx)
    {
        rawImagecurrent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format("Patch {0}/{1}", current_img_indx + 1, N_image);
    }





    public void CreateGOForSubsequentImage(int N_images)
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
            rawImagesubsequentGO.SetActive(false);

            Create_button_next2image(x_scale: 1f, y_scale: 0.45f);
        }

        else
        {
            Debug.Log("Only one image available");
        }
    }



    public void DisplaySecondImage()
    {
        if (this.gameObject != null && rawImagesubsequentGO != null)
        {
            subsequent_img = Canvas_script.current_img_indx;

            if (subsequent_img < (Canvas_script.images.Count - 1))
            {
                subsequent_img += 1;
            }
            else
            {
                subsequent_img = 0;
            }

            rawImagesubsequentGO.GetComponent<RawImage>().texture = Canvas_script.images[subsequent_img];
            rawImagesubsequentGO.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format("Patch {0}/{1}", subsequent_img + 1, Canvas_script.N_image);
            rawImagesubsequentGO.SetActive(true);

        }
        else
        {
            Debug.Log("RawImageCurrent is null");
        }
    }

public void Create_button_next2image(float x_scale = 0.8f, float y_scale = 1f)
{   


    if (CanvasUI == null)
    {
            // Create a button to display the next image
    string prefab_path = "Assets/Scenes/CIAnnotator/Canvas UI.prefab";
    CanvasUI = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefab_path), transform.position, transform.rotation);

    }

    else
    {
        CanvasUI.transform.position = transform.position;
        CanvasUI.transform.rotation = transform.rotation;
    }

    CanvasUI.transform.localScale = Vector3.one;
    CanvasUI.transform.position += Get_Axes_Offsets(x_scale, y_scale);

    GameObject button = CanvasUI.transform.Find("LocatePatch").gameObject;

    if (button == null)
    {
            // Create a button to display the next image
    string prefab_path = "Assets/Scenes/CIAnnotator/Button.prefab";
    button = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefab_path), transform.position, transform.rotation);
    }

    else
    {
        button.transform.position = transform.position;
        button.transform.rotation = transform.rotation;
    }


    button.transform.position += Get_Axes_Offsets(x_scale, y_scale);
    
}


private Vector3 Get_Axes_Offsets(float x_scale, float y_scale)
{

    // Get the width and height of the RawImage
    float width = GetComponent<RectTransform>().rect.width;
    float height = GetComponent<RectTransform>().rect.height;


    float scaled_width = width* GetComponent<RectTransform>().localScale.x;
    float scaled_height = height * GetComponent<RectTransform>().localScale.y;

    // Calculate the x and y offsets
    float x_offset = scaled_width  * x_scale;
    float y_offset = scaled_height * y_scale;

    return new Vector3(x_offset, y_offset, 0);




}


    private void Update()
    {

    }
}


