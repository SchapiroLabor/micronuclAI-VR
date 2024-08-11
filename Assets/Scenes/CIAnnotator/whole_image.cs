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

public class whole_image : MonoBehaviour
{
   List<element>  data_dict;
   private InteractableImageStack Canvas_script;
   private GameObject Arrow;
   private float height;
   private float width;

   private float raycast_distance = 10f; // Default distance to raycast from the camera, please do not change this !!!

    // Start is called before the first frame update
    public void Initialize()
    {
        Canvas_script = transform.parent.GetComponent<InteractableImageStack>();

        read_csv_with_python();
        
        SetTextureOnWholeImage();

        PositionWholeImage();

        PositionImagetitle();

        //InitializeIntensityCylinder();
    }


    public class element
    {   // X, Y = Width, Height
        public int x_min { get; set; } 
        public int x_max { get; set; }
        public int y_min { get; set; }
        public int y_max { get; set; }
    }

    private void read_csv_with_python()
    {
        string pythonScriptPath = Path.Combine("/media/ibrahim/Extended Storage/OneDrive/Internship/VR_schapiro/repos/cell_tinder/python_codes", "read_df.py");
        pythonScriptPath = $"\"{pythonScriptPath}\"";

        // Create a new process to run the Python script
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "/home/ibrahim/miniconda3/bin/python";
        process.StartInfo.Arguments = $"{pythonScriptPath}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

        // Start the process
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        // Wait for the process to finish
        process.WaitForExit();

        if (error != "")
        {
            Debug.Log($"Error is {error}");
        }

        data_dict = ConvertOutputToDictionary(output);
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
        // Get the bounding box of the pixel cluster
        Rect bbox = get_bbox_from_df(patch_indx);

        Debug.Log($"The bounding box is {bbox}");

        // Color the pixel cluster
        ColorPixelCluster(bbox, Color.red);
    }

    public void DisplayPatch()
    {
        current_cell_bbox(Canvas_script.current_img_indx);

        DisplayArrow();

    }

private void InitializeIntensityCylinder()
{
string prefabPath = "Assets/Scenes/CIAnnotator/Cylinder.prefab";
Arrow = Canvas_script.CreateGameObject(transform, prefabPath);
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


Debug.Log($"Get Arrow and player world rotation {Arrow.transform.rotation} and {Camera.main.transform.rotation}");
    return mid_point_world_with_offset;
}


private void DisplayArrow()
{
    // Get the patch position
    element patch_position = data_dict[Canvas_script.current_img_indx];

    Debug.Log($"Current image index is {Canvas_script.current_img_indx}");

    // Get the arrow position and rotation
    UnityEngine.Vector3 position = GetArrowPosition(patch_position);

    // Position the arrow
    PositionArrow(position);

}

private void SetTextureOnWholeImage()
    {
        Texture2D whole_img_texture = LoadTexture();

        if (whole_img_texture == null)
        {
            Debug.Log("Whole image texture is null");
        }

        GetComponent<RawImage>().texture = whole_img_texture;
        RectTransform rectTransform = GetComponent<RawImage>().GetComponent<RectTransform>();
        rectTransform.sizeDelta = new UnityEngine.Vector2(whole_img_texture.width, whole_img_texture.height);

        // Reduce Local Scale
        rectTransform.localScale = new UnityEngine.Vector3(0.1f, 0.1f, 1f);



    }

    private List<float> GetFOVatWD(float WD)
    {
        // Pythagoras theorem to calculate the distance
        List<float> holder = new List<float>();
        float vertical_fov = Camera.main.fieldOfView;
        float fov_height = (WD * Mathf.Tan(vertical_fov * 0.5f)) * 2;
        float fov_width =  Camera.main.aspect * fov_height;     // Aspect ratio of the camera is width/height

        holder.Add(fov_height);
        holder.Add(fov_width);
        holder.Add(WD);

        return holder;
    }

private void ResizeImgtobewithinFOV(float WD)
{

    // Get the FOV at the panel height
    List<float> outputs = GetFOVatWD(WD);
    float newWidth = outputs[0]*1.5f; // Width
    float newHeight = outputs[1]*1.5f; // Height

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

private void PositionWholeImage()
    {

        // Setup anchors and pivots
        RectTransform rectTransform = GetComponent<RectTransform>();
        Canvas_script.SetupAnchorsAndPivots(rectTransform);
        // Set the anchors and pivots of the Canvas as sizeDelta requires absolute difference
        transform.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        transform.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

        // Reduce Local Scale
        rectTransform.localScale = new UnityEngine.Vector3(1f, 1f, 1f);

        // Set position of the image at same x position as panel and at the same z position as panel but at y position of panel + height of the image
        Vector3 newPosition = Canvas_script.FacePlayer(raycast_distance);
        newPosition.y = transform.parent.GetChild(1).GetComponent<RectTransform>().sizeDelta.y;;

        // Set size of the image
        ResizeImgtobewithinFOV((newPosition.y+newPosition.z)/2);

        // Set angle of the image to panel at 90°
        rectTransform.localRotation = UnityEngine.Quaternion.Euler(90, 0, 0);
        rectTransform.transform.position = newPosition;

    }


    private Texture2D LoadTexture()
    {
        string path = "Assets/Resources/whole_img.png";
        (float width, float height) = GetDimensions(path);
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.R8, false);
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

    private void PositionImagetitle()
    {

        //Canvas_script.ChildIdenticalToParent(transform.gameObject, transform.Find("title").gameObject);

        Transform title = transform.Find("title");

        TMP_Text tmpText = title.GetComponent<TextMeshProUGUI>();

        // Position -90° from whole image, this causes its axis to rotated too
        title.GetComponent<RectTransform>().localRotation = UnityEngine.Quaternion.Euler(270, 0, 0);

        // Position at whole image height distance in z axis.
        title.GetComponent<RectTransform>().position = new UnityEngine.Vector3(transform.position.x, transform.position.y, transform.position.z  + (GetComponent<RectTransform>().sizeDelta.y/2) * 1.5f);
        Debug.Log($"The position of the title is {title.GetComponent<RectTransform>().position}");


        // Set size of text box to be same width but aspect ratio  for height
        title.GetComponent<RectTransform>().sizeDelta = new UnityEngine.Vector2(GetComponent<RectTransform>().sizeDelta.x, GetComponent<RectTransform>().sizeDelta.y/9);

        tmpText.text = "Whole image";
        tmpText.fontSize = GetComponent<RectTransform>().sizeDelta.y *0.1f;
        tmpText.alignment = TextAlignmentOptions.Center;

                // Set all text margins to 0
        tmpText.margin = new UnityEngine.Vector4(0, 0, 0, 0);
    }



}