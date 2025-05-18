using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
using System.IO;
using Debug = UnityEngine.Debug;
using System;
using System.Threading.Tasks;
using General;
using System.Threading;
using UnityEditor.ShaderGraph.Internal;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.Reflection;
using UnityEditor;
using UnityEngine.Events;
using Unity.PlasticSCM.Editor.WebApi;
using TMPro;
using Unity.XR.CoreUtils;

namespace CinAnnotator
{
    public class InteractableImageStack : MonoBehaviour
    {
        [Serializable]
        public class MyChangeEvent : UnityEvent<bool>
        {
            public bool value;
        }

        [SerializeField] MyChangeEvent PythonWorkerEvent = new MyChangeEvent(); // Event to be triggered when the value changes
        public Camera userCamera;  // Reference to the user's camera
        [SerializeField] private ClickNextImage CurrentImage;
        [SerializeField] private WholeImage WholeImage;
        [SerializeField] private GridMaker Panel;
        [SerializeField] private GameManaging gameManaging;
        private RectTransform rectTransform;
        private string inputfolder;
        private string python_exe;
        public DataFrame bbox_dict;
        private string PythonScript = "python_codes/save_as_df.py";
        private bool Ready2Exit = false;
        private float raycast_distance = 10f; // Default distance to raycast from the camera, please do not change this !!
        public GameObject CanvasUI;
        public static List<element> data_dict = null;
        public bool isReady = false;

        [SerializeField] GameObject load_indicator;

        //VisualElement PythonToggle = new VisualElement();

        [SerializeField] Toggle PythonToggle;

        string processName;



        public class element
        {   // X, Y = Width, Height
            public int x_min { get; set; }
            public int x_max { get; set; }
            public int y_min { get; set; }
            public int y_max { get; set; }

        }

        void Awake()
        {
            // Is played before start and 

            // Load the Game Manager
            // Why do we need to load the GameManaging scriptable object here?
            if (gameManaging == null)
            {
                // Load from path
                gameManaging = Resources.Load<GameObject>(Path.Combine("MicroNuclAI", Path.GetFileNameWithoutExtension("MicroNuclAI/SceneManager.prefab"))).GetComponent<GameManaging>();
            }

            // Position Canvas once it is enabled
            //PositionCanvas();

            // Get the input folder and python executable
            //inputfolder = gameManaging.InputFolder;
            //PythonToggle = new Toggle("Test Toggle");
            //PythonToggle.name = "Python Toggle";

            //PythonToggle.RegisterValueChangedCallback(PythonProcessStartCallback);


            /* GetBBoxes(inputfolder); */

            // Test python subprocess
            /*             Action<object> initPythonAction = (args) => PythonIPC.InitPython(args as string);
                        StartCoroutine(BuildCoroutine(initPythonAction, python_exe)); */


        }
        void PythonProcessStartCallback()
        { // Added as callback in the Editor. For some reason cannot be added in script.

            if (PythonWorkerEvent.value == true)
            {
                // Create text field to state image is loading
                load_indicator.GetComponent<TMP_Text>().text = $"Please wait until loading ended";
                load_indicator.GetComponent<TMP_Text>().fontSize = 20;
                GameObject image = GameObject.Find("Panel").GetNamedChild("Image");
                Vector3 image_position = image.transform.position;
                load_indicator.transform.position = image_position;
                load_indicator.SetActive(true);
            }
            else if (PythonWorkerEvent.value == false)
            {
                // Stop the Python process here
                // Create text field to state image is loading
                load_indicator.SetActive(false);
                // Get the text field to update textField.GetComponent<TMP_Text>().text = $"Process {"processName"} finished";
            }
            else
            {
                Debug.Log($"Process failed to start.");
            }


        }

        private void SendPythonProcessEvent(MyChangeEvent PythonWorkerEvent, bool Value)
        {
            // Set the value of the event and invoke it
            PythonWorkerEvent.value = Value;
            PythonWorkerEvent.Invoke(Value); // Why add it to the invoke function ?
        }




        [Serializable]
        public class DataFrame
        {   // Get the BBOX, index and image path
            public List<int> X1;
            public List<int> X2;
            public List<int> Y1;
            public List<int> Y2;
            public List<int> Index;
            public List<int> Image_path;
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

            inputfolder = @"D:\OneDrive\Desktop\Internship\VR_schapiro\data\data";
            python_exe = @"D:\OneDrive\Desktop\Internship\VR_schapiro\repos\micronuclAI-VR\Assets\venv\MNAIVR\Scripts\python.exe"; //gameManaging.PythonExecutable;

            PreprocessPatches(inputfolder, python_exe);
        }



        private async Task PreprocessPatches(string inputfolder, string python_exe)
        {
            // Call as lambda function
            // evt.previousValue is read-only and cannot be set. Remove this line.
            SendPythonProcessEvent(PythonWorkerEvent, true);
            var output = RunTask(() => Image2Bboxes(python_exe, inputfolder)); // Wait for the task to complete
            await output;

            string result = output.Result.ToString();

            // We are awaiting beyond the await output statement but Main thread is not blocked
            bbox_dict = JsonUtility.FromJson<DataFrame>(result);
            SendPythonProcessEvent(PythonWorkerEvent, false);
        }



        public Task<object> RunTask(Func<object> func)
        {
            var output = Task<object>.Run(() =>
            {
                return func();
            });
            return output;
        }

        public IEnumerator<object> BuildCoroutine(Action func)
        {
            // Use if no return is required !!!!
            Thread thread = new Thread(() => func());
            thread.Start();
            // The yield return null line is the point where 
            // execution pauses and resumes in the following frame
            while (thread.IsAlive)
            {
                yield return null;
            }
        }

        // Place all the positioning functions into their respective gameobjects to make this neat.!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        void PositionCanvas()
        {

            // Setup anchors and pivots
            rectTransform = GetComponent<RectTransform>();
            NonGOSripts.HelperFunctions.SetupAnchorsAndPivots(rectTransform);

            // Set anchor to the centre of the screen
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            // Set pivot to the centre of the screen
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Set local position to the centre of the screen at a distance of 10 units
            rectTransform.localPosition = NonGOSripts.HelperFunctions.FacePlayer(raycast_distance);

            // Set rotation of the Canvas to face the camera
            transform.rotation = Quaternion.Euler(Vector3.zero);

            // Set scale to 1
            transform.localScale = new Vector3(1, 1, 1);


        }

        public static string Image2Bboxes(string python_exe, string imagepath)
        {
            string processName = MethodBase.GetCurrentMethod().Name;
            string ScriptPath = Path.Combine(Application.streamingAssetsPath, "python_codes", "read_df.py");

            System.Diagnostics.Process process = PythonIPC.SetupPythonProcess(ScriptPath, python_exe, imagepath);

            // Start the process
            process.Start();

            string python_exe_new = PythonIPC.GetStdOutputFromConsole(process);

            return python_exe_new;

        }


        private void GetBBoxes(string data_dir)
        {
            isReady = false;

            try
            {
                // Create a new list to store the data
                data_dict = new List<element>();
                //read_csv_with_csharp(data_dir);

                // Read the CSV file with Python
                Task.Run(() => read_csv_with_python(data_dir)).Wait();
            }
            catch (Exception e)
            {
                SchapiroLabLog.Log($"An error occurred: {e.Message} with stack trace {e.StackTrace}");
            }

            finally
            {
                isReady = true;
            }


        }


        private void Initialize(Transform CurrentImage, Transform WholeImage, Transform Panel,
        Camera userCamera)
        {
            // Calculate the new position for the Canvas to minimum clipping distance
            transform.position = NonGOSripts.HelperFunctions.FacePlayer(raycast_distance);
            List<float> outputs = NonGOSripts.HelperFunctions.GetFOVatWD(raycast_distance, userCamera);
            rectTransform.sizeDelta = new Vector2(outputs[1], outputs[0]);

            /*             // Instanziate Exit Button
                        SetupSeperateButton(CurrentImage, transform); */

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
            public void SaveToCSVcsharp(string filePath)
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
                    SchapiroLabLog.Log($"No patches in the image for trash {script.gameObject.name}");
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


        private void read_csv_with_python(string data_dir)
        {
            // Path to the CSV file
            string csvFilePath = Path.Combine(data_dir, "bbox.txt");

            // Setup the Python process
            PythonIPC.GetStdOutputFromPython(General.HelperFunctions.AddQuotesIfRequired(Path.Combine(Application.streamingAssetsPath, PythonScript)),
             python_exe, General.HelperFunctions.AddQuotesIfRequired(csvFilePath));

        }

        /*
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
        */


    }

}