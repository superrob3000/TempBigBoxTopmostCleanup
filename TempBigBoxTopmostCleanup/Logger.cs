using System;
using System.IO;

namespace TempBigBoxTopmostCleanup
{
    internal class Logger
    {
        public Logger()
        {

        }

        public void InitLogging(String filename)
        {
            LogFileName = filename;
            enabled = true;
        }

        public void Log(String info)
        {
            if (enabled)
            {
                StreamWriter sw = null;

                if (!Directory.Exists(Path.GetDirectoryName(LogFileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(LogFileName));
                }

                if (!File.Exists(LogFileName))
                {
                    // Create a file to write to.
                    sw = File.CreateText(LogFileName);
                }
                else
                {
                    sw = File.AppendText(LogFileName);
                }

                if (sw != null)
                {
                    sw.WriteLine("[" + DateTime.Now + "] " + info);
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        private bool enabled = false;
        private String LogFileName;
    }
}
