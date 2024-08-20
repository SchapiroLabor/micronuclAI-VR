using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;

public class Tinyt : MonoBehaviour
{

    private Color originalColor;
    private Color originalEmissionColor;
    private Material material;
    private GameObject Image;
    public List<int> patches = new List<int>();
    public List<string> patches_names = new List<string>();
    public List<int> keys = new List<int>();


    
    // Start is called before the first frame update
     public void Initialize(Transform Image_t)
    {   

        Image = Image_t.gameObject;

        // Cache the Renderer's material and original color at start
        material = GetComponent<Renderer>().material;
        originalColor = material.color;
        
        // Assuming the original emission is set and needs to be stored
        originalEmissionColor = material.GetColor("_EmissionColor");
        
        // Ensure the material supports emission color by enabling emission
        material.EnableKeyword("_EMISSION");

        Image.GetComponent<XRGrabInteractable>().selectExited.AddListener((args) => Trashifwithinbounds());

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
        Vector3 current_position = Image.transform.position;
        Collider renderer = GetComponent<MeshCollider>();

        var bounds = renderer.bounds;

        if (bounds.Contains(current_position))
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

    InteractableImageStack Canvas_script = Image.transform.parent.parent.GetComponent<InteractableImageStack>();

    if (Image != null)
    {
        Vector3 current_position = Image.transform.position;
        Collider renderer = GetComponent<MeshCollider>();

        var bounds = renderer.bounds;

        if (bounds.Contains(current_position))
        {
            SavePatch(Image.GetComponent<ClickNextImage>().current_img_indx, Image.GetComponent<ClickNextImage>().img_names);

            transform.parent.GetComponent<Trash>().dispose(transform.gameObject.name);
            
        }

    }
}

    private void change2brightgreen()
    {         // Ensure the material supports emission color by enabling emission
            GetComponent<Renderer>().material.EnableKeyword("_EMISSION");

            // Set the emission color to a bright green
            GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0f, 1f, 0f, 1f));
            //transform.parent.GetComponent<Trash>().dispose();}
    }


        // Call this method to revert to the original color and emission
    private void RevertToOriginalColor()
    {
        material.color = originalColor;
        material.SetColor("_EmissionColor", originalEmissionColor);
    }


    

}
