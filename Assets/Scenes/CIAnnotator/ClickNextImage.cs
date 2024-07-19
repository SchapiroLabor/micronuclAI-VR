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
    
    private InteractableImageStack Canvas_script;
    private GameObject rawImagesubsequent;
    
   void Start()

   {



   }








    private  void create_GO4subsequentimage(Transform empty_subs_img)
    {   
        // Create subsequent image only when there are more than one images

        if (empty_subs_img == null){
            if (Canvas_script.images.Count > 1){
            // Create a new RawImage GameObject from the prefab
            rawImagesubsequent = Instantiate(empty_subs_img.gameObject,  transform);
            rawImagesubsequent.SetActive(false);

            }}
    }




    public void displaysecondimg()
    {   

        // Get preloaded image 
        Transform rawImagesubsequent = transform.Find("SubsequentImage");
        //Create second image
        create_GO4subsequentimage(rawImagesubsequent);

        Debug.Log("Displaying second image");

        if (this.gameObject != null && rawImagesubsequent == null)

        {

        // Display the second image only when current image has moved and rawimage has spwned
        //if (rawImagecurrent.transform.position != transform.position){

        int subsequent_img = Canvas_script.current_img_indx;

        if (subsequent_img < (Canvas_script.images.Count-1)){
        subsequent_img += 1;}

        else {
            subsequent_img = 0; 
        }
        
        rawImagesubsequent.GetComponent<RawImage>().texture = Canvas_script.images[subsequent_img];
        
        }

        

    }
    


}
















