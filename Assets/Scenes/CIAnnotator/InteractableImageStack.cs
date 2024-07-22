using System.Collections;
using System.Collections.Generic;
using System.IO; // Add this line to include the System.IO namespace
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System;
using System.IO;
using Unity.XR.CoreUtils;
using System.Numerics;

public class InteractableImageStack : MonoBehaviour
{
    //public RawImage[] imageDisplays; // Array to store image displays
    public List<Texture2D> images;
    private List<string> imagePaths;
    private GameObject rawImagecurrent;
    public int current_img_indx = 0;
    public int N_image;
    private GameObject whole_img;
    private Texture2D whole_img_texture;

    private void Start()
    {   


    // Set Canvas at acceptable viewing position
    setCanvasPosition();

    // Populate the list dedicated to image textures
    getImageTextures();

    // Load images asynchronously 
    // StartCoroutine(LoadImages()); Does not work for some reados although we count the number of images

    Debug.Log(string.Format("Number of images {0}", imagePaths.Count));

    // Get preloaded image 
    rawImagecurrent = transform.Find("Image").gameObject;

    // Initialize the image
    init_current_img(rawImagecurrent, current_img_indx);

    create_and_display_whole_image();


    }

    //Blebs should be counted using activation and be cuncurrent with current_img

    public void init_current_img(GameObject rawImagecurrent, int current_img_indx)
    {   

        if (rawImagecurrent == null)

        {
        rawImagecurrent = Instantiate(rawImagecurrent, transform);}

        float width = rawImagecurrent.GetComponent<RectTransform>().rect.width;
        float height = rawImagecurrent.GetComponent<RectTransform>().rect.height;
        rawImagecurrent.GetComponent<BoxCollider>().size = new  UnityEngine.Vector3(width, height, 0);
        rawImagecurrent.GetComponent<RawImage>().texture = images[current_img_indx];
        rawImagecurrent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format("Patch {0}/{1}", current_img_indx + 1, N_image);
        
    }

    private void getImageTextures()
    {
        var ext = new List<string> { "jpg", "gif", "png" };
        imagePaths = Directory.EnumerateFiles(Application.dataPath + "/Resources/test_imgs", "*", SearchOption.AllDirectories).ToList();
        imagePaths = imagePaths.Where(path => {string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant(); return ext.Contains(extension);}).ToList();

        Debug.Log(string.Format("Number of images {0}", imagePaths.Count));

       // float radius = 0.5f; // Adjust radius based on desired size
        foreach  (string imagePath in imagePaths) //Application.dataPath is a built-in Unity variable that provides the path to the main folder of your project on the device where it's running.
        {   
            //if (Path.GetExtension(imagePath).Equals(".png", System.StringComparison.OrdinalIgnoreCase))
            images.Add(Resources.Load<Texture2D>("test_imgs/" + Path.GetFileNameWithoutExtension(imagePath)));

            //This part extracts a substring from the imagePath. It removes the part of the path that matches the project's data path.
            //Resources.Load is a Unity function that allows you to load assets like textures directly from the Resources folder at runtime.
        }

        N_image = images.Count;

    }


    private void setCanvasPosition()

    { 
        float viewingDistance = CalculateViewingDistance(this.gameObject, 25);
         UnityEngine.Vector3 canvas_position = Camera.main.transform.TransformPoint( UnityEngine.Vector3.forward * viewingDistance);
        transform.position = canvas_position;
    }


    float CalculateViewingDistance(GameObject instance, float visualAngle)
    {

        float Width = instance.GetComponent<RectTransform>().rect.width;
        float Height = instance.GetComponent<RectTransform>().rect.height;

        float objectSize = Mathf.Sqrt(Mathf.Pow(Width, 2) + Mathf.Pow(Height, 2));


        // Convert visual angle from degrees to radians
        float thetaRadians = visualAngle * Mathf.Deg2Rad;

        // Calculate viewing distance using the rearranged formula
        float viewingDistance = objectSize / (2f * Mathf.Tan(thetaRadians / 2f));

        return viewingDistance;
    }


        //private int currentImageIndex; // Index of the currently displayed image

    public static (int width, int height) GetDimensions(string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            // Skip PNG signature (8 bytes)
            stream.Seek(8, SeekOrigin.Begin);

            // Read IHDR chunk length (4 bytes) and type (4 bytes)
            byte[] chunkLength = new byte[4];
            byte[] chunkType = new byte[4];
            stream.Read(chunkLength, 0, 4);
            stream.Read(chunkType, 0, 4);

            // Ensure we are reading the IHDR chunk
            string type = System.Text.Encoding.ASCII.GetString(chunkType);
            if (type != "IHDR")
            {
                throw new Exception("IHDR chunk not found");
            }

            // Read IHDR chunk data (width and height)
            byte[] dimensions = new byte[8];
            stream.Read(dimensions, 0, 8);

            // Convert byte arrays to integers (width and height)
            int width = BitConverter.ToInt32(new byte[] { dimensions[3], dimensions[2], dimensions[1], dimensions[0] }, 0);
            int height = BitConverter.ToInt32(new byte[] { dimensions[7], dimensions[6], dimensions[5], dimensions[4] }, 0);

            return (width, height);
        }
    }


    private Texture2D LoadTexture()
    {   string path = "Assets/Resources/whole_img.png";
        Debug.Log($"Path exists: {File.Exists(path)}");
        (int width, int height) = GetDimensions(path);
        Debug.Log($"Give us the dims: {(width, height)}");
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(width, height);
        texture.LoadImage(fileData); // LoadImage is a built-in Unity function that loads an image from a byte array
        return texture;
    }
    private GameObject set_texture2whole_img(GameObject whole_img)
    {
        whole_img_texture = LoadTexture();
        if (whole_img_texture == null)
        {
            Debug.Log("Whole image texture is null");
        }

        whole_img.GetComponent<RawImage>().texture = whole_img_texture;
        // Adjust the dimensions of the RawImage to match the texture's dimensions
        RectTransform rectTransform = whole_img.GetComponent<RawImage>().GetComponent<RectTransform>();
        rectTransform.sizeDelta = new  UnityEngine.Vector2(whole_img_texture.width, whole_img_texture.height);

        return whole_img;
    }
 
    private void create_and_display_whole_image()
    {
        // Create subsequent image only when there are more than one images
            if (images.Count > 1){

            whole_img = transform.Find("Whole Image").gameObject;

            if (whole_img == null)
            {
            // Step 1: Define the path to the prefab within the Resources folder
            string prefabPath = "Assets/Scenes/CIAnnotator/SubsequentImage.prefab";
            // Create a new RawImage GameObject from the prefab
            whole_img = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), transform.position, transform.rotation);
            whole_img.SetActive(true);
            }
            if (whole_img != null)
            {
            whole_img.name = "Whole Image";
            whole_img.transform.SetParent(transform);
            whole_img = set_texture2whole_img(whole_img);
             //UnityEngine.Vector3 position = situate2right_of_displayedimage(0.0004f, 0.004f, 1, 40, whole_img);
            // set to the right of the current image
            //whole_img.transform.position = position;
            

    }
    else {
        Debug.Log("Whole image is null");
    }

            }

    }


    private UnityEngine.Vector3 situate2right_of_displayedimage(float x_scale, float y_scale, float z_scale, float visualAngle, GameObject instance)
    {

         UnityEngine.Vector3 image_position = rawImagecurrent.GetComponent<RectTransform>().position;
        float width = rawImagecurrent.GetComponent<RectTransform>().rect.width;
        float height = rawImagecurrent.GetComponent<RectTransform>().rect.height;
        float x_shift = (width / 100)/x_scale;
        float y_shift = (height / 100)/y_scale;
        
        float z_shift = CalculateViewingDistance(instance, visualAngle)*z_scale;
         UnityEngine.Vector3 position = new  UnityEngine.Vector3(image_position.x + x_shift, image_position.y + y_shift, image_position.z + z_shift);
        whole_image_title(position, instance);
        return position;

    }
    
    private void whole_image_title( UnityEngine.Vector3 image_position, GameObject whole_image)
    {

        TMP_Text tmpText = whole_image.GetComponentInChildren<TMP_Text>();

        tmpText.text = "Whole image";
        tmpText.fontSize = 600;
        tmpText.alignment = TextAlignmentOptions.Center;
        //float scaledHeight = whole_image.GetComponent<RectTransform>().rect.height * whole_image.GetComponent<RectTransform>().localScale.y;
        tmpText.GetComponent<RectTransform>().position = new  UnityEngine.Vector3(399, 5660, -1928);
        //RectTransform rectTransform = tmpText.GetComponent<RectTransform>();
        //rectTransform.sizeDelta = new  UnityEngine.Vector2(600, 300);
        
    }
            

}
