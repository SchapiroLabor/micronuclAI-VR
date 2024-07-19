using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using Unity.VisualScripting;

public class ClickNextImage : MonoBehaviour
{
    //public XRGrabInteractable interactable;
    // Start is called before the first frame update
    
    public InteractableImageStack Canvas_script;
    public GameObject rawImagesubsequent;
    
   void Start()

   {



   }








    private  void create_GO4subsequentimage(GameObject rawImagesubsequent)
    {   
        // Create subsequent image only when there are more than one images
            if (Canvas_script.images.Count > 1){
            // Create a new RawImage GameObject from the prefab
            rawImagesubsequent = Instantiate(rawImagesubsequent, transform);
            rawImagesubsequent.SetActive(true);
            
            }
    }




    public void displaysecondimg()
    {   
        
        if (this.gameObject != null)

        {
        Debug.Log("Displaying second image as current image is not null");
        //Create second image
        create_GO4subsequentimage(rawImagesubsequent);
        // Display the second image only when current image has moved and rawimage has spwned
        //if (rawImagecurrent.transform.position != transform.position){
        int subsequent_img = Canvas_script.current_img_indx;

        if (subsequent_img < (Canvas_script.images.Count-1)){
        subsequent_img += 1;
         Debug.Log($"Subsequent image index changed from {Canvas_script.current_img_indx} to {subsequent_img}");
        }
        

        else {
            Debug.Log($"Subsequent image index goes back to  {Canvas_script.current_img_indx} to {subsequent_img}");
            subsequent_img = 0; 
        }
        
        rawImagesubsequent.GetComponent<RawImage>().texture = Canvas_script.images[subsequent_img];
        
        }

        

    }
    


}
















