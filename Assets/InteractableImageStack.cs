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
    public List<Texture2D> images = new List<Texture2D>();
    private Vector3 initialPosition;
    private List<string> imagePaths;
    
    private void Start()
    {   
        initialPosition = Camera.main.transform.position;
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
        for (int i = 0; i < n_imgs; i++)
        {
            create_imgs(i);
        }

        display_img(current_img);

    }

        //private int currentImageIndex; // Index of the currently displayed image
    public void create_imgs(int indx)
    {   
        Debug.Log(string.Format("Image count: {0}", current_img));
        Vector3 main_position = initialPosition + new Vector3(0 , 0, 0.5f);

        // Create a new RawImage GameObject from the prefab
        GameObject main_object = Instantiate(rawImagePrefab, main_position, Quaternion.identity);

        // Set parent
        main_object.transform.SetParent(transform);
        main_object.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

        Debug.Log(string.Format("Position of image is: {0}", main_position));
        main_object.GetComponent<RawImage>().texture = images[indx];
        main_object.GetComponent<RawImage>().enabled = false;
        main_object.GetComponent<BoxCollider>().enabled = false;
    }

    public void display_img(int indx)

    {   int previous_indx;
        //Set previous off regardless of state
        if (indx == 0){
        previous_indx = n_imgs-1;}

        else {
            previous_indx = indx - 1; 
        }
        Transform previous_object = transform.GetChild(previous_indx);
        previous_object.GetComponent<RawImage>().enabled = false;
        previous_object.GetComponent<BoxCollider>().enabled = false;


        // Get the child GameObject at the specified index
        Transform main_object = transform.GetChild(indx);
        main_object.GetComponent<RawImage>().enabled = true;
        main_object.GetComponent<BoxCollider>().enabled = true;

    }




}
