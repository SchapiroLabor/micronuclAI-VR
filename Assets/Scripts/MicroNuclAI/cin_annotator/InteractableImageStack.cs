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
using UnityEngine.Events;
using Unity.XR.CoreUtils;
using static NonGOSripts.HelperFunctions;
using Unity.Tutorials.Core.Editor;
using System.Runtime.InteropServices.WindowsRuntime;
using Palmmedia.ReportGenerator.Core;
using System.Collections;
using UnityEngine.UI;

namespace CinAnnotator
{
    public class InteractableImageStack : MonoBehaviour
    {
        public Camera userCamera;  // Reference to the user's camera
        public string ImgPath = @"D:\OneDrive\Desktop\Internship\VR_schapiro\data\data\s01c1.ome.tif";
        public string MaskPath = @"D:\OneDrive\Desktop\Internship\VR_schapiro\data\data\mask.tif";


        [SerializeField] private GridMaker _gridMaker;
        [SerializeField] private GameManaging _gameManaging;
        [SerializeField] private WholeImage _wholeImage;
        [Serializable]
        public class MyChangeEvent : UnityEvent<bool>
        {
            public bool value;
        }
        [SerializeField] public MyChangeEvent PythonWorkerEvent = new MyChangeEvent();
        [SerializeField] GameObject load_indicator;
        public DataFrame bbox_dict;

        [Header("Add to config file. Used to set world font size")]
        private float textfontsize = 0.5f; // Default font size for the text field
        [Header("Add to config file")]
        private string PythonScript = "python_codes/MicroNuclAI/singlecellcropper.py";
        [Header("Add to config file")]
        public string inputfolder;
        [Header("Add to config file")]
        private string python_exe;
        [Header("Add to config file")]
        public float raycast_distance = 10f; // Default distance to raycast from the camera, please do not change this !!

        private bool done = false;




        public class myjson_element
        {   // X, Y = Width, Height
            // ["N", "X1", "X2", "Y1", "Y2"]

            public int N { get; set; }
            public int X1 { get; set; }
            public int X2 { get; set; }
            public int Y1 { get; set; }
            public int Y2 { get; set; }
            public int[] whole_slide_img_shape { get; set; }

        }

        void Awake()
        {
            // Is played before start and 

            // Load the Game Manager
            // Why do we need to load the GameManaging scriptable object here?
            if (_gameManaging == null)
            {
                // Load from path
                _gameManaging = Resources.Load<GameObject>("Assets/Scripts/MicroNuclAI/SceneManager.prefab").GetComponent<GameManaging>();
            }

            // TODO: First pop up loading screen to 
            // indicate image loading and processing in python
            // Then load other gameobjects etc.
            //PythonWorkerEvent.AddListener(delegate { PythonProcessStartCallback(); });


            // Position Canvas once it is enabled
            //PositionCanvas();

            // Get the input folder and python executable
            //inputfolder = gameManaging.InputFolder;
            //PythonToggle = new Toggle("Test Toggle");
            //PythonToggle.name = "Python Toggle";

            //PythonToggle.RegisterValueChangedCallback(PythonProcessStartCallback);


            /* GetBBoxes(inputfolder); */

        }


        public class Container
        {   // ahsonkhan on May 17, 2019

            public int[][] Matrix { get; set; }
        }


        [Serializable]
        public class DataFrame
        {   // Get the BBOX, index and image path


            public List<int> Index;
            public List<int> X1;
            public List<int> X2;
            public List<int> Y1;
            public List<int> Y2;
            public List<string> Image_path;
            public List<int> whole_slide_img_ndim;
            public List<int> whole_slide_img_shape_Y;
            public List<int> whole_slide_img_shape_X;
            public List<int> whole_slide_img_shape_C;
            public List<int> whole_slide_img_shape_Z;
            public List<int> whole_slide_img_shape_T;


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

            PositionCanvas();

            inputfolder = @"D:\OneDrive\Desktop\Internship\VR_schapiro\data\data";
            python_exe = @"D:\OneDrive\Desktop\Internship\VR_schapiro\repos\micronuclAI-VR\Assets\venv\MNAIVR\Scripts\python.exe"; //gameManaging.PythonExecutable;

            ThreadWithState tws = new(python_exe, inputfolder, PythonScript,
            this);

            StartCoroutine(PreprocessPatches(tws));

        }

        void PositionCanvas()
        {

            // Setup anchors and pivots
            RectTransform rectTransform = GetComponent<RectTransform>();

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

        void PythonProcessOnTrue()
        {
            // Create text field to state image is loading
            //CreateLoadingWidget(transform, load_indicator, textfontsize);
        }

        void PythonProcessOnFalse()
        {
            //load_indicator.SetActive(false);
            _wholeImage.Initialize(_gridMaker.transform, userCamera);
        }


        void PythonProcessStartCallback()
        { // Added as callback in the Editor. For some reason cannot be added in script.

            if (PythonWorkerEvent.value == true)
            {
                // Start the Python process here
                // Create text field to state image is loading
                PythonProcessOnTrue();
                // Get the text field to update textField.GetComponent<TMP_Text>().text = $"Process {"processName"} started";

            }
            else if (PythonWorkerEvent.value == false)
            {
                // Stop the Python process here
                // Create text field to state image is loading
                PythonProcessOnFalse();
                // Get the text field to update textField.GetComponent<TMP_Text>().text = $"Process {"processName"} finished";
            }
            else
            {
                Debug.Log($"Process failed to start.");
            }
        }



        /*
                private async Task PreprocessPatches(string inputfolder, string python_exe, string python_script = "read_df.py")
                {
                    // Call as lambda function
                    // evt.previousValue is read-only and cannot be set. Remove this line.
                    SendPythonProcessEvent(PythonWorkerEvent, true);
                    var output = RunTask(() => Image2bbox_dict(python_exe, inputfolder, python_script)); // Wait for the task to complete
                    await output;

                    string result = output.Result.ToString();

                    // We are awaiting beyond the await output statement but Main thread is not blocked
                    //bbox_dict = JsonUtility.FromJson<DataFrame>(result);
                    ThreadSafeLogger.Log($"Python script output: {result}");
                    SendPythonProcessEvent(PythonWorkerEvent, false);
                }
        */
        public class ThreadWithState
        {
            // State information used in the task.
            private string _python_exe;
            private string _inputfolder;
            private string _python_script;
            public DataFrame output;
            public InteractableImageStack _InteractableImageStack;

            // The constructor obtains the state information.
            public ThreadWithState(string python_exe, string inputfolder, string python_script,
            InteractableImageStack _interactableImageStack)
            {
                _python_exe = python_exe;
                _inputfolder = inputfolder;
                _python_script = python_script;
                //_pythonWorkerEvent = PythonWorkerEvent;
                _InteractableImageStack = _interactableImageStack;
            }

            // The thread procedure performs the task, such as formatting
            // and printing a document.
            public void ThreadProc()
            {


                string ScriptPath = Path.Combine(Application.streamingAssetsPath, _python_script);

                string _MaskPath = @"D:\OneDrive\Desktop\Internship\VR_schapiro\data\data\mask.tif";
                string ImgPath = @"D:\OneDrive\Desktop\Internship\VR_schapiro\data\data\s01c1.ome.tif";
                Debug.Log($"Here: {_MaskPath}");
                string cmd_args = $"--mask_path {_MaskPath} --img_path {ImgPath} --save_dir {_inputfolder} " +
                                  $"--n {1} --max_side {250} --target_size {250} --target_a_ratio {1} " +
                                  $"--write-out-my-config {Path.Combine(_inputfolder, "python_config.json")}";

                System.Diagnostics.Process process = PythonIPC.SetupPythonProcess(ScriptPath, _python_exe, cmd_args);

                // Start the process
                process.Start();

                string json_bbox_dict = PythonIPC.GetStdOutputFromConsole(process);

                // We are awaiting beyond the await output statement but Main thread is not blocked
                _InteractableImageStack.bbox_dict = JsonUtility.FromJson<DataFrame>(json_bbox_dict);





            }

            // Unity API is not thread safe, so cannot use it in worker thread. Use indirect variables to pass data
        }



        private void SendPythonProcessEvent(MyChangeEvent PythonWorkerEvent, bool Value)
        {
            // Set the value of the event and invoke it
            PythonWorkerEvent.value = Value;
            PythonWorkerEvent.Invoke(Value); // Why add it to the invoke function ?
        }

        private IEnumerator PreprocessPatches(ThreadWithState tws)
        {
            // Call as lambda function
            // evt.previousValue is read-only and cannot be set. Remove this line.

            // Create a thread to execute the task, and then
            // start the thread.

            SendPythonProcessEvent(PythonWorkerEvent, true);

            Thread t = new(new ThreadStart(tws.ThreadProc));
            t.Start();

            while (t.IsAlive)
            {
                yield return null;
            }

            SendPythonProcessEvent(PythonWorkerEvent, false);

        }

        /*
            public string Image2bbox_dict()
            {
                SendPythonProcessEvent(PythonWorkerEvent, true);

                string ScriptPath = Path.Combine(Application.streamingAssetsPath, python_script);

                System.Diagnostics.Process process = PythonIPC.SetupPythonProcess(ScriptPath, python_exe, inputfolder);

                // Start the process
                process.Start();

                string json_bbox_dict = PythonIPC.GetStdOutputFromConsole(process);

                string result = json_bbox_dict.ToString();

                // We are awaiting beyond the await output statement but Main thread is not blocked
                //bbox_dict = JsonUtility.FromJson<DataFrame>(result);
                ThreadSafeLogger.Log($"Python script output: {result}");

                SendPythonProcessEvent(PythonWorkerEvent, false);

                return json_bbox_dict;

            }


            private void SendPythonProcessEvent(MyChangeEvent PythonWorkerEvent, bool Value)
            {
                // Set the value of the event and invoke it
                PythonWorkerEvent.value = Value;
                PythonWorkerEvent.Invoke(Value); // Why add it to the invoke function ?
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

                */

        // Place all the positioning functions into their respective gameobjects to make this neat.!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!



        private void GetBBoxes(string data_dir)
        {
            // TODO: Add indicator to state loading is done
            try
            {

                // Read the CSV file with Python
                Task.Run(() => read_csv_with_python(data_dir)).Wait();
            }
            catch (Exception e)
            {
                SchapiroLabLog.Log($"An error occurred: {e.Message} with stack trace {e.StackTrace}");
            }

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
            Transform Trash = _gridMaker.transform.GetChild(0);

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