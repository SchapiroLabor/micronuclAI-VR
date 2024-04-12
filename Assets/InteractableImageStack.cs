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
    public int current_img = 0;
    public int blebs = 0;
    public int subsequent_img;
    public int n_imgs;
    public List<Texture2D> images = new List<Texture2D>();
    private List<string> imagePaths;
    public Vector3 canvas_position;
    public Quaternion canvas_rotation;
    public GameObject rawImagecurrentprefab;
    public GameObject rawImagecurrent;
    public GameObject rawImagesubsequentprefab;
    public GameObject rawImagesubsequent;

    private void Start()
    {   


    // Set Canvas at acceptable viewing position
    setCanvasPosition();

    // Populate the list dedicated to image textures
    getImageTextures();

    init_current_img(rawImagecurrentprefab);

    

    
    }

    //Blebs should be counted using activation and be cuncurrent with current_img

    public void init_current_img(GameObject rawImagecurrentprefab)
    {
        rawImagecurrent = Instantiate(rawImagecurrentprefab, transform);
        float width = rawImagecurrent.GetComponent<RectTransform>().rect.width;
        float height = rawImagecurrent.GetComponent<RectTransform>().rect.height;
        rawImagecurrent.GetComponent<BoxCollider>().size = new Vector3(width, height, 0);

        rawImagecurrent.GetComponent<RawImage>().texture = images[current_img];

        assign_bleb_id(rawImagecurrent, current_img, blebs);
        


    }

    private void assign_bleb_id(GameObject image, int imgid, int blebid)

    {
        TextMeshProUGUI imageid = image.transform.Find("Image_ID").gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI bleb = image.transform.Find("CIN_counter").gameObject.GetComponent<TextMeshProUGUI>();

        // Get child objects to set bleb count and ID


        imageid.text = string.Format("Image ID: {0}", imgid);
        bleb.text = string.Format("N blebs: {0}", blebid);



    }


    private  void create_subsequent_img(GameObject rawImagesubsequentprefab)
    {   
        if (n_imgs > 1){

        // Create a new RawImage GameObject from the prefab
        rawImagesubsequent = Instantiate(rawImagesubsequentprefab,  transform);
        rawImagesubsequent.SetActive(true);


        }
    }


    void Update()
    {

    
        
    }



    public void displaysecondimg(GameObject rawImagesubsequentprefab)
    {   

        if (rawImagecurrent != null && rawImagesubsequent == null)

        {

        // Display the second image only when current image has moved and rawimage has spwned
        //if (rawImagecurrent.transform.position != transform.position){


        create_subsequent_img(rawImagesubsequentprefab);

        subsequent_img = current_img;

        if (subsequent_img < (n_imgs-1)){
        subsequent_img += 1;}

        else {
            subsequent_img = 0; 
        }
        
        rawImagesubsequent.GetComponent<RawImage>().texture = images[subsequent_img];
        assign_bleb_id(rawImagesubsequent, subsequent_img, blebs);
        
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
        canvas_rotation = transform.rotation;
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
