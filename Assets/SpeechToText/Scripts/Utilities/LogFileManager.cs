using System.IO;
using UnityEngine;

namespace UnitySpeechToText.Utilities
{
    /// <summary>
    /// Manager for logging to a file.
    /// </summary>
    public class LogFileManager : MonoSingleton<LogFileManager>
    {
        /// <summary>
        /// Store for LogFileBaseName property
        /// </summary>
        [SerializeField]
        string m_LogFileBaseName = "log.txt";
        /// <summary>
        /// Store for ShouldLogToFile property
        /// </summary>
        bool m_ShouldLogToFile;
        /// <summary>
        /// Absolute path to the log file
        /// </summary>
        string m_LogFilePath;
        /// <summary>
        /// Handle to be used for the file system lock
        /// </summary>
        object m_FileLockHandle = new object();
        
        /// <summary>
        /// Base name for the log file
        /// </summary>
        public string LogFileBaseName { set { m_LogFileBaseName = value; } }
        /// <summary>
        /// Whether text results should be saved to a file
        /// </summary>
        public bool ShouldLogToFile { set { m_ShouldLogToFile = value; } }

        /// <summary>
        /// Constructor for LogFileManager. Because this class inherits from MonoSingleton, ordinary construction must be prevented.
        /// </summary>
        protected LogFileManager() { }

        /// <summary>
        /// Initialization function called on the frame when the script is enabled just before any of the Update
        /// methods is called the first time.
        /// </summary>
        void Start()
        {
            m_LogFilePath = IOUtilities.MakeFilePathUnique(Path.Combine(Path.Combine(Application.dataPath, Constants.SpeechToTextFolderName), m_LogFileBaseName));
        }

        /// <summary>
        /// Writes the given text to a line in the log file if m_ShouldLogToFile is true.
        /// </summary>
        /// <param name="text">Text to write to the file</param>
        public void WriteTextToFileIfShouldLog(string text)
        {
            if (m_ShouldLogToFile)
            {
                lock (m_FileLockHandle)
                {
                    SmartLogger.Log(DebugFlags.LogFileManager, "log to file");
                    if (!File.Exists(m_LogFilePath))
                    {
                        FileStream file = File.Create(m_LogFilePath);
                        file.Close();
                    }

                    using (var fileStream = new FileStream(m_LogFilePath, FileMode.Append, FileAccess.Write))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.WriteLine(text);
                        }
                        fileStream.Close();
                    }
                }
            }
        }
    }
}
