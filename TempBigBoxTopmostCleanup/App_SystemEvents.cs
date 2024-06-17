using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;


namespace TempBigBoxTopmostCleanup
{
    internal class App_SystemEvents : ISystemEventsPlugin
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly UInt32 SWP_NOSIZE = 0x0001;
        static readonly UInt32 SWP_NOMOVE = 0x0002;
        static readonly UInt32 SWP_SHOWWINDOW = 0x0040;
        static readonly int GWL_EXSTYLE = -20;
        static readonly int WS_EX_TOPMOST = 0x0008;

        public bool IsWindowTopMost(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            return (exStyle & WS_EX_TOPMOST) == WS_EX_TOPMOST;
        }

        public void ClearWindowTopMost(IntPtr hWnd)
        {
            if (IsWindowTopMost(hWnd))
            {
                logger.Log("Clearing BigBox topmost");
                SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
            
            if (IsWindowTopMost(hWnd))
            {
                logger.Log("Failed to clear topmost");
            }
        }

        private IntPtr GetHandleWindow(string title)
        {
            return FindWindow(null, title);
        }

        void CleanUpBigBoxTopmost()
        {
            IntPtr hWnd = IntPtr.Zero;

            hWnd = GetHandleWindow("LaunchBox Big Box");

            if (hWnd != IntPtr.Zero)
            {
                ClearWindowTopMost(hWnd);
            }
            else
            {
                logger.Log("Could not find BigBox window");
            }
        }

        void ISystemEventsPlugin.OnEventRaised(string eventType)
        {
            switch (eventType)
            {
                case SystemEventTypes.PluginInitialized:
                    if (PluginHelper.StateManager.IsBigBox)
                    {
                        ExecutablePath = Path.GetDirectoryName(Application.ExecutablePath).ToString();
                        if (ExecutablePath.ToLower().EndsWith("/core") || ExecutablePath.ToLower().EndsWith("/core/")
                            || ExecutablePath.ToLower().EndsWith("\\core") || ExecutablePath.ToLower().EndsWith("\\core\\"))
                            ExecutablePath = Directory.GetParent(ExecutablePath).ToString();

                        if (LoggingEnabled.Equals("true"))
                        {
                            logger.InitLogging(Path.Combine(ExecutablePath, "Logs/TempBigBoxTopmostCleanupLog.txt"));
                        }
                    }
                    break;

                case SystemEventTypes.GameExited:
                case SystemEventTypes.SelectionChanged:
                    //Check for topmost being set after a game exits.
                    //Also check after a selection change because the GameExited event does not always fire.
                    if (PluginHelper.StateManager.IsBigBox)
                        CleanUpBigBoxTopmost();
                    break;
            }
        }

        String ExecutablePath;

        String LoggingEnabled = "true";

        Logger logger = new Logger();
    }
}
