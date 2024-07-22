using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Unity.Jobs;
using Unity.Collections; // Add this line to use the Thread class


public class whole_image : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //read_csv_with_python(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }




    private System.Diagnostics.Process set_python_script(int cell_indx)
    {

    string pythonScriptPath = Path.Combine("/media/ibrahim/Extended Storage/OneDrive/Internship/VR_schapiro/repos/cell_tinder/python_codes", "read_df.py");
    pythonScriptPath = $"\"{pythonScriptPath}\"";


    // Create a new process to run the Python script
    System.Diagnostics.Process process = new System.Diagnostics.Process();
    process.StartInfo.FileName = "/home/ibrahim/miniconda3/bin/python";
    process.StartInfo.Arguments = $"{pythonScriptPath} --cell_index {cell_indx}";
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;

    return process;


    }


    private Dictionary<string, object>  ExecutePythonProcess(System.Diagnostics.Process process)
    {
        // Start the process
        process.Start();
    
        string output = process.StandardOutput.ReadToEnd();
    
        // Wait for the process to finish
        process.WaitForExit();

        Console.WriteLine($"Output looks like {output} and is of type {output.GetType()}");

        Dictionary<string, object> data = ConvertOutputToDictionary(output);

        return data;

    }


    private Dictionary<string, object> ConvertOutputToDictionary(string output)
    {
        // Convert the output JSON string to a dictionary
        List<Dictionary<string, object>> data_list = JsonUtility.FromJson<List<Dictionary<string, object>>>(output);
        Dictionary<string, object> data = data_list[0];

        return data;
    }

    private void read_csv_with_python(int cell_indx)
    {
        // Set the terminal process
        System.Diagnostics.Process process = set_python_script(cell_indx);

         Dictionary<string, object> data = ExecutePythonProcess(process);

        if (data == null)
        {
            Console.WriteLine("Failed to parse the output JSON string");
        }

        else {
            // Access the dictionary values as needed
            foreach (KeyValuePair<string, object> entry in data)
            {
                string key = entry.Key;
                object value = entry.Value;

                // Do something with the key-value pair
                // For example, print them to the console
                Console.WriteLine($"Key: {key}, Value: {value}");
            }
        }


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
}
