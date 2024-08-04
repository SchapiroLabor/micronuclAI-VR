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
    }

    // Update is called once per frame
    void Update()
    {
        //confirm_if_within_bounds();
    }

    // Start is called before the first frame update

    // Update is called once per frame


 private Vector3 CheckRaycastHit(XRRayInteractor rayInteractor)
    {
        if (rayInteractor && rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // Log the name of the hit GameObject
            Debug.Log("Hit " + hit.collider.gameObject.name);
            Debug.Log("World coords: " + hit.point.x.ToString() + ", " + hit.point.y.ToString());

            return hit.point;
            
        }

        else

        {
            return new Vector3(0,0,0);
        }
        
    }

/*
    public void confirm_if_within_bounds()
{
    Vector3 current_position = Image.transform.position;
    Collider renderer = GetComponent<MeshCollider>();

    var bounds = renderer.bounds;
    var minX = bounds.min.x;
    var maxX = bounds.max.x;
    var minY = bounds.min.y;
    var maxY = bounds.max.y;

    if (current_position.x >= minX && current_position.x <= maxX && current_position.y >= minY && current_position.y <= maxY)
    {
        Debug.Log("X and Y are within bounds");
        change2brightgreen();
    }
    else
    {
        Debug.Log("X and/or Y are not within bounds");
        RevertToOriginalColor();
    }

}
*/

/*
public void confirm_if_within_bounds()
{   

    GameObject Image = transform.parent.parent.GetComponent<InteractableImageStack>().rawImagecurrent;
    if (Image != null)
    {
    Vector3 current_position = Image.transform.position;
    Collider renderer = GetComponent<MeshCollider>();

    var bounds = renderer.bounds;

    if (bounds.Contains(current_position))
    {
        Debug.Log("X and Y are within bounds");
        change2brightgreen();
    }
    else
    {
        Debug.Log("X and/or Y are not within bounds");
        RevertToOriginalColor();
    }
    }


}*/
/*
void OnCollisionEnter(Collision collision)


{

    // Check if the collision involves the other GameObject you are interested in
    if (collision.gameObject.name == "Image")
    {
        transform.parent.GetComponent<Trash>().dispose();
    }

    else 
    {
            Debug.Log(string.Format("No collision as if statement is not satisfied {0}", collision.gameObject.name));
    }

   

} */

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
