using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public class RunCompiler : Microsoft.Build.Utilities.Task
{
    [Required]
    public string OutputDirectory { get; set; }
    [Required]
    public string CompilerPath { get; set; }
    [Required]
    public ITaskItem[] InputFiles { get; set; }

    [Output]
    public ITaskItem[] Output { get; private set; }
    [Output]
    public ITaskItem[] Errors { get; private set; }

    public override bool Execute()
    {
        try
        {
            var argumentsList = new List<string>(){
                "-NoProfile",
                "-ExecutionPolicy",
                "Bypass",
                "-File",
                $"{CompilerPath}",
                "-outDir",
                $"{OutputDirectory}"
            };

            if (InputFiles != null)
            {
                foreach (var inputFile in InputFiles)
                {
                    argumentsList.Add(inputFile.ItemSpec);
                }
            }
            var processStartInfo = new ProcessStartInfo("pwsh", argumentsList)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };


            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                process.WaitForExit();
                var outputLines = new List<ITaskItem>();

                while (!process.StandardOutput.EndOfStream)
                {
                    string? line = process.StandardOutput.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        outputLines.Add(new TaskItem(line));
                    }
                }
                Output = outputLines.ToArray();

                var errorLines = new List<ITaskItem>();
                while (!process.StandardError.EndOfStream)
                {
                    string? line = process.StandardError.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        errorLines.Add(new TaskItem(line));
                    }
                }

                Errors = errorLines.ToArray();
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"Error executing compiler.ps1: {ex.Message}");
            return false;
        }
    }
}
