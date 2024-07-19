using System.Collections;
using System.Collections.Generic;
using System.IO; // Add this line to include the System.IO namespace
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;


public class InteractableImageStack : MonoBehaviour
{
    //public RawImage[] imageDisplays; // Array to store image displays
    public List<Texture2D> images = new List<Texture2D>();
    private List<string> imagePaths;
    private GameObject rawImagecurrent;
    public int current_img_indx = 0;
    public int N_image;


    private void Start()
    {   


    // Set Canvas at acceptable viewing position
    setCanvasPosition();

    // Populate the list dedicated to image textures
    getImageTextures();

    // Get preloaded image 
    rawImagecurrent = transform.Find("Image").gameObject;

    // Initialize the image

    init_current_img(rawImagecurrent, current_img_indx);


    }

    //Blebs should be counted using activation and be cuncurrent with current_img

    public void init_current_img(GameObject rawImagecurrent, int current_img_indx)
    {   
        if (rawImagecurrent == null)

        {
        rawImagecurrent = Instantiate(rawImagecurrent, transform);}

        float width = rawImagecurrent.GetComponent<RectTransform>().rect.width;
        float height = rawImagecurrent.GetComponent<RectTransform>().rect.height;
        rawImagecurrent.GetComponent<BoxCollider>().size = new Vector3(width, height, 0);
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

    {   float Width = GetComponent<RectTransform>().rect.width;
        float Height = GetComponent<RectTransform>().rect.height;
        float viewingDistance = CalculateViewingDistance(Mathf.Sqrt(Mathf.Pow(Width, 2) + Mathf.Pow(Height, 2)), 25);
        Vector3 canvas_position = Camera.main.transform.TransformPoint(Vector3.forward * viewingDistance);
        transform.position = canvas_position;
    }


    float CalculateViewingDistance(float objectSize, float visualAngle)
    {
        // Convert visual angle from degrees to radians
        float thetaRadians = visualAngle * Mathf.Deg2Rad;

        // Calculate viewing distance using the rearranged formula
        float viewingDistance = objectSize / (2f * Mathf.Tan(thetaRadians / 2f));

        return viewingDistance;
    }


        //private int currentImageIndex; // Index of the currently displayed image


 

    


}
