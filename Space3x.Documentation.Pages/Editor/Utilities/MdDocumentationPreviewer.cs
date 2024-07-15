using System;
using System.Diagnostics;
using Space3x.UiToolkit.Types;
using UnityEngine.Device;
using Debug = UnityEngine.Debug;

namespace Space3x.Documentation
{
    public static class MdDocumentationPreviewer
    {
        public static event Action<bool> OnServerRunningChanged;
        public static bool IsServerRunning
        {
            get => m_IsRunning;
            private set
            {
                if (m_IsRunning == value)
                    return;
                m_IsRunning = value;
                OnServerRunningChanged?.Invoke(m_IsRunning);
            }
        }
        
        public static int Port { get; set; } = 3003;
        
        private static Process m_Process = null;
        private static bool m_IsRunning = false;
        
        private static void Run(string binaryPath, string arguments)
        {
            m_Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = binaryPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true,
                    WorkingDirectory = MdDocumentationGenerator.DocumentationPath
                },
                EnableRaisingEvents = true
            };

            m_Process.Exited += (_, _) => IsServerRunning = false;
            m_Process.Start();
        }

        public static void Start()
        {
            var npxPath = Paths.Npx;
            if (!string.IsNullOrEmpty(npxPath))
            {
                Run(npxPath, $"--yes http-server -p {Port}");
                IsServerRunning = !m_Process.HasExited;
            }
            else
            {
                var pythonPath = Paths.Python;
                if (!string.IsNullOrEmpty(pythonPath))
                {
                    Run(pythonPath, $"-m http.server {Port}");
                    IsServerRunning = !m_Process.HasExited;
                }
                else 
                    Debug.LogError("No suitable process to start an http server found. Please install either NPM or Python3.");
            }
        }

        public static void Stop()
        {
            if (m_Process != null && !m_Process.HasExited)
            {
                m_Process.Kill();
                m_Process = null;
            }
            IsServerRunning = false;
        }

        public static void Open() => Application.OpenURL($"http://localhost:{Port}");
    }
}
