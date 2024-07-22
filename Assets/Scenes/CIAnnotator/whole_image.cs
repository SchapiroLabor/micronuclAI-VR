using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class whole_image : MonoBehaviour
{
   List<element>  data_dict;

    // Start is called before the first frame update
    void Start()
    {
        read_csv_with_python();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        Debug.Log($"The axes ranges are {patchData.x_min}, {patchData.x_max}, {patchData.y_min}, {patchData.y_max}");

        return bbox;
    }

    public void current_cell_bbox(int patch_indx)
    {
        // Get the bounding box of the pixel cluster
        Rect bbox = get_bbox_from_df(patch_indx);

        // Color the pixel cluster
        ColorPixelCluster(bbox, Color.red);
    }

    
}
