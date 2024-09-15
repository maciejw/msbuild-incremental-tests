using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text;
using System.Management.Automation;

public class RunCompiler : PowerShellCommandTask
{
    [Required]
    public string OutputDirectory { get; set; }
    [Required]
    public string CompilerPath { get; set; }
    [Required]
    public ITaskItem[] SourceFiles { get; set; }
    [Required]
    public ITaskItem[] IncludeFiles { get; set; }

    protected override void BindParameters(PowerShell ps)
    {
        ps.AddCommand(CompilerPath)
            .AddParameter("OutputDirectory", OutputDirectory)
            .AddParameter("SourceFiles", SourceFiles.Select(x => x.ItemSpec))
            .AddParameter("IncludeFiles", IncludeFiles.Select(x => x.ItemSpec));
    }
}
