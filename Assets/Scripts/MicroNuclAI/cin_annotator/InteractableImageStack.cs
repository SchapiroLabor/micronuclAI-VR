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
using System.Diagnostics;
using UnityEditor.Rendering;

namespace CinAnnotator
{
    public class InteractableImageStack : MonoBehaviour
    {
        public Camera userCamera;  // Reference to the user's camera


        public int target_size = 60;


        [SerializeField] private GridMaker _gridMaker;
        [SerializeField] private GameManaging _gameManaging;
        [SerializeField] private WholeImage _wholeImage;
        [SerializeField] private ClickNextImage _clickNextImage;
        [SerializeField] private Trash _trash;
        [SerializeField] private SetupButtons _buttons;


        [Serializable]
        public class MyChangeEvent : UnityEvent<bool>
        {
            public bool value;
        }
        [SerializeField] public MyChangeEvent PythonWorkerEvent = new MyChangeEvent();
        [SerializeField] private GameObject load_indicator;
        public DataFrame bbox_dict;

        [Header("Add to config file")]
        private string python_exe; //gameManaging.PythonExecutable;
        [Header("Add to config file")]
        private string PythonScript;
        [Header("Add to config file")]
        public string inputfolder;
        [Header("Add to config file")]
        public string ImgPath;
        [Header("Add to config file")]
        public string ImgPNGPath;
        [Header("Add to config file")]
        private string MaskPath;
        [Header("Add to config file")]
        private string PythonConfigPath;

        public float raycast_distance = 10f; // Default distance to raycast from the camera, please do not change this !!

        [Header("Add to config file. Used to set world font size")]
        private float textfontsize = 0.5f; // Default font size for the text field




        void PythonProcessOnTrue()
        {
            // Create text field to state image is loading
            //CreateLoadingWidget(transform, load_indicator, textfontsize);
        }

        void PythonProcessOnFalse()
        {
            //load_indicator.SetActive(false);
   

                _wholeImage.Initialize(_gridMaker.transform, userCamera, ImgPNGPath);


            _clickNextImage.Initialize();
            _buttons.Initialize(_clickNextImage.transform, _clickNextImage.transform.rotation, _trash.transform);
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


            // TODO: Add the following to the game manager
            python_exe = @"D:\OneDrive\Desktop\Career\Internship\UniKlinikum\Schapiro\repos\micronuclAI-VR\Assets\venv\MNAIVR\Scripts\python.exe"; //gameManaging.PythonExecutable;
            PythonScript = "python_codes/MicroNuclAI/singlecellcropper.py";
            inputfolder = @"D:\OneDrive\Desktop\Career\Internship\UniKlinikum\Schapiro\data\data\";


            ImgPath = Path.Combine(inputfolder, "s01c1.ome.tif");
            ImgPNGPath = Path.Combine(inputfolder, "img.png");
            MaskPath = Path.Combine(inputfolder, "mask.tif");
            PythonConfigPath = Path.Combine(inputfolder, "config.json");



            ThreadWithState tws = new(python_exe, inputfolder, PythonScript, PythonConfigPath,
            MaskPath, ImgPath, this);

            StartCoroutine(PreprocessPatches(tws));






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


            public List<int> label_ids;
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

            public List<int> X1_downsampled;
            public List<int> X2_downsampled;
            public List<int> Y1_downsampled;
            public List<int> Y2_downsampled;



            public Rect GetBBOX(int df_index, bool downsampled = false, float canvasscalefactor = 1f)
            {
                if (df_index < 0 || df_index >= label_ids.Count)
                    throw new ArgumentOutOfRangeException(nameof(df_index), "Index is out of range.");

                if (downsampled)
                {   
                    float x1 = X1_downsampled[df_index] / canvasscalefactor;
                    float x2 = X2_downsampled[df_index] / canvasscalefactor;
                    float y1 = Y1_downsampled[df_index] / canvasscalefactor;
                    float y2 = Y2_downsampled[df_index] / canvasscalefactor;

                    return new Rect(x1, y1, x2 - x1, y2 - y1);
                }
                else
                {
                    float x1 = X1[df_index];
                    float x2 = X2[df_index];
                    float y1 = Y1[df_index];
                    float y2 = Y2[df_index];

                    return new Rect(x1, y1, x2 - x1, y2 - y1);
                }
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

            PositionCanvas();

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
            private string _MaskPath;
            private string _ImgPath;
            private string _PythonConfigPath;

            // The constructor obtains the state information.
            public ThreadWithState(string python_exe, string inputfolder,
            string python_script, string python_config_path, string mask_path, string img_path,
            InteractableImageStack _interactableImageStack)
            {
                _python_exe = python_exe;
                _inputfolder = inputfolder;
                _python_script = python_script;
                //_pythonWorkerEvent = PythonWorkerEvent;
                _InteractableImageStack = _interactableImageStack;
                _MaskPath = mask_path;
                _ImgPath = img_path;
                _PythonConfigPath = python_config_path;
            }

            // The thread procedure performs the task, such as formatting
            // and printing a document.
            public void ThreadProc()
            {

                string ScriptPath = Path.Combine(Application.streamingAssetsPath, _python_script);

                string cmd_args = $"--mask_path {_MaskPath} --img_path {_ImgPath} --save_dir {_inputfolder} " +
                                  $"--n {15} --target_a_ratio {1} " +
                                  $"--write-out-my-config {_PythonConfigPath}";

                System.Diagnostics.Process process = PythonIPC.SetupPythonProcess(ScriptPath, _python_exe, cmd_args);


                try
                {
                    // Start the process
                    process.Start();

                    string json_bbox_dict = PythonIPC.GetStdOutputFromConsole(process);

                    // We are awaiting beyond the await output statement but Main thread is not blocked
                    _InteractableImageStack.bbox_dict = JsonUtility.FromJson<DataFrame>(json_bbox_dict);

                    if (_InteractableImageStack.bbox_dict == null)
                    {
                        Debug.LogError("Failed to parse the JSON output from the Python script.");
                    }
                    else
                    {
                        Debug.Log($"Parsed DataFrame: {_InteractableImageStack.bbox_dict.label_ids.Count} entries found.");
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"An error occurred: {e.Message} with stack trace {e.StackTrace}");
                }
                finally
                {
                    // Ensure the process is disposed of properly
                    if (process != null)
                    {
                        process.Dispose();
                    }
                }







            }

            // Unity API is not thread safe, so cannot use it in worker thread. Use indirect variables to pass data
        }

        private void read_csv_with_python(string data_dir)
        {
            // TODO: Use this all fallback incase the Python process fails to start (Make sure that python script 
            // and this have same filename for the csv file)

            // Path to the CSV file
            string csvFilePath = Path.Combine(data_dir, "bbox.txt");

            // Setup the Python process
            PythonIPC.GetStdOutputFromPython(General.HelperFunctions.AddQuotesIfRequired(Path.Combine(Application.streamingAssetsPath,
            PythonScript)),
             python_exe, General.HelperFunctions.AddQuotesIfRequired(csvFilePath));

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

            Task t = new Task(() => tws.ThreadProc());
            t.Start();

            // Display a loading indicator while the thread is running

            // Setup anchors and pivots
            RectTransform rectTransform = load_indicator.GetComponent<RectTransform>();

            // Set anchor to the centre of the screen
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            // Set pivot to the centre of the screen
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Set local position to the centre of the screen at a distance of 10 units
            rectTransform.localPosition = new UnityEngine.Vector3(0, 0, raycast_distance);

            // Set rotation of the Canvas to face the camera
            transform.rotation = Quaternion.Euler(Vector3.zero);

            // Set scale to 1
            transform.localScale = new Vector3(1, 1, 1);

            while (!t.IsCompleted)
            {
                yield return null;
            }

            // Wait for the thread to finish
            t.Wait();

            // Delete the loading indicator after the thread has finished
            Destroy(load_indicator);

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


    }

}