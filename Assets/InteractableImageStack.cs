using System.Collections;
using System.Collections.Generic;
using System.IO; // Add this line to include the System.IO namespace
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;



public class InteractableImageStack : MonoBehaviour
{
    //public RawImage[] imageDisplays; // Array to store image displays
    public int current_img = 0;
    public int subsequent_img;
    public int n_imgs;
    public List<Texture2D> images = new List<Texture2D>();
    private List<string> imagePaths;
    public Vector3 canvas_position;
    public GameObject rawImagecurrent;
    public GameObject rawImagesubsequentprefab;
    private GameObject rawImagesubsequent;

    private void Start()
    {   


    // Set Canvas at acceptable viewing position
    setCanvasPosition();

    // Populate the list dedicated to image textures
    getImageTextures();

    init_current_img();

    create_subsequent_img();

    
    }

    private void init_current_img()
    {
        
        float width = rawImagecurrent.GetComponent<RectTransform>().rect.width;
        float height = rawImagecurrent.GetComponent<RectTransform>().rect.height;
        rawImagecurrent.GetComponent<BoxCollider>().size = new Vector3(width, height, 0);
        Debug.Log(string.Format("Size of 3D Collider {0}", rawImagecurrent.GetComponent<BoxCollider>().size));

        rawImagecurrent.GetComponent<RawImage>().texture = images[current_img];

    }


    private  void create_subsequent_img()
    {   
        if (n_imgs > 1){

        // Create a new RawImage GameObject from the prefab
        rawImagesubsequent = Instantiate(rawImagesubsequentprefab,  transform, true);

        rawImagesubsequent.transform.position = rawImagecurrent.transform.position;
        rawImagesubsequent.SetActive(false);


        }
    }


    void Update()
    {

    displaysecondimg();
        
    }



    public void displaysecondimg()
    {   

        if (rawImagecurrent != null)

        {

        // Display the second image only when current image has moved and rawimage has spwned
        if (rawImagecurrent.transform.position != transform.position){

        subsequent_img = current_img;

        if (subsequent_img < (n_imgs-1)){
        subsequent_img += 1;}

        else {
            subsequent_img = 0; 
        }
        
        rawImagesubsequent.GetComponent<RawImage>().texture = images[subsequent_img];
        rawImagesubsequent.SetActive(true);

        }
        }

    }


    private void getImageTextures()
    {
        var ext = new List<string> { "jpg", "gif", "png" };
        imagePaths = Directory.EnumerateFiles(Application.dataPath + "/Resources/test_imgs", "*", SearchOption.AllDirectories).ToList();
        imagePaths = imagePaths.Where(path => {string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant(); return ext.Contains(extension);}).ToList();

        Debug.Log(string.Format("Image paths are {0}", imagePaths));
        n_imgs = imagePaths.Count;
        Debug.Log(string.Format("Number of images {0}", n_imgs));

       // float radius = 0.5f; // Adjust radius based on desired size
        foreach  (string imagePath in imagePaths) //Application.dataPath is a built-in Unity variable that provides the path to the main folder of your project on the device where it's running.
        {   
            //if (Path.GetExtension(imagePath).Equals(".png", System.StringComparison.OrdinalIgnoreCase))
            images.Add(Resources.Load<Texture2D>("test_imgs/" + Path.GetFileNameWithoutExtension(imagePath)));

            //This part extracts a substring from the imagePath. It removes the part of the path that matches the project's data path.
            //Resources.Load is a Unity function that allows you to load assets like textures directly from the Resources folder at runtime.
        }

    }


    private void setCanvasPosition()
    {
        float Width = GetComponent<RectTransform>().rect.width;
        float Height = GetComponent<RectTransform>().rect.height;
        float viewingDistance = CalculateViewingDistance(Mathf.Sqrt(Mathf.Pow(Width, 2) + Mathf.Pow(Height, 2)), 25);
        canvas_position = Camera.main.transform.TransformPoint(Vector3.forward * viewingDistance);
        transform.position = canvas_position;
        Debug.Log(string.Format("Z distance: {0}", viewingDistance));

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
