using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;


public class Trash : MonoBehaviour
{
    public ClickNextImage rawImagecurrent_script;
    public InteractableImageStack Canvas_script;
    public XRInteractionManager interactionManager;
     public GameObject rawImagesubsequent;
    
    
        // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnCollisionEnter(Collision collision)
    {   
        if (rawImagecurrent_script != null && Canvas_script != null)

        {

        // Check if the collision involves the other GameObject you are interested in
        if (collision.gameObject.name == rawImagecurrent_script.gameObject.name)
        {Debug.Log(string.Format("Objected collided {0}", "YES !"));
            dispose();
        }



        }

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
        

        closedisplaysecondimg();



    }


    public void closedisplaysecondimg()

{

        if (rawImagesubsequent != null){
        
        rawImagesubsequent.GetComponent<RawImage>().texture = null;
        rawImagesubsequent.SetActive(false);


        }

   
    }


}
