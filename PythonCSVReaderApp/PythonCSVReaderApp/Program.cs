// See https://aka.ms/new-console-template for more information
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Newtonsoft.Json;

class Program
{  
    static void Main(string[] args)
    {
        // Call the local function
        get_bbox_from_df(1);

        // Other code in your Main method
    }

    private static  List<element>  read_csv_with_python()
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

    // Start the process
    process.Start();

    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();

    // Wait for the process to finish
    process.WaitForExit();

    Console.WriteLine($"Output is of type {output.GetType()}");

    if (error != "")
    {
        Console.WriteLine($"Error is {error}");
    }

    return ConvertOutputToDictionary(output);

    }

public class element
{
    public int x_min { get; set; }
    public int x_max { get; set; }
    public int y_min { get; set; }
    public int y_max { get; set; }
}



    private static List<element> ConvertOutputToDictionary(string output)
    {
        List<element> result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<element>>(output);
        Console.WriteLine($"Data dict keys are {result[0]}");

        return result;
    }


    private static void get_bbox_from_df(int patch_indx)
    {
        // Get the bounding box of the pixel cluster
        List<element> data = read_csv_with_python();
        
        element patchData = data[patch_indx];
        int width = (int)(patchData.x_max - patchData.x_min);
        int height = (int)(patchData.y_max - patchData.y_min);
        Console.WriteLine($"Width is {width} and height is {height}");
        //Rect bbox = new Rect((int)patchData["x_min"], (int)patchData["y_min"], width, height);

       // return bbox;
    }

}
