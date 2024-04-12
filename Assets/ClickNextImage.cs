using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

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
       Debug.Log(string.Format("Function was executed {0}", "Yes"));
        
        Canvas_script.displaysecondimg(Canvas_script.rawImagesubsequentprefab);
        
    }


}
















