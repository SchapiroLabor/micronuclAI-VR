

using System.Collections.Generic;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Unity.VisualScripting;

namespace General
{
    class HelperFunctions
    {
        static public void LogDiagnosticsProcess(Process proc, bool writeOut)
        {
            proc.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                using (StreamWriter stderrorWriter =
                        new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Output", "r_log.txt"), true))
                {
                    stderrorWriter.WriteLine("\n STDERROR: " + e.Data);
                }
            }
        });

            proc.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    if (writeOut)
                    {
                        using (StreamWriter stdoutWriter =
                                new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Output", "r_log.txt"), true))
                        {
                            stdoutWriter.WriteLine("\n STDOUT: " + e.Data);
                        }

                    }
                }
            });
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

    }



}