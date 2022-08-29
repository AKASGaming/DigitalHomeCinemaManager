using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalHomeCinemaManager
{
    public class Output
    {
        private readonly string LogDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        private static Output _outputSingleton;
        private static Output OutputSingleton
        {
            get
            {
                if (_outputSingleton == null)
                {
                    _outputSingleton = new Output();
                }
                return _outputSingleton;
            }
        }

        public StreamWriter SW { get; set; }

        public Output()
        {
            EnsureLogDirectoryExists();
            InstantiateStreamWriter();
        }

        ~Output()
        {
            if (SW != null)
            {
                try
                {
                    SW.Dispose();
                }
                catch (ObjectDisposedException) { } // object already disposed - ignore exception
            }
        }

        public static void WriteLine(string str)
        {
            if(Properties.Settings.Default.EnableLogs == true)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", DateTime.Now, str));
                OutputSingleton.SW.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", DateTime.Now, str));
            } else Console.WriteLine(str);
        }

        public static void Write(string str)
        {
            if (Properties.Settings.Default.EnableLogs == true)
            {
                Console.Write(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", DateTime.Now, str));
                OutputSingleton.SW.Write(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", DateTime.Now, str));
            }
            else Console.Write(str);
        }

        private void InstantiateStreamWriter()
        {
            string filePath = Path.Combine(LogDirPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")) + ".txt";
            try
            {
                SW = new StreamWriter(filePath);
                SW.AutoFlush = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new ApplicationException(string.Format("Access denied. Could not instantiate StreamWriter using path: {0}.", filePath), ex);
            }
        }

        private void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(LogDirPath))
            {
                try
                {
                    Directory.CreateDirectory(LogDirPath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw new ApplicationException(string.Format("Access denied. Could not create log directory using path: {0}.", LogDirPath), ex);
                }
            }
        }
    }
}
