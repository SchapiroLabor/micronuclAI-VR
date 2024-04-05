using System.Collections;
using System.Collections.Generic;
using System.IO; // Add this line to include the System.IO namespace
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;


public class InteractableImageStack : MonoBehaviour
{
    //public RawImage[] imageDisplays; // Array to store image displays
    public int current_img = 0;
    public int n_imgs;
    public GameObject rawImagePrefab; // Prefab for the RawImage UI element
    public GameObject trash;
    private Vector3 initialPosition;
    public List<Texture2D> images = new List<Texture2D>();
    private List<string> imagePaths;
    
    private void Start()
    {   

        float scaledWidth = GetComponent<RectTransform>().rect.width;
        float scaledHeight = GetComponent<RectTransform>().rect.height;
        float viewingDistance = CalculateViewingDistance(Mathf.Sqrt(Mathf.Pow(scaledWidth, 2) + Mathf.Pow(scaledHeight, 2)), 25);
        transform.position = Camera.main.transform.TransformPoint(Vector3.forward * viewingDistance);
        
        Debug.Log(string.Format("Z distance: {0}", viewingDistance));



        var ext = new List<string> { "jpg", "gif", "png" };
        imagePaths = Directory.EnumerateFiles(Application.dataPath + "/Resources/test_imgs", "*", SearchOption.AllDirectories).ToList();
        imagePaths = imagePaths.Where(path => {string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant(); return ext.Contains(extension);}).ToList();


        //imagePaths = System.IO.Directory.GetFiles(Application.dataPath + "/Resources/test_imgs");
        
        Debug.Log(string.Format("Images are {0}", imagePaths));
        Debug.Log(string.Format("Current position: {0}", initialPosition));
       // float radius = 0.5f; // Adjust radius based on desired size
        foreach  (string imagePath in imagePaths) //Application.dataPath is a built-in Unity variable that provides the path to the main folder of your project on the device where it's running.
        {   
            //if (Path.GetExtension(imagePath).Equals(".png", System.StringComparison.OrdinalIgnoreCase))
            images.Add(Resources.Load<Texture2D>("test_imgs/" + Path.GetFileNameWithoutExtension(imagePath)));

            //This part extracts a substring from the imagePath. It removes the part of the path that matches the project's data path.
            //Resources.Load is a Unity function that allows you to load assets like textures directly from the Resources folder at runtime.
        }

        n_imgs = imagePaths.Count;
        Debug.Log(string.Format("Number of images {0}", n_imgs));

                // Loop through each interactable

        display_img(current_img);

        create_trash();

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
    public void create_img(int indx)
    {   
        // Create a new RawImage GameObject from the prefab
        GameObject main_object = Instantiate(rawImagePrefab,  transform, true);

        main_object.GetComponent<RawImage>().texture = images[indx];

        main_object.transform.position = transform.position;

    }

    public void create_trash()

    { 
        // Create a new RawImage GameObject from the prefab
        //Transform childgameobject = transform.GetChild(0);
        //float imageheight = childgameobject.GetComponent<RectTransform>().rect.height;
        //Vector3 trashposition = childgameobject.position - new Vector3(0 , imageheight , 0) ;
        
        //GameObject main_object = Instantiate(trash, transform, true);

        //main_object.transform.SetParent(transform);
    }


    public void display_img(int indx)

    {   
        Transform childTransform = transform.Find("RawImage");
        childTransform.GetComponent<RawImage>().texture = images[indx];

    }




}
