using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trash : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnCollisionEnter(Collision collision)
    {   
        Debug.Log(string.Format("Collison noted by trash {0}", "YES !"));
        // Check if the collision involves the other GameObject you are interested in
        if (collision.gameObject == transform.parent.GetComponent<InteractableImageStack>().rawImagecurrent)
        {Debug.Log(string.Format("Collison noted by GO {0}", "YES !"));
            transform.parent.GetComponent<InteractableImageStack>().dispose();
        }
    }
}
