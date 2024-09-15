using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text;
using System.Management.Automation;
using Json.More;

public class RunAnalyzer : PowerShellCommandTask
{
    [Required]
    public string AnalyzerPath { get; set; }
    [Required]
    public ITaskItem[] SourceFiles { get; set; }
    [Required]
    public ITaskItem[] IncludeFiles { get; set; }

    [Output]
    public ITaskItem[] SourceFilesWithDependents { get; set; }


    protected override void BindParameters(PowerShell ps)
    {
        ps.AddCommand(AnalyzerPath)
            .AddParameter("SourceFiles", SourceFiles.Select(x => x.ItemSpec))
            .AddParameter("IncludeFiles", IncludeFiles.Select(x => x.ItemSpec))
            .AddParameter("Verbose");
    }
    protected override void ProcessOutput(ICollection<PSObject> outputs)
    {
        try
        {
            List<ITaskItem> sourceFilesWithDependents = [];
            Log.LogMessage(MessageImportance.Normal, $"Got {outputs.Count} outputs");

            foreach (var output in outputs)
            {
                if (output.BaseObject is PSCustomObject)
                {
                    var source = output.Properties["Source"];
                    var sourceDependency = output.Properties["SourceDependency"];
                    if (source != null && source.Value is string)
                    {
                        if (sourceDependency != null && sourceDependency.Value is string)
                        {
                            sourceFilesWithDependents.Add(new TaskItem($"{source.Value}", new Dictionary<string, string>
                            {
                                ["SourceDependency"] = $"{sourceDependency.Value}"
                            }));
                        }
                        else
                        {
                            sourceFilesWithDependents.Add(new TaskItem($"{source.Value}"));
                        }
                    }
                    else
                    {
                        Log.LogWarning($"Unexpected output object: {output}");
                    }
                }
                else
                {
                    Log.LogWarning($"Unexpected output type: {output.BaseObject.GetType()}");
                }
            }

            Log.LogMessage(MessageImportance.Normal, $"Found {sourceFilesWithDependents.Count} include files with dependents");
            SourceFilesWithDependents = sourceFilesWithDependents.ToArray();
        }
        catch (System.Exception ex)
        {
            Log.LogErrorFromException(ex, true);
        }
    }
}
