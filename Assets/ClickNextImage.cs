using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;


public class ClickNextImage : MonoBehaviour
{
    //public XRGrabInteractable interactable;
    // Start is called before the first frame update
    
    private RectTransform myRectTransform;
    private BoxCollider myBoxCollider;

    public GameObject rawImagecurrentprefab;

    public GameObject rawImagesubsequent;

    public InteractableImageStack Canvas_script;

    public XRInteractionManager interactionManager;

    
    void Start()

    {          // Find and reference the ScriptA component attached to a GameObject
     //   interactable = GetComponent<XRSimpleInteractable>();
       // interactable.selectEntered.AddListener(check);
        myRectTransform = GetComponent<RectTransform>();
        myBoxCollider = GetComponent<BoxCollider>();
        
        float width = myRectTransform.rect.width;
        float height = myRectTransform.rect.height;
        myBoxCollider.size = new Vector3(width, height, 0);
        Debug.Log(string.Format("Size of 3D Collider {0}", myBoxCollider.size));


        // Initiliaze images 
        GetComponent<RawImage>().texture = Canvas_script.images[Canvas_script.current_img];

   
    }


// This is executed once the trash object collider is triggered
    public void dispose()

    { 

        
         if (Canvas_script.current_img < (Canvas_script.n_imgs-1)){
        Canvas_script.current_img += 1;}

        else {
            Canvas_script.current_img = 0; 
        }
        

        interactionManager.CancelInteractableSelection(GetComponent<IXRSelectInteractable>());
        GetComponent<RawImage>().texture = Canvas_script.images[Canvas_script.current_img];
        transform.position = Canvas_script.canvas_position;
        



        if (rawImagesubsequent != null){
        
        rawImagesubsequent.GetComponent<RawImage>().texture = null;
        rawImagesubsequent.SetActive(false);


        }


    }

}
