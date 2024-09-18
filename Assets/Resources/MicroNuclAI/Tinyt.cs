using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Logger;
using System.IO;

public class Tinyt : MonoBehaviour
{

    private Color originalColor;
    private Color originalEmissionColor;
    private Material material;
    private GameObject Image;
    public List<int> cell_ids = new List<int>();
    public List<int> patches = new List<int>();
    public List<string> patches_names = new List<string>();
    public List<int> keys = new List<int>();
    private float img_height;
    private float img_width;
    private float intersecting_diameter;
    private Bounds bounds;  


    
    // Start is called before the first frame update
     public void Initialize(Transform Image_t)
    {   

        Image = Image_t.gameObject;

        // Cache the Renderer's material and original color at start
        material = GetComponent<Renderer>().material;
        originalColor = material.color;
        
        // Ensure the material supports emission color by enabling emission
        material.EnableKeyword("_EMISSION");

                // Assuming the original emission is set and needs to be stored
        originalEmissionColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);

        GetComponent<Renderer>().material.SetColor("_EmissionColor",originalEmissionColor );

        // Log if emission is on
        Logger.Log("Emission enabled: " + material.IsKeywordEnabled("_EMISSION"));


        Image.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>().selectExited.AddListener((args) => Trashifwithinbounds());

        // Get the image height and width
        img_height = Image.GetComponent<RectTransform>().rect.height;
        img_width = Image.GetComponent<RectTransform>().rect.width;

    }



    // Update is called once per frame
   void Update()
    {
        confirm_if_within_bounds();
    }
    


private void confirm_if_within_bounds()
{   

    if (Image != null&& this != null)
    {   
        Bounds temp = Image.GetComponent<BoxCollider>().bounds;

        Bounds img_bounds = new Bounds(new Vector3(temp.center.x, temp.center.y, bounds.center.z), new Vector3(0.2f, 0.2f, 1));

        // Confrim if bounding box intersects with renderer bounds

        Collider renderer = GetComponent<MeshCollider>();

        bounds = renderer.bounds;

        if (bounds.Intersects(img_bounds))
        {
            change2brightgreen();
        }
        else
        {
            RevertToOriginalColor();
        }
    }
}

public void SavePatch(int img_indx, List<string> img_names)
{
    // Add index to list
    patches.Add(img_indx);

    // Add image name and the trash count to a list
    patches_names.Add(img_names[img_indx]);

    // Get first character of the gameobject name
    keys.Add(Int32.Parse(transform.gameObject.name.Substring(0, 1)));

}

public void RemovePatch()
{   
    if (patches.Count > 0)
    {
        // Remove index from list
        patches.RemoveAt(-1);

        // Remove image name and the trash count from a list
        patches_names.RemoveAt(-1);

        keys.RemoveAt(-1);
    }

}

private void Trashifwithinbounds()
{
    GameObject Image = transform.parent.parent.Find("Image").gameObject;

    if (Image != null)
    {
        Collider renderer = GetComponent<MeshCollider>();

        //  Confirm if image area is intersecting with the trash area
        

        var bounds = renderer.bounds;

        Bounds temp = Image.GetComponent<BoxCollider>().bounds;

        Bounds img_bounds = new Bounds(new Vector3(temp.center.x, temp.center.y, bounds.center.z), new Vector3(0.2f, 0.2f, 1));

        if (bounds.Intersects(img_bounds))
        {
            SavePatch(Image.GetComponent<ClickNextImage>().current_img_indx, Image.GetComponent<ClickNextImage>().img_names);

            transform.parent.GetComponent<Trash>().dispose(transform.gameObject.name);
            
        }

    }
}

    private void change2brightgreen()
    {         // Ensure the material supports emission color by enabling emission
            GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
Color color = new Color(0f, 1f, 0f, 1f);
            // Set the emission color to a bright green
            GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
            
            Logger.Log($"Emission color on : {color}");
            //transform.parent.GetComponent<Trash>().dispose();}
    }


        // Call this method to revert to the original color and emission
    private void RevertToOriginalColor()
    {
        
        material.SetColor("_EmissionColor", originalEmissionColor);

            Logger.Log($"Emission color off : {originalEmissionColor}");
    }


    

}
