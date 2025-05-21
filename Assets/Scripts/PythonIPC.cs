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
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.UIElements;
using System.Collections.Concurrent;


// If admin rights are needed
// https://stackoverflow.com/questions/58250962/making-processes-inside-c-sharp-to-run-as-administrator
// https://learn.microsoft.com/en-us/previous-versions/bb756929(v=msdn.10)#how-to-create-an-embedded-manifest-with-microsoft-visual-studio

namespace General
{

    static class PythonIPC
    {
        // Use this code for windows to get python path: get-command python | ForEach-Object -Process {write-host $_.Definition}


        public static string InitPython(string python_exe)
        {
            string ScriptPath = Path.Combine(Application.streamingAssetsPath, "python_codes", "init_python.py");

            Debug.Log($"Initialising Python env with: {python_exe} with arguments: {ScriptPath}");
            // Create a new process to run the Python script

            string venv_dir = Path.Combine(Application.dataPath, "venv");

            Debug.Log($"Python venv dir: {venv_dir}");


            System.Diagnostics.Process process = SetupPythonProcess(ScriptPath, python_exe, venv_dir);

            // Start the process
            process.Start();

            process.StandardInput.Write(python_exe);

            string python_exe_new = GetStdOutputFromConsole(process);

            return python_exe_new;

        }


        static void PythonProcessEndCallback(ChangeEvent<bool> evt, GameObject textField, string processName)
        {
            if (evt.newValue == false)
            {
                Debug.Log($"Process {processName} ended successfully.");
                // Get the text field to update
                textField.GetComponent<TMP_Text>().text = $"";
                textField.SetActive(true);
            }
            else
            {
                Debug.Log($"Process {processName} failed to start.");
            }

        }

        public static System.Diagnostics.Process SetupPythonProcess(string ScriptPath, string python_exe,
        string argument = null)
        {
            Debug.Log($"Running Python script: {ScriptPath} with arguments: {argument}");

            // Create a new process to run the Python script    
            return new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = python_exe,
                    Arguments = $"{ScriptPath} {argument}",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true, // Redirect the standard error to capture it
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
        }

        public static string GetStdOutputFromConsole(Process process)
        {
            // Read the output from the Python script
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            // Wait for the process to finish
            process.WaitForExit();
            process.Close();

            if (!string.IsNullOrEmpty(error))
            {
                Debug.Log($"Error from Python script: {error}");

            }

            return output;
        }

        public static string GetStdOutputFromPython(string ScriptPath, string python_exe,
        string argument = null)
        {

            // Create a new process to run the Python script
            System.Diagnostics.Process process = SetupPythonProcess(ScriptPath, python_exe, argument);

            // Start the process
            process.Start();

            string output = GetStdOutputFromConsole(process);

            return output;

        }

        public static void Write2Python_type1(string ScriptName, string python_exe,
        string workingdirectory, string data_dir, string message)
        {

            System.IO.File.WriteAllText(System.IO.Path.Combine(data_dir, "results", "message.txt"), message);
            SchapiroLabLog.Log($"Message written to {System.IO.Path.Combine(data_dir, "message.txt")}");
            try
            {
                string argmuents = HelperFunctions.AddQuotesIfRequired(workingdirectory) + " " + HelperFunctions.AddQuotesIfRequired(data_dir);

                System.Diagnostics.Process process = SetupPythonProcess(HelperFunctions.AddQuotesIfRequired(Path.Combine(workingdirectory, ScriptName)), python_exe,
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
                {
                    SchapiroLabLog.Log($"Error from Python script: {error}");

                }

            }
            catch (Exception e)
            {
                SchapiroLabLog.Log($"An error occurred: {e.Message} with stack trace {e.StackTrace}");
            }



        }



        public static void ThreadPooling(Delegate method, bool isReady, Action finalAction = null, params object[] args)

        {      // Params array in conjunction with a Delegate to pass a dynamic number of arguments to your method
               // Params keyword allows the method to accept a variable number of arguments
               // The method.DynamicInvoke(args) call dynamically invokes the delegate with the provided arguments. 
               // This makes it possible to pass any number of arguments, as long as they match the delegate's signature.

            SchapiroLabLog.Log("Function invoked successfully");
            // Execute on a second thread
            System.Threading.ThreadPool.QueueUserWorkItem((state) =>
            {
                try
                { // Ensures that any exceptions thrown by method(args) are caught and handled properly within the thread
                    SchapiroLabLog.Log("Function started successfully");
                    method.DynamicInvoke(args);
                    SchapiroLabLog.Log("Function ended successfully");



                }

                catch (Exception e)
                { // Catch any exceptions thrown by method(args) and log them
                    SchapiroLabLog.Log($"An error occurred: {e.Message} with stack trace {e.StackTrace}");
                }
                finally
                { // Will execute regardless of whether an exception is thrown, 
                  //ensuring that cleanup actions like setting Ready2Exit = true and logging the exit message are always performed. 
                  //This is especially important for maintaining the application's state and ensuring resources are cleaned up correctly.

                    if (finalAction != null)
                    { finalAction?.Invoke(); }
                    isReady = true;
                }
            });




        }


        /*         public static void Write2Python(string ScriptPath, string python_exe, string message)
                {

                    // Throws a pipe is broken exception. Not sure why that is. 

                    // Event to signal when the client is ready
                    // ManualResetEvent clientReady = new ManualResetEvent(false);

                    using (AnonymousPipeServerStream pipeServer =
                        new AnonymousPipeServerStream(PipeDirection.Out,
                        HandleInheritability.Inheritable))
                    {
                        SchapiroLabLog.Log($"[SERVER] Current TransmissionMode: {pipeServer.TransmissionMode}.");

                        // Pass the client process a handle to the server and execute it
                        System.Diagnostics.Process pipeClient = SetupPythonProcess(ScriptPath, python_exe, pipeServer.GetClientHandleAsString());

                        SchapiroLabLog.Log($"[SERVER] Client handle: {pipeServer.GetClientHandleAsString()}");
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
                            SchapiroLabLog.Log($"[SERVER] Error: {e.Message}");
                        }

                        // Read the output and error from the Python script
                        string output = pipeClient.StandardOutput.ReadToEnd();
                        string error = pipeClient.StandardError.ReadToEnd();

                        pipeClient.WaitForExit();
                        pipeClient.Close();

                        // Print the output and error from the Python script
                        SchapiroLabLog.Log(output);
                        Debug.LogError(error);

                    }


                } */
    }

    // Declare classes related to Event functions

    /* From source: https://gamedevbeginner.com/events-and-delegates-in-unity/#event_based_systems
    
        While Unity Events can be a great way to manage relationships between local scripts and components,
         you probably won’t want to connect two remote objects in this way.

    Hooking up scripts in the Inspector requires you to make a manual connection which may not work well for different objects in the scene, 
    especially if they’re created as the game runs.

    Meaning that, when connecting events between unrelated objects, you may find it more useful to use event delegates instead.

    By using scriptable objects to create a common event variable, two unrelated game objects can react to the same 
    event without needing to know about each other.
    Which allows you to create Unity Event style functionality, but between any objects in the scene.
    */


}