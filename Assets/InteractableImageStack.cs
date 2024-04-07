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
    private int current_img = 0;
    private int n_imgs;
    public GameObject rawImagecurrent;
    private GameObject  rawImagesubsequent;
    public GameObject rawImageprefab;
    public GameObject trash;
    public List<Texture2D> images = new List<Texture2D>();
    private List<string> imagePaths;

    private Vector3 canvas_position;

    public XRInteractionManager interactionManager;
    
    private void Start()
    {   

        rawImagecurrent = transform.Find("RawImage").gameObject; // Prefab for the RawImage UI element

        // Set Canvas at acceptable viewing position
        setCanvasPosition();

        // Populate the list dedicated to image textures
        getImageTextures();
        

        initialize_images(current_img);

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


    private void initialize_images(int indx)

    {   
        rawImagecurrent.GetComponent<RawImage>().texture = images[indx];

        if (n_imgs > 1){
            create_subsequent_img();
        }


    }


    private  GameObject create_subsequent_img()
    {   
        // Create a new RawImage GameObject from the prefab
        rawImagesubsequent = Instantiate(rawImageprefab,  transform, true);

        rawImagesubsequent.transform.position = rawImagecurrent.transform.position;

        rawImagesubsequent.SetActive(false);

        return rawImagesubsequent;
    }

// This is only executed whilst the object is selected
    public void displaysecondimg()
    {   

        Debug.Log("Next image displayed");
        if (rawImagecurrent.transform.position != canvas_position && rawImagesubsequent != null){

        int indx = current_img;

        if (indx < (n_imgs-1)){
        indx += 1;}

        else {
            indx = 0; 
        }
        
        rawImagesubsequent.GetComponent<RawImage>().texture = images[indx];
        rawImagesubsequent.SetActive(true);


        }
    }


// This is executed once the trash object collider is triggered
    public void dispose()

    { 

        
         if (current_img < (n_imgs-1)){
        current_img += 1;}

        else {
            current_img = 0; 
        }
        
        interactionManager.CancelInteractableSelection(rawImagecurrent.GetComponent<IXRSelectInteractable>());
        rawImagecurrent.GetComponent<RawImage>().texture = images[current_img];
        rawImagecurrent.transform.position = canvas_position;
        



        if (rawImagesubsequent != null){
        
        rawImagesubsequent.GetComponent<RawImage>().texture = null;
        rawImagesubsequent.SetActive(false);


        }


    }

    


}
