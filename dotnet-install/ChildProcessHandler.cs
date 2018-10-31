namespace VQCore.Infrastructure.ProcessHost
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    public class ChildProcessHandler : IDisposable
    {
        private readonly string processName;

        public ChildProcessHandler(string program, string args)
        {
            this.processName = program.Substring(program.LastIndexOf("/") + 1);
           
            var info = new ProcessStartInfo(program, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            this.Process = new Process
            {
                StartInfo = info,
                EnableRaisingEvents = true,
            };
        }

        public Action<object, DataReceivedEventArgs> OnOutputDataReceived { get; set; }

        public Action<object, DataReceivedEventArgs> OnErrorDataReceived { get; set; }

        public Action<object, EventArgs> OnExited { get; set; }

        public int ExitCode => this.Process.HasExited ? this.Process.ExitCode : -1;

        public bool IsRunning
        {
            get
            {
                try
                {
                    return Process.GetProcessesByName(this.processName).Any();
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                catch (ArgumentException)
                {
                    return false;
                }
                catch (NullReferenceException)
                {
                    return false;
                }
            }
        }

        private Process Process { get; }

        public void Run()
        {
            this.Process.OutputDataReceived += (sender, e) => { this.OnOutputDataReceived?.Invoke(sender, e); };

            this.Process.ErrorDataReceived += (sender, e) => { this.OnErrorDataReceived?.Invoke(sender, e); };

            this.Process.Exited += (sender, e) => { this.OnExited?.Invoke(sender, e); };

            this.Process.Start();
            this.Process.BeginOutputReadLine();
            this.Process.BeginErrorReadLine();
            this.Process.WaitForExit();
        }

        public void Cancel()
        {
            if (!this.Process.HasExited)
            {
                this.Process.Kill();
                this.Process.Dispose();
            }
        }

        public void Dispose()
        {
            this.Cancel();
        }
    }
}
