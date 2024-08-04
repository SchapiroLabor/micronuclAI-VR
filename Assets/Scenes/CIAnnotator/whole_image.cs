using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class whole_image : MonoBehaviour
{
   List<element>  data_dict;
   public InteractableImageStack Canvas_script;
   private GameObject Arrow;

    // Start is called before the first frame update
    public void Initialize()
    {
        read_csv_with_python();
        CreateArrow();

        set_texture2whole_img();
        PositionImagetitle();
    }


    public class element
    {
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
                originalTexture.SetPixel(x, y, newColor);
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
        Debug.Log($"The axes ranges are FOR X {patchData.x_min}, {patchData.x_max} ADN FOR Y {patchData.y_min}, {patchData.y_max}");

        return bbox;
    }

    public void current_cell_bbox(int patch_indx)
    {
        // Get the bounding box of the pixel cluster
        Rect bbox = get_bbox_from_df(patch_indx);

        // Color the pixel cluster
        ColorPixelCluster(bbox, Color.red);
    }

    public void DisplayPatch()
    {
        current_cell_bbox(Canvas_script.current_img_indx);
        DisplayArrow();

    }

private void CreateArrow()
{
    string prefabPath = "Assets/Samples/XR Interaction Toolkit/2.5.3/Starter Assets/Models/Primitive_Cylinder.fbx";
    Arrow = Canvas_script.CreateGameObject(transform, prefabPath);
    Arrow.SetActive(false);

    // Size of arrow can only be changed via scale propoerty
// Make scaling 10% of the size of the parent
float x_scale = GetComponent<RawImage>().texture.width * 0.1f;
float y_scale = GetComponent<RawImage>().texture.height * 0.1f;
Arrow.transform.localScale = new UnityEngine.Vector3(4000, 500, 10);

}

private void PositionArrow(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
{



Arrow.transform.localPosition = new UnityEngine.Vector3(position.x, position.y, position.z*10);
Debug.Log($"The position of cylinder is {Arrow.transform.localPosition}");
Arrow.transform.localRotation =rotation;
Debug.Log($"After rotation the mid point world with offset is {Arrow.transform.position}");
if (!Arrow.activeSelf){
Arrow.SetActive(true);
}

}

private (UnityEngine.Vector3, UnityEngine.Quaternion) GetArrowPositionAndRotation(element patch_position)
{
    // Get mid point from the patch position
    UnityEngine.Vector2 mid_point_pixel = new UnityEngine.Vector2((patch_position.y_min + patch_position.y_max)/2, (patch_position.x_min + patch_position.x_max)/2);
    UnityEngine.Vector3 mid_point_world = Pixel2WorldCoordinate(mid_point_pixel);

    // Add y offset
    UnityEngine.Vector2 max_bounds = Pixel2WorldCoordinate(new UnityEngine.Vector2(patch_position.x_max, patch_position.y_max));
    UnityEngine.Vector3 mid_point_world_with_offset = new UnityEngine.Vector3(max_bounds.x, max_bounds.y, -transform.position.z);
    

    // Get the rotation in 150° Angle
    UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(0, 90, 0);
    

    return (mid_point_world_with_offset, rotation);
}


private UnityEngine.Vector2 Pixel2WorldCoordinate(UnityEngine.Vector2 PixelCoordinate)

{       
    /*
        // Normalize pixel coords 
        // Remember, both pixel coords and local coords are in the range [0, 1] and have origin at the bottom-left corner
        UnityEngine.Vector2 NormalizedPixelCoordinate = new UnityEngine.Vector2(PixelCoordinate.x / GetComponent<RawImage>().texture.width,
        PixelCoordinate.y / GetComponent<RawImage>().texture.height);
        Debug.Log($"The normalized pixel coordinate is {NormalizedPixelCoordinate}");
        // Convert normalized pixel coords to world coords
        Debug.Log($"The transform size is {GetComponent<RectTransform>().rect.size}");
        UnityEngine.Vector2 WorldCoordinate = transform.TransformPoint(NormalizedPixelCoordinate);
        Debug.Log($"The world coordinate is {WorldCoordinate}");
    */

        return PixelCoordinate;



}

public void DisplayArrow()
{
    // Get the patch position
    element patch_position = data_dict[Canvas_script.current_img_indx];

    // Get the arrow position and rotation
    (UnityEngine.Vector3 position, UnityEngine.Quaternion rotation) = GetArrowPositionAndRotation(patch_position);

    // Position the arrow
    PositionArrow(position, rotation);

}

    private void set_texture2whole_img()
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

    private Texture2D LoadTexture()
    {
        string path = "Assets/Resources/whole_img.png";
        (int width, int height) = GetDimensions(path);
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(width, height, TextureFormat.R8, false);
        texture.LoadImage(fileData);
        return texture;
    }

    public static (int width, int height) GetDimensions(string filePath)
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

            int width = BitConverter.ToInt32(new byte[] { dimensions[3], dimensions[2], dimensions[1], dimensions[0] }, 0);
            int height = BitConverter.ToInt32(new byte[] { dimensions[7], dimensions[6], dimensions[5], dimensions[4] }, 0);

            return (width, height);
        }
    }

    private void PositionImagetitle()
    {
        TMP_Text tmpText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        tmpText.transform.localScale = new UnityEngine.Vector3(1, 1, 1);
        tmpText.text = "Whole image";
        tmpText.fontSize = 600;
        tmpText.alignment = TextAlignmentOptions.TopGeoAligned;
    }


}