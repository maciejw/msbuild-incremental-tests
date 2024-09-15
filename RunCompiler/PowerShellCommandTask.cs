using Microsoft.Build.Framework;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Management.Automation.Host;

public abstract class PowerShellCommandTask : Microsoft.Build.Utilities.Task
{
    public override bool Execute()
    {
        InitialSessionState initialSessionState = InitialSessionState.CreateDefault();

        initialSessionState.ExecutionPolicy = ExecutionPolicy.Bypass;

        using (var ps = PowerShell.Create(initialSessionState))
        {
            ps.Runspace.SessionStateProxy.SetVariable("VerbosePreference", "Continue");

            Log.LogMessage(MessageImportance.Low, "Command Excuted");

            ps.Streams.Information.DataAdded += (sender, e) =>
            {
                var record = ps.Streams.Information[e.Index];
                Log.LogMessage(MessageImportance.Normal, record.MessageData.ToString());
            };
            ps.Streams.Verbose.DataAdded += (sender, e) =>
            {
                var record = ps.Streams.Verbose[e.Index];
                Log.LogMessage(MessageImportance.Low, record.Message);
            };
            ps.Streams.Warning.DataAdded += (sender, e) =>
            {
                var record = ps.Streams.Warning[e.Index];
                Log.LogWarning(record.Message);
            };

            BindParameters(ps);

            var results = ps.Invoke();

            ProcessOutput(results);

            if (ps.Streams.Error.Count > 0)
            {
                ProcessErrors(ps.Streams.Error);
                return false;
            }
            return true;
        }
    }

    protected virtual void ProcessErrors(ICollection<ErrorRecord> errors)
    {
        foreach (var error in errors)
        {
            BuildEngine.LogErrorEvent(new BuildErrorEventArgs(
                subcategory: "",
                code: error.FullyQualifiedErrorId,
                file: "",
                lineNumber: 0,
                columnNumber: 0,
                endLineNumber: 0,
                endColumnNumber: 0,
                message: error.ToString(),
                helpKeyword: "",
                senderName: GetType().Name
            ));
        }
    }

    protected virtual void ProcessOutput(ICollection<PSObject> outputs)
    {
        foreach (var output in outputs)
        {
            if (output.BaseObject is string)
            {
                Log.LogMessage(MessageImportance.Normal, output.BaseObject.ToString());
            }
        }
    }

    protected virtual void BindParameters(PowerShell ps) { }
}
