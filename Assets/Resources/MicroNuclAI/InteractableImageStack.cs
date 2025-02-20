using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Debug = UnityEngine.Debug;
using System;
using Newtonsoft.Json;
using static Logger;
using System.Threading.Tasks;

public class InteractableImageStack : MonoBehaviour
{
    public Camera userCamera;  // Reference to the user's camera
    private ClickNextImage CurrentImage;
    private whole_image WholeImage;
    private GridMaker Panel;
    public GameObject GameManager;
    private RectTransform rectTransform;
    public string inputfolder;
    public string python_exe;
    private string PythonScript = "python_codes/save_as_df.py";
    private bool Ready2Exit = false;
    private float raycast_distance = 10f; // Default distance to raycast from the camera, please do not change this !!
    public GameObject CanvasUI;
    private string python_path;
    public static List<element> data_dict = null;
    public bool isReady = false;



            public class element
    {   // X, Y = Width, Height
        public int x_min { get; set; } 
        public int x_max { get; set; }
        public int y_min { get; set; }
        public int y_max { get; set; }

    }

    private void Awake()
    {   
        // Is played before start and 
        
        // Load the Game Manager
        if (GameManager == null)
        {
            // Load from path
            GameManager = Resources.Load<GameObject>(Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/SceneManager.prefab")));
        }

        // Position Canvas once it is enabled
        PositionCanvas();

                // Get the input folder and python executable
        inputfolder = GameManager.GetComponent<GameManaging>().InputFolder;
        python_exe = GameManager.GetComponent<GameManaging>().PythonExecutable;
        
        GetBBoxes(inputfolder);
        // Position the Canvas in front of the camera
        PositionCanvas();
     
    }

    void PositionCanvas()
    {

        // Setup anchors and pivots
        rectTransform = GetComponent<RectTransform>();
        SetupAnchorsAndPivots(rectTransform);

        // Set anchor to the centre of the screen
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        // Set pivot to the centre of the screen
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Set local position to the centre of the screen at a distance of 10 units
        rectTransform.localPosition = FacePlayer(raycast_distance);

        // Set rotation of the Canvas to face the camera
        transform.rotation = Quaternion.Euler(Vector3.zero);

        // Set scale to 1
        transform.localScale = new Vector3(1, 1, 1);


    }
    private void GetBBoxes(string data_dir)
    {
        isReady = false;

        try
        {
            // Create a new list to store the data
            data_dict = new List<element>();
            read_csv_with_python(data_dir);

            // Read the CSV file with Python
            //Task.Run(() => read_csv_with_python(data_dir)).Wait();
        }
        catch (Exception e)
        {
            Logger.Log($"An error occurred: {e.Message} with stack trace {e.StackTrace}");
        }

        finally
        {
            isReady = true;
        }


    }

    private void Start()
    {
        if (userCamera == null)
        {
            userCamera = Camera.main;  // Use the main camera if no camera is assigned
        }

        // Ensure the Canvas is using World Space
        Canvas canvas = GetComponent<Canvas>();
        if (canvas.renderMode != RenderMode.WorldSpace)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }

        // Get the RawImage, Whole Image, Trash and UserCamera        
        Panel = transform.GetComponentInChildren<GridMaker>();
        Panel.Initialize();

        CurrentImage = Panel.GetComponentInChildren<ClickNextImage>();

        WholeImage = transform.GetComponentInChildren<whole_image>();
        
        WholeImage.Initialize(transform, Panel.transform, userCamera, CurrentImage);

        // Initialize the Canvas
        Initialize(CurrentImage.transform, WholeImage.transform, Panel.transform, userCamera);

    }
    
private void read_csv_with_python(string data_dir)
{
    // Path to the CSV file
    string csvFilePath = Path.Combine(data_dir, "bbox.txt");
    
    // Reading the contents of the CSV file
    string csvData = File.ReadAllText(csvFilePath);
    
    // Split the data into lines based on newlines
    string[] lines = csvData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

    // Loop through each line and process it
    for (int i = 0; i < lines.Length; i++)
    {
        // Skip empty lines or lines without commas
        if (string.IsNullOrWhiteSpace(lines[i]) || !lines[i].Contains(','))
        {
            continue;
        }

        // Split the line into values based on commas
        string[] values = lines[i].Split(',');

        // Ensure the correct number of values (5 expected)
        if (values.Length != 5)
        {
            Logger.Log($"Skipping line {i + 1}: Incorrect number of values.");
            continue;
        }

        // Try parsing the values to integers and skip if parsing fails
        if (!int.TryParse(values[0], out int label) ||
            !int.TryParse(values[3], out int x1) ||
            !int.TryParse(values[4], out int x2) ||
            !int.TryParse(values[1], out int y1) ||
            !int.TryParse(values[2], out int y2))
        {
            Logger.Log($"Skipping line {i + 1}: Parsing error.");
            continue;
        }

        else
        {
        data_dict.Add(new element
        {
            x_min = x1,
            x_max = x2,
            y_min = y1,
            y_max = y2
        });
        }
    


        
    }


}



    static public string AddQuotesIfRequired(string path)
{
    /// <summary>
    /// Adds quotes to a string if required.
    /// </summary>
    /// <param name="path">The string to add quotes to.</param>
    /// <returns>The input string with quotes added if required, or an empty string if the input is null, empty, or whitespace.</returns>

    return !string.IsNullOrWhiteSpace(path) ? 
        path.Contains(" ") && (!path.StartsWith("\"") && !path.EndsWith("\"")) ? 
            "\"" + path + "\"" : path : 
            string.Empty;
}

    private void Initialize(Transform CurrentImage, Transform WholeImage, Transform Panel,
    Camera userCamera)
    {   
        // Calculate the new position for the Canvas to minimum clipping distance
        transform.position = FacePlayer(raycast_distance);
        List<float> outputs = GetFOVatWD(raycast_distance, userCamera);
        rectTransform.sizeDelta = new Vector2(outputs[1], outputs[0]);

        // Instanziate Exit Button
        SetupSeperateButton(CurrentImage, transform);

        // Set up Exit Button
        InstantiateCanvasUI(CurrentImage, WholeImage, Panel);

    }

    public static void SetupAnchorsAndPivots(RectTransform rectTransform)
    {
        // Set the anchors and pivots of the Canvas
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }

    public static List<float> GetFOVatWD(float WD, Camera userCamera)
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

    public List<float> GetFOVatNearClipping(Camera userCamera)
    {   // Must be near or else the child elements of canvas will not be visible

        // Pythagoras theorem to calculate the distance
        List<float> holder = new List<float>();
        float vertical_fov = userCamera.fieldOfView;
        float clipping_distance = userCamera.nearClipPlane;
        float fov_height = (clipping_distance * Mathf.Tan(vertical_fov * 0.5f)) * 2;
        float fov_width =  userCamera.aspect * fov_height; // Aspect ratio of the camera is width/height

        holder.Add(fov_height);
        holder.Add(fov_width);
        holder.Add(clipping_distance);

        return holder;
    }


    public static Vector3 FacePlayer(float scaler)
    {
        // Face the player
        Vector3 cameraPosition = new Vector3(0,0,0);
        Vector3 cameraForward = new Vector3(0,0,1);

        return cameraPosition + cameraForward * scaler;
    }



    public static GameObject CreateGameObject(Transform parent, string prefabPath, Transform transform)
    {
        // Create a new RawImage GameObject from the prefab
        GameObject instance= Instantiate(Resources.Load<GameObject>(prefabPath), transform.position, transform.rotation);
        instance.transform.SetParent(parent);
        return instance;
    }

public static void ChildIdenticalToParent(GameObject parent, GameObject child)
{
    child.transform.SetParent(parent.transform);
    // Anchors and pivots are the same as the parent
    child.GetComponent<RectTransform>().anchorMin = parent.GetComponent<RectTransform>().anchorMin;
    child.GetComponent<RectTransform>().anchorMax = parent.GetComponent<RectTransform>().anchorMax;
    child.GetComponent<RectTransform>().pivot = parent.GetComponent<RectTransform>().pivot;

    child.transform.position = parent.transform.position;
    child.transform.rotation = parent.transform.rotation;
    child.transform.localScale = parent.transform.localScale;

    // Set the size of the child to be the same as the parent
    child.GetComponent<RectTransform>().sizeDelta = parent.GetComponent<RectTransform>().sizeDelta;


}

private void InstantiateCanvasUI(Transform rawImageTransform, Transform WholeImage, Transform trash)
{   
    // Create a new Canvas UI GameObject
    //GameObject CanvasUI = CreateGameObject(transform, Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/Canvas UI.prefab")), transform);

    // MenuPanel
    CanvasUI.name = "Canvas UI";

    PositionandResizeCanvasUI(CanvasUI, rawImageTransform);
    
    // Setup all the required buttons
    CanvasUI.GetComponent<SetupButtons>().Initialize(rawImageTransform, CanvasUI.transform.position, CanvasUI.transform.rotation, WholeImage, trash);
    
}

private void PositionandResizeCanvasUI(GameObject CanvasUI, Transform rawImageTransform)
{
    // Set the anchors and pivots of the Canvas UI
    SetupAnchorsAndPivots(CanvasUI.GetComponent<RectTransform>());

    // Set to bottom left corner the anchors
    CanvasUI.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
    CanvasUI.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

    // Face the Canvas UI to the player
    CanvasUI.transform.rotation = Quaternion.Euler(Vector3.zero);

    // Change pivot to top left corner of the Canvas UI, so no overlap with the RawImage
    CanvasUI.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

    // Set location of the Canvas UI to the top right corner of the RawImage
    CanvasUI.transform.position = new Vector3((rawImageTransform.position.x + rawImageTransform.GetComponent<RectTransform>().sizeDelta.x/2)*1.1f,
    rawImageTransform.position.y + rawImageTransform.GetComponent<RectTransform>().sizeDelta.y/2, rawImageTransform.position.z);

    // Set scale of the Canvas
    CanvasUI.transform.localScale = Vector3.one;
}


    void SetupSeperateButton(Transform ImagePatch, Transform parent)
    {   
        

        // Set position of the button
        Vector3 image_position = ImagePatch.GetComponent<RectTransform>().position;
        float width = ImagePatch.GetComponent<RectTransform>().rect.width;
        float x_shift = width;

        Vector3 position = new Vector3((image_position.x + x_shift)*1.2f, image_position.y, image_position.z);

        // Set rotation of the button
        Quaternion rotation = new Quaternion(0, ImagePatch.transform.rotation.y, ImagePatch.transform.rotation.z, ImagePatch.transform.rotation.w);

        // Set Canvas up
        Transform CanvasUI = SetUICanvasup4ExitButton(parent, rotation, position);

        // Set up exit button
        setupExitButton(ImagePatch, CanvasUI);
         
    }

    private Transform  SetUICanvasup4ExitButton(Transform parent, Quaternion rotation, Vector3 position)
    {
        string prefab_path = Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/Canvas UI.prefab"));
        GameObject CanvasUI = Resources.Load<GameObject>(prefab_path);
        CanvasUI.name = "Canvas UI 4 EXIT Button";
        CanvasUI = Instantiate(CanvasUI);
        CanvasUI.transform.SetParent(parent);

        // Set scale to 1
        CanvasUI.transform.localScale = new Vector3(1, 1, 1);


        // Set anchors to right bottom
        RectTransform rectTransform = CanvasUI.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        // Set pivot to bottom left, for some reason if it is 0,0.5 then it changes its localposition to localPosition.y + 50
        rectTransform.pivot = new Vector2(0, 0); 

        // Set Canvas UI
        CanvasUI.transform.rotation = rotation;
        CanvasUI.transform.position = position;

        //CanvasUI.GetComponent<RectTransform>().sizeDelta = Vector2.one;

        // Add content size fitter componet
        CanvasUI.AddComponent<ContentSizeFitter>();
        CanvasUI.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        CanvasUI.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;



        return CanvasUI.transform;
    }


    private void setupExitButton(Transform ImagePatch, Transform Parent)

    {

    GameObject ExitButton = Resources.Load<GameObject>(Path.Combine("MicroNuclAI",Path.GetFileNameWithoutExtension("MicroNuclAI/Button.prefab")));

    // Instantiate
    ExitButton = Instantiate(ExitButton, Parent);

    // Set name
    ExitButton.name = "ExitButton";

    // Set rotation of the button
    ExitButton.transform.rotation = Parent.rotation;
    // Set position of the button
    ExitButton.transform.position = Parent.position;



    StandardiseExitButton(ExitButton, ImagePatch, Parent.GetComponent<RectTransform>().anchorMin,
    Parent.GetComponent<RectTransform>().anchorMax, Parent.GetComponent<RectTransform>().pivot);

    // Set Canvas to same size as button, for some reason when I set it same to Exit Button, localpositioning of Exit Button is not working
    Parent.GetComponent<RectTransform>().sizeDelta = Vector2.one;

    // Set text of the button
    ExitButton.GetComponentInChildren<TextMeshProUGUI>().text = "EXIT Game";

    // Add a listener to the button
    ExitButton.GetComponent<Button>().onClick.AddListener(() => Exit());

    }

    private void StandardiseExitButton(GameObject Button, Transform ImagePatch, Vector2 AnchorMin, Vector2 AnchorMax, Vector2 Pivot)
    {
    // Set the font size of the Button same to width of image
    Button.GetComponentInChildren<TextMeshProUGUI>().fontSize = ImagePatch.GetComponent<RectTransform>().sizeDelta.x * 0.1f;;

    // Set the size of the Canvas UI to 1/3 of width of image with aspect ratio of 3:1
    Button.GetComponent<RectTransform>().sizeDelta = ResizeButton(ImagePatch);

    // Set alginment of the text in the button
    Button.GetComponentInChildren<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Give button black background and white text
    Button.GetComponent<Image>().color = Color.black;
    Button.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

    // Set text margins to 0
    Button.GetComponentInChildren<TextMeshProUGUI>().margin = new Vector4(0, 0, 0, 0);

    // Setup anchors to bottom left corner
    Button.GetComponent<RectTransform>().anchorMin = AnchorMin;
    Button.GetComponent<RectTransform>().anchorMax = AnchorMax;

        // Set pivot to top right left corner
    Button.GetComponent<RectTransform>().pivot = Pivot;

    // Scale all to one
    Button.transform.localScale = Vector3.one;

    Button.transform.localPosition =  new Vector3(0, 0, 0);

    }


    // Define dictionary class to store the counts of micro nuclei
    public class MicronucleiCounts : Dictionary<string, List<object>>
    {
        public void AddMicronuclei(string key, object value)
        {
            if (!ContainsKey(key))
            {
                this[key] = new List<object>();
            }
            this[key].Add(value);
        }


            // Method to save the data to CSV
    public void SaveToCSV(string filePath)
    {
        // Open a StreamWriter to write to the CSV file
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write the header row (dictionary keys)
            writer.WriteLine(string.Join(",", Keys));

            // Get the maximum number of rows in the dictionary's lists
            int rowCount = 0;
            foreach (var list in Values)
            {
                rowCount = Mathf.Max(rowCount, list.Count);
            }

            // Write each row by iterating through the lists in the dictionary
            for (int i = 0; i < rowCount; i++)
            {
                List<string> row = new List<string>();

                foreach (var key in Keys)
                {
                    if (i < this[key].Count)
                    {
                        row.Add(this[key][i]?.ToString()); // Convert objects to string
                    }
                    else
                    {
                        row.Add(""); // Add an empty string if no value exists for this row
                    }
                }

                writer.WriteLine(string.Join(",", row));
            }
        }

        Debug.Log("CSV file saved at: " + filePath);
    }

    }




    private MicronucleiCounts CollectMicroNucleiCounts()
    {   
        MicronucleiCounts micronucleiCounts = new MicronucleiCounts();


        // Only get basename of the image using 
        Transform Trash = Panel.transform.GetChild(0);

        for (int i = 0; i < Trash.childCount; i++)
        {
            Tinyt script = Trash.GetChild(i).GetComponent<Tinyt>();


            if (script.patches.Count == 0)
            {   
                Logger.Log($"No patches in the image for trash {script.gameObject.name}");
                continue;
            }
            else
            {
            for (int j = 0; j < script.patches.Count; j++)
            {

                micronucleiCounts.AddMicronuclei("img", script.patches_names[j]);
                micronucleiCounts.AddMicronuclei("Micronuclei", script.keys[j]);
                micronucleiCounts.AddMicronuclei("Patch ID", script.patches[j]);
            }
            }

            
        }

        // Jsonify the micronucleiCounts
        //string json = JsonConvert.SerializeObject(micronucleiCounts);
        return micronucleiCounts;

    }

    private void Exit()
    {
        // Get the counts of micro nuclei
        MicronucleiCounts micronucleiCounts = CollectMicroNucleiCounts();

        isReady = false;
        // Thread pool write to Python
        Write2Python(inputfolder, micronucleiCounts);

        OnApplicationQuit();
        //ThreadPooling(new Action<string, string, string, string, string> (Write2Python), isReady,
        //new Action(OnApplicationQuit), PythonScript, python_exe, Application.streamingAssetsPath, inputfolder, micronucleiCounts);

        



    }

void Update()
{
    if (Ready2Exit == true)
    { 
            // Quit the application
            Application.Quit();

            // If running in the Unity Editor, stop play mode
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
    }
}

    private Vector2 ResizeButton(Transform ImagePatch)

    {
    
    // Get the width and height of the RawImage
    float width = ImagePatch.GetComponent<RectTransform>().rect.width;
    float height = ImagePatch.GetComponent<RectTransform>().rect.height;

    float scaled_width = width* ImagePatch.GetComponent<RectTransform>().localScale.x;
    float scaled_height = height * ImagePatch.GetComponent<RectTransform>().localScale.y;

    // Set the size of the Canvas UI to 1/3 of width of image with aspect ratio of 3:1
     return new UnityEngine.Vector2(scaled_width/3, scaled_width/6);

    }



    public static System.Diagnostics.Process SetupPythonProcess(string ScriptPath, string python_exe,
    string argument = null)
    {   
     
        
        // Create a new process to run the Python script    
        return new System.Diagnostics.Process
        {   
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = python_exe,
                Arguments = $"{ScriptPath} {argument}",
                RedirectStandardOutput = true,
                RedirectStandardError = true, // Redirect the standard error to capture it
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
    }


    public static string ReadfromPython(string ScriptPath, string python_exe, string argument = null)
    {
        // Create a new process to run the Python script
        System.Diagnostics.Process process = SetupPythonProcess(ScriptPath, python_exe, argument);

        // Start the process
        process.Start();

        // Read the output from the Python script
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        // Wait for the process to finish
        process.WaitForExit();
        process.Close();

        if (!string.IsNullOrEmpty(error))
        {   Logger.Log($"Error from Python script: {error}");
    
        }

        // Return the output from the Python script
        return output;
    }

/*
    public static void Write2Python(string ScriptName, string python_exe,
    string workingdirectory, string data_dir, string message)
    {   
  
        System.IO.File.WriteAllText(System.IO.Path.Combine(data_dir, "results", "message.txt"), message);
        Logger.Log($"Message written to {System.IO.Path.Combine(data_dir, "message.txt")}");
        try
        {
        string argmuents = AddQuotesIfRequired(workingdirectory) + " " + AddQuotesIfRequired(data_dir);

        System.Diagnostics.Process process = SetupPythonProcess(AddQuotesIfRequired(Path.Combine(workingdirectory, ScriptName)), python_exe,
        argmuents);


        // Redirect the standard output and error to capture them
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;


                // Start the process
        process.Start();

        // Read the output from the Python script
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        // Wait for the process to finish
        process.WaitForExit();
        process.Close();

        if (!string.IsNullOrEmpty(error))
        {   Logger.Log($"Error from Python script: {error}");
    
        }

        }
        catch (Exception e)
        {
            Logger.Log($"An error occurred: {e.Message} with stack trace {e.StackTrace}");
        }



    }
*/

void Write2Python(string data_dir,  MicronucleiCounts micronucleiCounts)
{

        string filename = "output.csv";

        // Create new directory if current one does not exists
        string results_dir = Path.Combine(data_dir, "results");
        if (!Directory.Exists(results_dir))
        {
            Directory.CreateDirectory(results_dir);
        }

        string filePath = Path.Combine(results_dir, filename);

        micronucleiCounts.SaveToCSV(filePath);



}

    void OnApplicationQuit()
    {
        // Set Ready2Exit to true
        Ready2Exit = true;
    }
 public static void ThreadPooling(Delegate method, bool isReady, Action finalAction = null, params object[] args)

 {      // Params array in conjunction with a Delegate to pass a dynamic number of arguments to your method
        // Params keyword allows the method to accept a variable number of arguments
        // The method.DynamicInvoke(args) call dynamically invokes the delegate with the provided arguments. 
        // This makes it possible to pass any number of arguments, as long as they match the delegate's signature.

        Logger.Log("Function invoked successfully");
        // Execute on a second thread
        System.Threading.ThreadPool.QueueUserWorkItem((state) =>
        {
        try
        { // Ensures that any exceptions thrown by method(args) are caught and handled properly within the thread
        Logger.Log("Function started successfully");
            method.DynamicInvoke(args);
                       Logger.Log("Function ended successfully");

                       

        }

        catch (Exception e)
        { // Catch any exceptions thrown by method(args) and log them
             Logger.Log($"An error occurred: {e.Message} with stack trace {e.StackTrace}");
        }
        finally
        { // Will execute regardless of whether an exception is thrown, 
          //ensuring that cleanup actions like setting Ready2Exit = true and logging the exit message are always performed. 
          //This is especially important for maintaining the application's state and ensuring resources are cleaned up correctly.
            
            if (finalAction != null)
            {finalAction?.Invoke();}
            isReady = true;
        } 
        }); 




 }




/*
    public static void Write2Python(string ScriptPath, string python_exe, string message)
    {   

        // Throws a pipe is broken exception. Not sure why that is. 

        // Event to signal when the client is ready
            // ManualResetEvent clientReady = new ManualResetEvent(false);

        using (AnonymousPipeServerStream pipeServer =
            new AnonymousPipeServerStream(PipeDirection.Out,
            HandleInheritability.Inheritable))
        {
            Logger.Log($"[SERVER] Current TransmissionMode: {pipeServer.TransmissionMode}.");

            // Pass the client process a handle to the server and execute it
            System.Diagnostics.Process pipeClient = SetupPythonProcess(ScriptPath, python_exe, pipeServer.GetClientHandleAsString());

            Logger.Log($"[SERVER] Client handle: {pipeServer.GetClientHandleAsString()}");
            pipeClient.Start();
            // Remove the client handle from the local variable list to free up memory
            pipeServer.DisposeLocalCopyOfClientHandle(); 

            try
            {   
                // Wait for the client to signal that it's ready
                    // Block the current thread until the client signals
                //clientReady.WaitOne();

                // Read user input and send that to the client process.
                using (StreamWriter sw = new StreamWriter(pipeServer))
                {
                    // Flush buffer to stream after every write. 
                        // Means, write data now and do not leave it in memory
                        // Don't use for frequent communication, it slows down the process
                    sw.AutoFlush = true;
                    // Send output and add line terminator to it
                    sw.WriteLine(message);

                    // Ensures all data is written to pipe before continuing execution 
                        //of current thread i.e. will block thread.
                        // Does not apear to work 
                    pipeServer.WaitForPipeDrain();

                }

                // Capture and print the error output from the Python script
                string errorOutput = pipeClient.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    Debug.LogError($"[SERVER] Error from Python script: {errorOutput}");
                }
                
            }
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            catch (IOException e)
            {
                Logger.Log($"[SERVER] Error: {e.Message}");
            }
        
                // Read the output and error from the Python script
        string output = pipeClient.StandardOutput.ReadToEnd();
        string error = pipeClient.StandardError.ReadToEnd();

        pipeClient.WaitForExit();
        pipeClient.Close();

                // Print the output and error from the Python script
        Logger.Log(output);
        Debug.LogError(error);

        }


    }*/

    


}
