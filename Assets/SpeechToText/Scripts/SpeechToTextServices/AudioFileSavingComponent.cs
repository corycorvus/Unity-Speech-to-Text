using System.IO;
using UnityEngine;
using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Container for temporary audio file management.
    /// </summary>
    public class TempAudioFileSavingComponent
    {
        /// <summary>
        /// Store for TempAudioFolderName property
        /// </summary>
        string m_TempAudioFolderName = "Default";
        /// <summary>
        /// File name to use for temporarily saved audio
        /// </summary>
        const string k_TempAudioFileName = "audio";

        /// <summary>
        /// Name of folder to which to save temporary audio files
        /// </summary>
        public string TempAudioFolderName { set { m_TempAudioFolderName = value; } }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="tempAudioFolderName">Name of folder to which to save temporary audio files</param>
        public TempAudioFileSavingComponent(string tempAudioFolderName)
        {
            TempAudioFolderName = tempAudioFolderName;
        }

        /// <summary>
        /// Returns the relative path to temporary audio.
        /// </summary>
        /// <returns>Relative path to temporary audio</returns>
        public string TempAudioRelativePath()
        {
            return Path.Combine(m_TempAudioFolderName, k_TempAudioFileName);
        }

        /// <summary>
        /// Recursively removes the directory containing temporary audio files.
        /// </summary>
        public void ClearTempAudioFiles()
        {
            string tempAudioDirectory = Path.Combine(Path.Combine(Path.Combine(Application.dataPath, Constants.SpeechToTextFolderName), Constants.TempFolderName), m_TempAudioFolderName);
            if (Directory.Exists(tempAudioDirectory))
            {
                Directory.Delete(tempAudioDirectory, true);
            }
        }
    }
}
