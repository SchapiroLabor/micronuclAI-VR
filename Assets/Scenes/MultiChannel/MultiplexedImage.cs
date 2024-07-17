using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using System;

public class MultiplexedImage : MonoBehaviour

{

    public GameObject leftRayInteractor;
    public GameObject rightRayInteractor;



    public void CheckRaycastHitBoth()
    {
        CheckRaycastHit(leftRayInteractor.GetComponent<XRRayInteractor>());
        CheckRaycastHit(rightRayInteractor.GetComponent<XRRayInteractor>());
    }


    private void CheckRaycastHit(XRRayInteractor rayInteractor)
    {
        if (rayInteractor && rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // Log the name of the hit GameObject
            Debug.Log("Hit " + hit.collider.gameObject.name);
            Debug.Log("World coords: " + hit.point.x.ToString() + ", " + hit.point.y.ToString());

            // Convert the world position of the hit to screen point
                    Vector3 screenPoint = Camera.main.WorldToScreenPoint(hit.point);

                        // Convert world position to local position
            Vector2 localHitPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GetComponent<RawImage>().rectTransform,
                screenPoint,
                Camera.main,
                out localHitPoint
            );

            Debug.Log("localHitPoint coords: " + localHitPoint.x.ToString() + ", " + localHitPoint.y.ToString());

            // Convert local position to pixel coordinates
            Rect rect = GetComponent<RawImage>().rectTransform.rect;
   
            //float u = Mathf.InverseLerp(rect.xMin, rect.xMax, localHitPoint.x);
            //float v = Mathf.InverseLerp(rect.yMin, rect.yMax, localHitPoint.y);


            double x = (localHitPoint.x - rect.x) * GetComponent<RawImage>().texture.width / rect.width;
            double y = (localHitPoint.y - rect.y) * GetComponent<RawImage>().texture.height / rect.height;


            //Vector2 pixelCoordinate = new Vector2(, Math.Round(y));

            Debug.Log("Pixel Coordinates: " + Math.Round(x).ToString() + " " + Math.Round(y).ToString());

            getpixelIntensity( GetComponent<RawImage>().texture,  (int)Math.Floor(x), (int)Math.Floor(y));
        }


        
    }



    
    private  Vector3 c_p;


    public Camera cam;






    
    public void init_current_img()
    {
        // Cannot change child object size with this. Maybe sparingly

         GetComponent<RawImage>().SetNativeSize();
                 float width = GetComponent<RectTransform>().rect.width; 
        float height = GetComponent<RectTransform>().rect.height;
        GetComponent<BoxCollider>().size = new Vector3(width, height, 0);
        
    }


    // Start is called before the first frame update
    void Start()


    {     
         init_current_img();


        
    }

    // Update is called once per frame
    void Update()
    {
        
    
    }




    void getpixelIntensity(Texture texture, int x, int y)
    {
        // Get the color of a specific pixel
        UnityEngine.Color pixelColor = ((Texture2D)texture).GetPixel(x, y);

        //Debug.Log("Pixel coords: " + x.ToString() + ", " + y.ToString());

        // Perform pixel tracking logic here

        // Example: Print the RGB values of the pixel
        Debug.Log("Pixel Color: R=" + pixelColor.r + ", G=" + pixelColor.g + ", B=" + pixelColor.b);



    }


    private void GetPixelCoordinate()
    {
        RawImage rawImage = GetComponent<RawImage>();

        GameObject CANVAS = transform.parent.gameObject;

        XRRayInteractor ray = CANVAS.GetComponent<XRRayInteractor>();
        
        RaycastHit hit;
        if (ray.TryGetCurrent3DRaycastHit(out hit))
        {
            Debug.Log("Local coords: " + hit.textureCoord.x.ToString() + ", " + hit.textureCoord.y.ToString());
        }
    }
 





}
