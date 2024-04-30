using System.Collections;
using System.Collections.Generic;
using System.IO; // Add this line to include the System.IO namespace
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System;
using System.Drawing;
using UnityEditor.Scripting.Python;

public class MultiplexedImage : MonoBehaviour
{   public List<Texture2D> images = new List<Texture2D>();
    private List<string> imagePaths;
    public int n_imgs;
    public GameObject Channelprefab;

    // Start is called before the first frame update
    void Start()


    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   private Texture2D Init_tif_texture()
   {

    tex = UE.Texture2D(*shape[:2], UE.TextureFormat.RGB48)
    tex.LoadRawTextureData(array)
    tex.Apply()
    class_ = UE.Object.FindObjectsOfType(UE.RawImage)
    class_.RawImage.tex = tex

    }


    private void getImageTextures()
    {
        var ext = new List<string> { "tif"};
        imagePaths = Directory.EnumerateFiles(Application.dataPath + "/Resources/tiff_imgs", "*", SearchOption.AllDirectories).ToList();
        imagePaths = imagePaths.Where(path => {string extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant(); return ext.Contains(extension);}).ToList();

        Debug.Log(string.Format("Image paths are {0}", imagePaths));
        n_imgs = imagePaths.Count;
        Debug.Log(string.Format("Number of images {0}", n_imgs));

       // float radius = 0.5f; // Adjust radius based on desired size
        foreach  (string imagePath in imagePaths) //Application.dataPath is a built-in Unity variable that provides the path to the main folder of your project on the device where it's running.
        {   
            //if (Path.GetExtension(imagePath).Equals(".png", System.StringComparison.OrdinalIgnoreCase))
            images.Add(Resources.Load<Texture2D>("tiff_imgs/" + Path.GetFileNameWithoutExtension(imagePath)));
            tiffimg(imagePath);
            //This part extracts a substring from the imagePath. It removes the part of the path that matches the project's data path.
            //Resources.Load is a Unity function that allows you to load assets like textures directly from the Resources folder at runtime.
        }

    }


    public void init_current_img(GameObject Channelprefab)
    {
        GameObject rawImagecurrent = Instantiate(Channelprefab, transform);
        float width = rawImagecurrent.GetComponent<RectTransform>().rect.width;
        float height = rawImagecurrent.GetComponent<RectTransform>().rect.height;
        rawImagecurrent.GetComponent<BoxCollider>().size = new Vector3(width, height, 0);

        rawImagecurrent.GetComponent<RawImage>().texture = images[0];

        //assign_bleb_id(rawImagecurrent, current_img, blebs);
        
    }


    private void tiffimg(string filePath)

    { 

       // Size imageSize = Image.FromFile(filePath).Size;
        //TextureFormat.RGB565
       // Debug.Log(string.Format("What size is this {}", imageSize));
        //Texture2D tex = new Texture2D(16, 16, TextureFormat.PVRTC_RGBA4, false);
        






    }




}
