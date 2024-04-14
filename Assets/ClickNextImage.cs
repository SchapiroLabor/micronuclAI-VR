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
    
   void Start()

   {
    
    Canvas_script = transform.parent.GetComponent<InteractableImageStack>();

   }



    void Update()


    {




    }



    public void onSelectEnter()


    {  
    
        
        Canvas_script.displaysecondimg(Canvas_script.rawImagesubsequentprefab);
        
    }


    public void blebcounting()

    {   
        Canvas_script.blebs[Canvas_script.current_img] = Canvas_script.blebs[Canvas_script.current_img] + 1;
        Debug.Log(string.Format("Bleb counted {0}", Canvas_script.blebs[Canvas_script.current_img]));
        Canvas_script.assign_bleb_id(gameObject, Canvas_script.current_img, Canvas_script.blebs[Canvas_script.current_img]);
        Debug.Log(string.Format("Object has assgined bleb count: {0}", gameObject.transform.Find("CIN_counter").gameObject.GetComponent<TextMeshProUGUI>().text));
        
 


    }


    


}
















