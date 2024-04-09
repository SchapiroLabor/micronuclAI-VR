using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;


public class Counter : MonoBehaviour
{

    int blebls = 0;
    // Start is called before the first frame update
    private void Display_CIN_count()
    {


        GetComponent<TextMeshProUGUI>().text = "Number of blebs:"+ string.Format(" {0}", blebls);


    }


    void Update()

    {
        Display_CIN_count();



    }


    void onActivated(ActivateEventArgs args)
    {
        blebls += 1;

    }


}
