using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
// Import functions from another script
using static InteractableImageStack;
// With a static directive, you can access the members of the class by using the class name itself

public class whole_image : MonoBehaviour
{
   List<element>  data_dict;
   private GameObject GameManager;
   private GameObject Arrow;
   private ClickNextImage CurrentImage_script;
   private float height;
   private float width;
   string data_dir;
   private string python_path;
   string working_dir;
   private float raycast_distance = 10f; // Default distance to raycast from the camera, please do not change this !!!
   private float newWidth;
    private float newHeight;




       // All functions independet of other objects can be placed in even functions Awake, OnEnable, Start

    void Awake()
    {

        gameObject.name = "Image";

                // Load the Game Manager
        if (GameManager == null)
        {
            // Load from path
            GameManager = Resources.Load<GameObject>(Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/SceneManager.prefab")));
        }


        // Get the img and python path
        data_dir = GameManager.GetComponent<GameManaging>().InputFolder;
        python_path = GameManager.GetComponent<GameManaging>().PythonExecutable;
        working_dir = Directory.GetCurrentDirectory();

        // Setup anchors and pivots
        RectTransform rectTransform = GetComponent<RectTransform>();
        SetupAnchorsAndPivots(rectTransform);

        // Set the anchors and pivots of the Canvas as sizeDelta requires absolute difference
        transform.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        transform.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

        // Reduce Local Scale
        rectTransform.localScale = new UnityEngine.Vector3(1f, 1f, 1f);

        // Plays on background thread
        InteractableImageStack.ThreadPooling(new Action<string, string, string> (read_csv_with_python),
        null , working_dir, data_dir, python_path);
        
        // Plays on main thread with pauses
        StartCoroutine(MyCoroutine(Path.Combine(data_dir, "img.png")));

    }

private System.Collections.IEnumerator MyCoroutine(string img_path)
{
    // Remove the call to WaitForWholeImage since it is not being used
    SetTextureOnWholeImage(img_path);
    yield return null; // Wait for the next frame
}




    // Start is called before the first frame update
    public void Initialize(Transform parent, Transform Panel, Camera userCamera, ClickNextImage CurrentImage)

    {


        gameObject.transform.SetParent(parent);
        gameObject.SetActive(true);

        PositionWholeImage(parent, Panel, userCamera);
        // Should occure after the image is positioned as we are using world coordinates
        PositionImagetitle(transform.GetChild(0));

        // Get CurrentImage
        CurrentImage_script = CurrentImage;
        
    }


    public class element
    {   // X, Y = Width, Height
        public int x_min { get; set; } 
        public int x_max { get; set; }
        public int y_min { get; set; }
        public int y_max { get; set; }
    }

    private void read_csv_with_python(string cwd, string data_dir, string python_exe)
    {   

        // Execute using System thread pool

        string pythonScriptPath = Path.Combine(cwd, "python_codes", "read_df.py");
        pythonScriptPath = $"\"{pythonScriptPath}\"";


        string output = ReadfromPython(pythonScriptPath, python_exe, InteractableImageStack.AddQuotesIfRequired(data_dir));


        data_dict = ConvertOutputToDictionary(output);

    }

    public void ConfirmDataDict(List<element> data_dict)
    {
        if (data_dict == null)
        {
            Debug.Log("Data dictionary is null");
        }
        else
        {   
            Debug.Log("Data dictionary is not null");
        }
    }

    private List<element> ConvertOutputToDictionary(string output)
    {
        List<element> result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<element>>(output);

        return result;
    }

    private void ColorPixelCluster(Rect pixelCluster, Color newColor)
    {
        // Clone the original texture
        Texture2D originalTexture = GetComponent<RawImage>().texture as Texture2D;

        // Apply the new color to the specified cluster of pixels
        for (int x = (int)pixelCluster.xMin; x < (int)pixelCluster.xMax; x++)
        {
            for (int y = (int)pixelCluster.yMin; y < (int)pixelCluster.yMax; y++)
            {
                originalTexture.SetPixel(x, y, newColor); // The x and y are swapped because the texture is rotated
            }
        }

        // Apply all changes to the texture
        originalTexture.Apply();
    }

    private Rect get_bbox_from_df(int patch_indx)
    {
        // Get the bounding box of the pixel cluster
        element patchData = data_dict[patch_indx];
        int width = patchData.x_max - patchData.x_min;
        int height = patchData.y_max - patchData.y_min;
        Rect bbox = new Rect(patchData.x_min, patchData.y_min, width, height);
        //Debug.Log($"The axes ranges are FOR X {patchData.x_min}, {patchData.x_max} ADN FOR Y {patchData.y_min}, {patchData.y_max}");

        return bbox;
    }

    public void current_cell_bbox(int patch_indx)
    {   
        if (patch_indx < data_dict.Count)
        {
        // Get the bounding box of the pixel cluster
        Rect bbox = get_bbox_from_df(patch_indx);

        // Color the pixel cluster
        ColorPixelCluster(bbox, Color.red);
        }

    }

    public void DisplayPatch()
    {   
        Debug.Log("Displaying patch: " + CurrentImage_script.current_img_indx.ToString());
        current_cell_bbox(CurrentImage_script.current_img_indx);

        //DisplayArrow(CurrentRawImage.current_img_indx);

    }

private void InitializeIntensityCylinder()
{
string prefabPath = Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/Cylinder.prefab"));
Arrow = CreateGameObject(transform, prefabPath, transform);
Arrow.SetActive(false);

// Square root area of image
float estimate_length = math.sqrt((GetComponent<RectTransform>().rect.width * GetComponent<RectTransform>().rect.height));
Arrow.transform.localScale = new UnityEngine.Vector3(estimate_length*0.1f*0.1f, estimate_length*0.1f , estimate_length*0.1f*0.1f);

// Get the rotation in 150° Angle
Arrow.transform.localRotation = UnityEngine.Quaternion.Euler(90, 0, 0);

// Anchor to the left bottom corner
Arrow.GetComponent<RectTransform>().anchorMin = new UnityEngine.Vector2(0, 0);
Arrow.GetComponent<RectTransform>().anchorMax = new UnityEngine.Vector2(0, 0);

// Pivot should be at centre bottom
Arrow.GetComponent<RectTransform>().pivot = new UnityEngine.Vector2(0.5f, 0);

// Set parent to the whole image
Arrow.transform.SetParent(transform);


}

private void PositionArrow(UnityEngine.Vector3 position)
{

Arrow.transform.position = transform.TransformPoint(new UnityEngine.Vector3(position.x, position.y, position.z));

Debug.Log($"The position of the arrow is {Arrow.transform.localPosition}");

if (!Arrow.activeSelf){
Arrow.SetActive(true);
}

}

private UnityEngine.Vector3 GetArrowPosition(element patch_position)
{
    // Get mid point from the bbox of the patch 
    UnityEngine.Vector2 mid_point_pixel = new UnityEngine.Vector2((patch_position.x_min + patch_position.x_max)/2,
    (patch_position.y_min + patch_position.y_max)/2) / new UnityEngine.Vector2(width, height);


    Debug.Log($"The mid point of the patch is {mid_point_pixel} and the width and height are {width} and {height}");

    float max_x = GetComponent<RectTransform>().rect.width;
    float max_y = GetComponent<RectTransform>().rect.height;

    UnityEngine.Vector3 mid_point_world_with_offset = new UnityEngine.Vector3( mid_point_pixel.x * max_x,
    mid_point_pixel.y * max_y, transform.InverseTransformPoint(transform.position).z);
    
    Debug.Log($"The mid point of the patch is {mid_point_world_with_offset}");

    // Get centre of image in local coords
    UnityEngine.Vector3 mid_point_world = new UnityEngine.Vector3(max_x/2, max_y/2, transform.InverseTransformPoint(transform.position).z);

    // If image is rotated 90°, then anchor point is not applicable anymore
    mid_point_world_with_offset = new UnityEngine.Vector3(mid_point_world_with_offset.x, mid_point_world_with_offset.y, mid_point_world_with_offset.z) - mid_point_world;

    return mid_point_world_with_offset;
}


private void DisplayArrow(int current_img_indx)
{
    // Get the patch position
    element patch_position = data_dict[current_img_indx];

    // Get the arrow position and rotation
    UnityEngine.Vector3 position = GetArrowPosition(patch_position);

    // Position the arrow
    PositionArrow(position);

}

private void SetTextureOnWholeImage(string img_path)
    {
        Texture2D whole_img_texture = LoadTexture(img_path);

        if (whole_img_texture == null)
        {
            Debug.Log("Whole image texture is null");
        }

        // Size delta must be explicitly matched to the size of the image
        GetComponent<RawImage>().texture = whole_img_texture;
        RectTransform rectTransform = GetComponent<RawImage>().GetComponent<RectTransform>();
        rectTransform.sizeDelta = new UnityEngine.Vector2(whole_img_texture.width, whole_img_texture.height);
    }

    private List<float> GetFOVatWD(float WD, Camera userCamera)
    {
        // Pythagoras theorem to calculate the distance
        List<float> holder = new List<float>();
        float vertical_fov = userCamera.fieldOfView;
        float fov_height = (WD * Mathf.Tan(vertical_fov * 0.5f)) * 2;
        float fov_width =  userCamera.aspect * fov_height;     // Aspect ratio of the camera is width/height

        holder.Add(fov_height);
        holder.Add(fov_width);
        holder.Add(WD);

        return holder;
    }

private void ResizeImgtobewithinFOV(float WD, Camera userCamera)
{

    // Get the FOV at the panel height
    List<float> outputs = GetFOVatWD(WD, userCamera);
    newWidth = outputs[0]*2f; // Width
    newHeight = outputs[1]*2f; // Height

    // Reduce image size whilst keeping the image aspect ratio
    float aspect_ratio = width/height;

    // Adjust the dimensions to maintain the aspect ratio
    if (newWidth > newHeight * aspect_ratio)
    {
        newWidth = newHeight * aspect_ratio; // Aspect ratio is 1, so newWidth = newHeight
    }
    else
    {
        newHeight = newWidth / aspect_ratio; // Aspect ratio is 1, so newHeight = newWidth
    }

    // Set width and height of the Canvas
    RectTransform rectTransform = GetComponent<RectTransform>();

    rectTransform.sizeDelta = new UnityEngine.Vector2(newWidth, newHeight);
}

private void PositionWholeImage(Transform CurrentImage, Transform Panel, Camera userCamera)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        // Set at the centre of the screen
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        transform.localPosition = Vector3.zero;

        // Set position of the image at same x position as panel and at the same z position as panel but at y position of panel + height of the image
        float y = Panel.GetComponent<RectTransform>().sizeDelta.y;

        Debug.Log($"The y position of the panel is {y}");

        // Set size of the image
        ResizeImgtobewithinFOV((y+transform.position.z)/2, userCamera);

        
        // Set angle of the image to panel at 90°
        rectTransform.position = new Vector3(rectTransform.position.x, rectTransform.position.y + y, rectTransform.position.z);

        // Last incase, it affects positioning
        rectTransform.localRotation = UnityEngine.Quaternion.Euler(90, 0, 0);
    }


    private Texture2D LoadTexture(string img_path)
    {
        
        //Texture2D texture = Resources.Load<Texture2D>(Path.Combine("MicroNuclAI", name));
        byte[] fileData = File.ReadAllBytes(img_path);
        (float width, float height) = GetDimensions(img_path);
        Debug.Log($"Size of img: {width} {height}");
        Texture2D texture = new Texture2D((int)width, (int)height);
        texture.LoadImage(fileData);

        return texture;
    }

    public (float width, float height) GetDimensions(string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            stream.Seek(8, SeekOrigin.Begin);

            byte[] chunkLength = new byte[4];
            byte[] chunkType = new byte[4];
            stream.Read(chunkLength, 0, 4);
            stream.Read(chunkType, 0, 4);

            string type = System.Text.Encoding.ASCII.GetString(chunkType);
            if (type != "IHDR")
            {
                throw new Exception("IHDR chunk not found");
            }

            byte[] dimensions = new byte[8];
            stream.Read(dimensions, 0, 8);

            width = BitConverter.ToInt32(new byte[] { dimensions[3], dimensions[2], dimensions[1], dimensions[0] }, 0);
            height = BitConverter.ToInt32(new byte[] { dimensions[7], dimensions[6], dimensions[5], dimensions[4] }, 0);

            return (width, height);
        }
    }

    private void PositionImagetitle(Transform title)
    {
        // Create Title

        TMP_Text tmpText = title.GetComponent<TextMeshProUGUI>();

        // Position -90° from whole image, this causes its axis to rotated too
        title.GetComponent<RectTransform>().localRotation = UnityEngine.Quaternion.Euler(270, 0, 0);

        float d_height = GetComponent<RectTransform>().rect.height;
        float d_width = GetComponent<RectTransform>().rect.width;

        // Position at whole image height distance in z axis.
        title.GetComponent<RectTransform>().position = new UnityEngine.Vector3(transform.position.x, transform.position.y, 
        transform.position.z  + (height/2) * 1.5f);

        tmpText.text = "Whole image";
        tmpText.fontSize = newHeight *0.1f;
        tmpText.alignment = TextAlignmentOptions.Center;

                // Set all text margins to 0
        tmpText.margin = new UnityEngine.Vector4(0, 0, 0, 0);

        ContentSizeFitter fitter = title.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;


    }






}