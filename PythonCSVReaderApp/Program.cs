// See https://aka.ms/new-console-template for more information
using System;
using System.Collections;
using System.Collections.Generic;


class Program
{
    static void Main(string[] args)
    {
        // Call the local function
        read_csv_with_python(1);

        // Other code in your Main method
    }

 static void read_csv_with_python(int cell_indx)
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

    // Start the process
    process.Start();

    // Read the output and error streams
    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();

    // Wait for the process to finish
    process.WaitForExit();

    // Check if there was any error
    if (!string.IsNullOrEmpty(error))
    {
        Console.WriteLine(error);
        return;
    }

    Console.WriteLine($"Output looks like {output} and is of type {output.GetType()}");
    // Convert the output JSON string to a dictionary
    List<Dictionary<string, object>> data_list = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(output);
    Dictionary<string, object> data = data_list[0];
    
    if (data == null)
    {
        Console.WriteLine("Failed to parse the output JSON string");
        return;
    }

    // Access the dictionary values as needed
    foreach (KeyValuePair<string, object> entry in data)
    {
        string key = entry.Key;
        object value = entry.Value;

        // Do something with the key-value pair
        // For example, print them to the console
        Console.WriteLine($"Key: {key}, Value: {value}");
    }

        // Process the output as needed
    Console.WriteLine(data.Values);
}


}
