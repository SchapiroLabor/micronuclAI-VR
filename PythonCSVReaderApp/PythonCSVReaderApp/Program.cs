// See https://aka.ms/new-console-template for more information
using System;
using System.Collections;
using System.Collections.Generic;


class Program
{  private Dictionary<string, Dictionary<string, float>> data_dict;
    static void Main(string[] args)
    {
        // Call the local function
        get_bbox_from_df(1);

        // Other code in your Main method
    }

    private static Dictionary<string, Dictionary<string, float>> read_csv_with_python()
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

    // Wait for the process to finish
    process.WaitForExit();

    Console.WriteLine($"Output is of type {output.GetType()}");

    return ConvertOutputToDictionary(output);

    }

    private static Dictionary<string, Dictionary<string, float>> ConvertOutputToDictionary(string output)
    {
        // Convert the output JSON string to a dictionary
        Dictionary<string, object> data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, float>>>(output);

        return data;
    }


    private static void get_bbox_from_df(int patch_indx)
    {
        // Get the bounding box of the pixel cluster
        Dictionary<string, object> data_dict = read_csv_with_python();
        Console.WriteLine($"Data dict keys are {data_dict.Keys.Count}");
        Dictionary<string, float> patchData = data_dict[data_dict.Keys.ToList()[patch_indx]];
        int width = (int)(patchData["x_max"] - patchData["x_min"]);
        int height = (int)(patchData["y_max"] - patchData["y_min"]);
        //Rect bbox = new Rect((int)patchData["x_min"], (int)patchData["y_min"], width, height);

       // return bbox;
    }

}
