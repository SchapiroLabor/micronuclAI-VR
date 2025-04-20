using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

//  from https://github.com/sonejilab/cellexalvr
public static class SchapiroLabLog
{
    public static ConsoleManager consoleManager;

    private static string logDirectory;
    private static string logFilePath = "";
    private static int maxNrOfLogFiles = 5;
    public static string LogFilePath
    {
        get { return logFilePath; }
        private set { logFilePath = value; }
    }
    private static List<string> logThisLater = new List<string>();

    public static void InitNewLog()
    {
        // File names can't have colons so we only use hyphens
        var now = DateTime.Now;
        var time = now.ToString("yyyy-MM-dd-HH-mm-ss");

        logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Output", "Logs");
        if (!Directory.Exists(logDirectory))
        {
            logThisLater.Add("\tCreated directory " + logDirectory);
            Directory.CreateDirectory(logDirectory);
        }

        // To clean up some older log files.
        string[] files = Directory.GetFiles(logDirectory);
        int nrOfFilesToDelete = files.Length - maxNrOfLogFiles;
        for (int i = 0; i < nrOfFilesToDelete; i++)
        {
            File.Delete(files[i]);
        }

        LogFilePath = Path.Combine(logDirectory, "cellexal-log-" + time + ".txt");
        // this will most likely always happen
        if (!File.Exists(LogFilePath))
        {
            logThisLater.Add("\tCreated file " + LogFilePath);
            File.Create(LogFilePath).Dispose();
        }

        string nicerTime = now.ToString("yyyy-MM-dd HH:mm:ss");
        Log("Welcome to CellexalVR " + Application.version,
            "Running on Unity " + Application.unityVersion,
            "BuildGUID: " + Application.buildGUID,
            "Logfile created at " + nicerTime);

        Log("\nSome system information:",
            "\tOS: " + SystemInfo.operatingSystem,
            "\tCPU: " + SystemInfo.processorType,
            "\tProcessor count: " + SystemInfo.processorCount,
            "\tGPU: " + SystemInfo.graphicsDeviceName,
            "\tRAM size: " + SystemInfo.systemMemorySize);

        if (logThisLater.Count > 0)
        {
            Log("The following was generated before the log file existed:");
            LogBacklog();
            Log("End of what was generated before the log file existed.");
        }


    }

    /// <summary>
    /// Writes everything that has been accumulated to the log file.
    /// </summary>
    public static void LogBacklog()
    {
        if (logFilePath == "")
        {
            return;
        }
        using (StreamWriter logWriter = new StreamWriter(new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.None)))
        {
            foreach (string s in logThisLater)
            {
                logWriter.WriteLine(s);
            }
            logWriter.Flush();
        }
        logThisLater.Clear();
    }



    /// <summary>
    /// Writes to the log. This method will append a linebreak at the end of the written line.
    /// </summary>
    /// <param name="message"> The string that should be written to the log. </param>
    public static void Log(string message)
    {
        if (consoleManager)
        {
            consoleManager.AppendOutput(message);
        }
        if (logFilePath == "")
        {
            logThisLater.Add(message);
        }
        else
        {
            using (StreamWriter logWriter = new StreamWriter(new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.None)))
            {
                logWriter.WriteLine(message);
                logWriter.Flush();
            }
        }
    }

    /// <summary>
    /// Logs multiple messages. This method will append a linebreak between each message.
    /// </summary>
    /// <param name="message"> The messages that should be written to the log. </param>
    public static void Log(params string[] message)
    {
        foreach (string s in message)
        {
            Log(s);
        }
    }

    /// <summary>
    /// Saves someting that should be logged later. Call <see cref="LogBacklog"/> whenever you are ready to log everything passed to all calls this function since the last call to <see cref="LogBacklog"/>.
    /// </summary>
    /// <param name="message">The message that should be written to the log later.</param>
    public static void LogLater(string message)
    {
        logThisLater.Add(message);
    }

    /// <summary>
    /// Replaces all forward and backward slashes with whatever is the correct directory seperator character on this system.
    /// </summary>
    /// <param name="path"> A file path with a weird mix of forward and backward slashes. </param>
    /// <returns> A file path without a weird mix of forward and backward slashes. </returns>
    public static string FixFilePath(string path)
    {
        char directorySeparatorChar = Path.DirectorySeparatorChar;
        path = path.Replace('/', directorySeparatorChar);
        path = path.Replace('\\', directorySeparatorChar);
        return path;
    }

    /// <summary>
    /// Closes the old log and opens a new log.
    /// </summary>
}




/// <summary>
/// The console for executing commands from the desktop.
/// </summary>
public class ConsoleManager : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public GameObject consoleGameObject;
    public TMPro.TMP_InputField inputField;
    public TMPro.TMP_InputField outputField;
    public TMPro.TMP_Text suggestionField;
    private bool consoleActive = false;
    private Dictionary<MethodInfo, string> accessors = new Dictionary<MethodInfo, string>();
    private Dictionary<string, MethodInfo> commands = new Dictionary<string, MethodInfo>();
    private Dictionary<string, string> folders = new Dictionary<string, string>();
    private Dictionary<MethodInfo, List<string>> aliases = new Dictionary<MethodInfo, List<string>>();

    private LinkedList<string> history = new LinkedList<string>();

    private int currentNumberOfLines = 0;
    private string outputBufferString = "";

    private void Start()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods())
                {
                    var attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
                    if (attribute != null)
                    {
                        accessors[method] = attribute.Access;
                        foreach (string alias in attribute.Aliases)
                        {
                            commands[alias] = method;
                            if (!aliases.ContainsKey(method))
                            {
                                aliases[method] = new List<string>();
                            }
                            aliases[method].Add(alias);
                            folders[alias] = attribute.Folder;
                        }
                    }
                }
            }
        }

        history.AddFirst("");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            consoleActive = !consoleActive;
            if (consoleActive)
            {
                consoleGameObject.SetActive(true);
                inputField.ActivateInputField();
            }
            else
            {
                inputField.DeactivateInputField();
                consoleGameObject.SetActive(false);
            }
        }

        if (!consoleActive)
            return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            EnterCommand(inputField.text);
        }
    }

    public void AppendOutput(string output)
    {
        outputBufferString = outputBufferString + "\n" + output;
        int nbrOfNewLines = output.Count((c) => c == '\n') + 1;
        currentNumberOfLines += nbrOfNewLines;

        int maxBufferLines = 1999;
        if (currentNumberOfLines > maxBufferLines)
        {
            int nbrOfExcessLines = currentNumberOfLines - maxBufferLines;
            int lineBreakIndex = 0;
            for (int i = 0; i < nbrOfExcessLines; ++i)
            {
                while (outputBufferString[lineBreakIndex] != '\n')
                {
                    lineBreakIndex++;
                }
                lineBreakIndex++;
            }
            outputBufferString = outputBufferString.Remove(0, lineBreakIndex);
            currentNumberOfLines -= nbrOfExcessLines;
        }

        outputField.text = outputBufferString;
        outputField.MoveTextEnd(false);
        outputField.textComponent.ForceMeshUpdate();

        ClearAndHideSuggestions();
    }

    public void EnterCommand(string command)
    {
        AppendOutput("> " + command);
        inputField.text = "";

        inputField.ActivateInputField();
        inputField.Select();

        if (command == "")
        {
            return;
        }

        history.First.Value = command;
        history.AddFirst("");

        string[] words = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (!commands.ContainsKey(words[0]))
        {
            AppendOutput("Command not found. Type 'listall' for all defined commands.");
            return;
        }

        MethodInfo method = commands[words[0]];
        string accessFieldName = accessors[method];

        var access = ReferenceManager.instance.GetType().GetField(accessFieldName).GetValue(referenceManager);

        ParameterInfo[] parameterInfo = method.GetParameters();
        if (words.Length - 1 != parameterInfo.Length)
        {
            AppendOutput(string.Format("Wrong number of parameters. The command {0} has the parameters {1}", words[0], ParameterInfosToString(parameterInfo)));
            return;
        }

        object[] parameters = new object[words.Length - 1];
        for (int i = 1; i < words.Length; ++i)
        {
            try
            {
                parameters[i - 1] = ParseArgument(words[i], parameterInfo[i - 1].ParameterType);
            }
            catch (ArgumentException e)
            {
                AppendOutput(e.Message);
                return;
            }
            catch (FormatException)
            {
                AppendOutput(string.Format("The paramater {0} could not be parsed as type {1}", words[i], parameterInfo[i - 1].ParameterType));
                return;
            }
        }

        method.Invoke(access, parameters);
    }

    private object ParseArgument(string arg, Type t)
    {
        if (t == typeof(int))
        {
            return int.Parse(arg);
        }
        else if (t == typeof(float))
        {
            return float.Parse(arg);
        }
        else if (t == typeof(double))
        {
            return double.Parse(arg);
        }
        else if (t == typeof(bool))
        {
            return arg != "0";
        }
        else if (t == typeof(string))
        {
            return arg;
        }
        else
        {
            throw new ArgumentException("ERROR: Argument was not a known type that could be parsed");
        }
    }

    private string ParameterInfosToString(ParameterInfo[] info)
    {
        if (info.Length == 0)
        {
            return "Command has no arguments";
        }
        StringBuilder sb = new StringBuilder();
        sb.Append(info[0].ParameterType).Append(" ").Append(info[0].Name);
        for (int i = 1; i < info.Length; ++i)
        {
            sb.Append(", ").Append(info[i].ParameterType).Append(" ").Append(info[i].Name);
        }
        return sb.ToString();
    }

    public void ClearAndHideSuggestions()
    {
        suggestionField.text = "";
        suggestionField.transform.parent.gameObject.SetActive(false);
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class ConsoleCommandAttribute : Attribute
{
    public string Access { get; private set; }
    public string Folder { get; private set; }
    public string[] Aliases { get; private set; }

    public ConsoleCommandAttribute(string access, string folder = "", params string[] aliases)
    {
        Access = access;
        Folder = folder;
        Aliases = aliases;
    }
}
