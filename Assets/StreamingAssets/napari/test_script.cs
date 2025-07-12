

namespace PythonCSVReaderApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadCsvWithPython(1); // Example call with cell index 1
        }

private void read_csv_with_python(int cell_indx)
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
        Debug.LogError(error);
        return;
    }

    // Convert the output JSON string to a dictionary
    Dictionary<string, object> data = JsonUtility.FromJson<Dictionary<string, object>>(output);

    if (data == null)
    {
        Debug.LogError("Failed to parse the output JSON string");
        return;
    }

    // Access the dictionary values as needed
    foreach (KeyValuePair<string, object> entry in data)
    {
        string key = entry.Key;
        object value = entry.Value;

        // Do something with the key-value pair
        // For example, print them to the console
        Debug.Log($"Key: {key}, Value: {value}");
    }

        // Process the output as needed
    Debug.Log(data.Values);
}
    }

}
