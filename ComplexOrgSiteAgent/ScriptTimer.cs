using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.IO;
using System.Diagnostics;
using ADSync.Common.Events;
using System.Collections.ObjectModel;

namespace ComplexOrgSiteAgent
{
    public class ScriptTimer : IDisposable
    {
        #region Events
        public event EventHandler<StatusEvent> StatusUpdate;
        protected void OnStatusUpdate(StatusEvent e)
        {
            StatusUpdate?.Invoke(this, e);
        }

        public event EventHandler<ErrorEvent> ErrorEvent;
        protected void OnErrorEvent(ErrorEvent e)
        {
            ErrorEvent?.Invoke(this, e);
        }
        #endregion

        private Timer _timer;
        private int FrequencyMinutes { get; set; }
        private ScriptObject[] _scripts;

        public ScriptTimer(ScriptObject[] Scripts, int FrequencyMinutes)
        {
            _scripts = Scripts;
            var ms = FrequencyMinutes * 60 * 1000;
            _timer = new Timer(ms);
            _timer.Elapsed += FireScripts;
        }

        private void FireScripts(object sender, ElapsedEventArgs e)
        {
            Collection<PSObject> res;
            string scriptName;
            foreach(var script in _scripts)
            {
                scriptName = Path.GetFileNameWithoutExtension(script.ScriptPath);
                try
                {
                    res = RunScript(script);
                    if (res == null)
                    {
                        OnStatusUpdate(new StatusEvent("Previous iteration of script \"{0}\" still running, loop aborted.", scriptName));
                        return;
                    }
                    var err = res.FirstOrDefault(s => (s!=null && s.BaseObject.ToString().StartsWith("Exception:")));
                    if (err != null)
                    {
                        var msg = string.Format("Error running script \"{0}\":\n{1}", scriptName, err.BaseObject.ToString());
                        OnErrorEvent(new ErrorEvent(msg, EventLogEntryType.Error, 300, null));
                    }
                    else
                    {
                        OnStatusUpdate(new StatusEvent("Completed script command \"{0}\"", scriptName));
                    }
                }
                catch (Exception ex)
                {
                    var msg = string.Format("Error running script \"{0}\", see the event log for more details.", scriptName);
                    OnErrorEvent(new ErrorEvent(msg, EventLogEntryType.Error, 400, ex));
                }
            }
        }

        public void Start()
        {
            FireScripts(this, null);
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public static Collection<PSObject> RunScript(ScriptObject script)
        {
            if (script.IsRunning) return null;

            var res = new Collection<PSObject>();

            script.IsRunning = true;

            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            
            using (Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration))
            {
                runspace.Open();

                var directory = Path.GetDirectoryName(script.ScriptPath);
                runspace.SessionStateProxy.Path.SetLocation(directory);

                using (Pipeline pipeline = runspace.CreatePipeline())
                {
                    Command cmd = new Command(script.ScriptPath, false, false);
                   
                    if (script.ScriptArgs != null)
                    {
                        foreach (var arg in script.ScriptArgs)
                        {
                            CommandParameter testParam = new CommandParameter(arg.Key, arg.Value);
                            cmd.Parameters.Add(testParam);
                        }
                    }

                    pipeline.Commands.Add(cmd);

                    try
                    {
                        res = pipeline.Invoke();
                    }
                    catch (ParseException ex)
                    {
                        var msg = string.Format("Exception: ParseException: {0}\n{1}", ex.Message, ex.ErrorRecord.ScriptStackTrace);
                        res.Add(msg);
                    }
                    catch (RuntimeException ex)
                    {
                        var msg = string.Format("Exception: RuntimeException: {0}\n{1}", ex.Message, ex.ErrorRecord.ScriptStackTrace);
                        res.Add(msg);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Unknown error occured executing Powershell script", ex);
                    }
                    finally
                    {
                        script.IsRunning = false;
                    }
                    return res;
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _timer.Close();
            }
            catch { }
            _timer.Dispose();
        }
    }
    public class ScriptObject
    {
        public string ScriptPath { get; set; }
        public Dictionary<string, string> ScriptArgs { get; set; }
        public bool IsRunning { get; set; }

        public ScriptObject(string scriptPath, Dictionary<string,string> scriptArgs = null)
        {
            ScriptPath = scriptPath;
            ScriptArgs = scriptArgs;
        }
    }
}
