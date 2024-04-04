using System.Collections;
using System.Collections.Generic;
using System.IO; // Add this line to include the System.IO namespace
using UnityEngine;
using UnityEngine.UI;



public class InteractableImageStack : MonoBehaviour
{
    //public RawImage[] imageDisplays; // Array to store image displays
    public GameObject rawImagePrefab; // Prefab for the RawImage UI element
    public List<Texture2D> images = new List<Texture2D>();
    private Vector3 initialPosition;
    
    private void Start()
    {   

        initialPosition = Camera.main.transform.position;
        string[] imagePaths = System.IO.Directory.GetFiles(Application.dataPath + "/Resources/test_imgs");
        Debug.Log(string.Format("Images are {0}", imagePaths));
        Debug.Log(string.Format("Current position: {0}", initialPosition));
       // float radius = 0.5f; // Adjust radius based on desired size
        foreach  (string imagePath in imagePaths) //Application.dataPath is a built-in Unity variable that provides the path to the main folder of your project on the device where it's running.
        {   
            if (Path.GetExtension(imagePath).Equals(".png", System.StringComparison.OrdinalIgnoreCase))
            { images.Add(Resources.Load<Texture2D>("test_imgs/" + Path.GetFileNameWithoutExtension(imagePath)));}

            //This part extracts a substring from the imagePath. It removes the part of the path that matches the project's data path.
            //Resources.Load is a Unity function that allows you to load assets like textures directly from the Resources folder at runtime.
        }

        
        Vector3 main_position = makemain(0);
       // Instantiate(rawImagePrefab, main_position + new Vector3(1, 0, 0), Quaternion.identity);

        //  float angleStep = Mathf.PI * 2f/images.Count; // Angle between image

    }

        //private int currentImageIndex; // Index of the currently displayed image
    public Vector3 makemain(int indx)
    {

        Vector3 main_position = initialPosition + new Vector3(0 , 0, 1);
        // Create a new RawImage GameObject from the prefab
        GameObject main_object = Instantiate(rawImagePrefab, main_position, Quaternion.identity);
        // Set parent
        main_object.transform.SetParent(transform);
        main_object.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

        Debug.Log(string.Format("Position of image is: {0}", main_position));

        main_object.GetComponent<RawImage>().texture = images[indx];

        return main_position;
    }




}
