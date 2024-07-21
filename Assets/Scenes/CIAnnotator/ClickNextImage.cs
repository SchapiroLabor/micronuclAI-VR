using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;

public class ClickNextImage : MonoBehaviour
{
    //public XRGrabInteractable interactable;
    // Start is called before the first frame update
    
    public InteractableImageStack Canvas_script;
    public GameObject rawImagesubsequentGO;
    
   void Start()

   {

    create_GO4subsequentimage();

   }


    private  void create_GO4subsequentimage()
    {   
        // Create subsequent image only when there are more than one images
            if (Canvas_script.images.Count > 1){
                        // Step 1: Define the path to the prefab within the Resources folder
            string prefabPath = "Assets/Scenes/CIAnnotator/SubsequentImage.prefab";
            // Create a new RawImage GameObject from the prefab
            rawImagesubsequentGO = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), transform.position, transform.rotation);
            rawImagesubsequentGO.transform.SetParent(transform.parent);
    }
    }



    public void displaysecondimg()
    {   
        
        if (this.gameObject != null && rawImagesubsequentGO != null)

        {
        Debug.Log("Displaying second image as current image is not null");
        //Create second image
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
        
        rawImagesubsequentGO.GetComponent<RawImage>().texture = Canvas_script.images[subsequent_img];
        rawImagesubsequentGO.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Format("Patch {0}/{1}", subsequent_img + 1, Canvas_script.N_image);
        rawImagesubsequentGO.SetActive(true);
        
    }}
    


}
















