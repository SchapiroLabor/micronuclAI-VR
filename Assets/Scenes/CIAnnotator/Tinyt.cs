using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Tinyt : MonoBehaviour
{

    private Color originalColor;
    private Color originalEmissionColor;
    private Material material;
    private GameObject Image;


    
    // Start is called before the first frame update
     void Start()
    {
        // Cache the Renderer's material and original color at start
        material = GetComponent<Renderer>().material;
        originalColor = material.color;
        
        // Assuming the original emission is set and needs to be stored
        originalEmissionColor = material.GetColor("_EmissionColor");
        
        // Ensure the material supports emission color by enabling emission
        material.EnableKeyword("_EMISSION");

        Image = transform.parent.parent.Find("Image").gameObject;

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

private void Trashifwithinbounds()
{
    GameObject Image = transform.parent.parent.Find("Image").gameObject;

    if (Image != null)
    {
        Vector3 current_position = Image.transform.position;
        Collider renderer = GetComponent<MeshCollider>();

        var bounds = renderer.bounds;

        if (bounds.Contains(current_position))
        {
            transform.parent.GetComponent<Trash>().dispose();
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
