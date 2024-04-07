using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class ClickNextImage : MonoBehaviour
{
    //public XRGrabInteractable interactable;
    // Start is called before the first frame update
    
    private RectTransform myRectTransform;
    private BoxCollider myBoxCollider;
    private InteractableImageStack stackable;
    void Start()

    {          // Find and reference the ScriptA component attached to a GameObject
     //   interactable = GetComponent<XRSimpleInteractable>();
       // interactable.selectEntered.AddListener(check);
        myRectTransform = GetComponent<RectTransform>();
        myBoxCollider = GetComponent<BoxCollider>();
        
        float width = myRectTransform.rect.width;
        float height = myRectTransform.rect.height;
        myBoxCollider.size = new Vector3(width, height, 0);
        stackable = transform.parent.GetComponent<InteractableImageStack>();
        Debug.Log(string.Format("Size of 3D Collider {0}", myBoxCollider.size));

   
    }

    public void message()
    {   Debug.Log("Worked");
    }

}
