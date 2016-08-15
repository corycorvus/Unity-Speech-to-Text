using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace UnitySpeechToText.Utilities
{
    /// <summary>
    /// Threaded job to convert an audio file to FLAC encoding using SoX.
    /// </summary>
    public class SoXAudioConversionJob : ThreadedJob
    {
        /// <summary>
        /// Formatting info for an audio file.
        /// </summary>
        public class AudioFileFormatting
        {
            /// <summary>
            /// Number of channels
            /// </summary>
            public int Channels { get; set; }
            /// <summary>
            /// Number of bits in each sample
            /// </summary>
            public int EncodingBits { get; set; }
            /// <summary>
            /// Audio encoding type
            /// </summary>
            public AudioEncoding.EncodingType EncodingType { get; set; }
            /// <summary>
            /// Byte order of audio data
            /// </summary>
            public Endianness.EndiannessType Endianness { get; set; }
            /// <summary>
            /// Sampling frequency in Hertz
            /// </summary>
            public int Frequency { get; set; }

            /// <summary>
            /// Class constructor
            /// </summary>
            /// <param name="channels">Number of channels</param>
            /// <param name="encodingBits">Number of bits in each sample</param>
            /// <param name="encodingType">Audio encoding type</param>
            /// <param name="endianness">Byte order of audio data</param>
            /// <param name="frequency">Sampling frequency in Hertz</param>
            public AudioFileFormatting(int channels, int encodingBits, 
                AudioEncoding.EncodingType encodingType, Endianness.EndiannessType endianness, int frequency)
            {
                Channels = channels;
                EncodingBits = encodingBits;
                EncodingType = encodingType;
                Endianness = endianness;
                Frequency = frequency;
            }
        }
        
        /// <summary>
        /// Default number of bits per sample in the output audio
        /// </summary>
        const int k_DefaultOutputBits = 16;
        /// <summary>
        /// Default number of channels in the output audio
        /// </summary>
        const int k_DefaultOutputChannels = 1;
        /// <summary>
        /// Base folder for SoX
        /// </summary>
        const string k_SoXFolderName = "SoX";
        /// <summary>
        /// Folder for the Windows version of SoX
        /// </summary>
        const string k_SoXWindowsFolderName = "Windows";
        /// <summary>
        /// Folder for the Mac OSX version of SoX
        /// </summary>
        const string k_SoXMacOSXFolderName = "MacOSX";
        string m_SoXPath;
        /// <summary>
        /// Store for ErrorMessage property
        /// </summary>
        string m_ErrorMessage;
        /// <summary>
        /// Store for InputFilePath property
        /// </summary>
        string m_InputFilePath;
        /// <summary>
        /// Store for OutputFilePath property
        /// </summary>
        string m_OutputFilePath;
        /// <summary>
        /// Store for OutputFileFormatting property
        /// </summary>
        AudioFileFormatting m_OutputFileFormatting;

        /// <summary>
        /// Error message associated with the conversion job
        /// </summary>
        public string ErrorMessage { get { return m_ErrorMessage; } }
        /// <summary>
        /// Path to the audio file to convert
        /// </summary>
        public string InputFilePath { set { m_InputFilePath = value; } }
        /// <summary>
        /// Path to the output audio file
        /// </summary>
        public string OutputFilePath { set { m_OutputFilePath = value; } }
        /// <summary>
        /// Formatting for the output audio file
        /// </summary>
        public AudioFileFormatting OutputFileFormatting { set { m_OutputFileFormatting = value; } }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="inputFilePath">Path to the audio file to convert</param>
        /// <param name="outputFilePath">Path to the output audio file</param>
        /// <param name="outputFrequency">Sampling frequency of the output audio in Hertz</param>
        /// <param name="outputBits">Number of bits in each sample in the output audio</param>
        /// <param name="outputChannels">Number of channels in the output audio</param>
        /// <param name="outputEncodingType">Encoding type of the output audio</param>
        /// <param name="outputEndianness">Byte order of the output audio</param>
        public SoXAudioConversionJob(string inputFilePath, string outputFilePath, int outputFrequency,
            int outputBits = k_DefaultOutputBits, int outputChannels = k_DefaultOutputChannels,
            AudioEncoding.EncodingType outputEncodingType = AudioEncoding.EncodingType.Unspecified, 
            Endianness.EndiannessType outputEndianness = Endianness.EndiannessType.Unspecified) : 
            this(inputFilePath, outputFilePath, new AudioFileFormatting(outputChannels, 
                outputBits, outputEncodingType, outputEndianness, outputFrequency)) { }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="inputFilePath">Path to the audio file to convert</param>
        /// <param name="outputFilePath">Path to the output audio file</param>
        /// <param name="outputFormatting">Formatting of the output audio</param>
        public SoXAudioConversionJob(string inputFilePath, string outputFilePath, AudioFileFormatting outputFormatting)
        {
            m_InputFilePath = inputFilePath;
            m_OutputFilePath = outputFilePath;
            m_OutputFileFormatting = outputFormatting;
            m_SoXPath = Path.Combine(Path.Combine(Application.streamingAssetsPath, Constants.StreamingAssetsThirdPartyFolderName), k_SoXFolderName);
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WINRT || UNITY_WINRT_8_0 || UNITY_WINRT_8_1 || UNITY_WINRT_10_0)
            m_SoXPath = Path.Combine(Path.Combine(m_SoXPath, k_SoXWindowsFolderName), Constants.SoXApplicationName + ".exe");
#else 
            m_SoXPath = Path.Combine(Path.Combine(m_SoXPath, k_SoXMacOSXFolderName), Constants.SoXApplicationName);
#endif
        }

        /// <summary>
        /// Specific thread function to run.
        /// </summary>
        protected override void ThreadFunction()
        {
            ConvertAudio();
        }

        /// <summary>
        /// Attempts to run an SoX process that converts the input audio file to FLAC encoding.
        /// </summary>
        void ConvertAudio()
        {
            m_ErrorMessage = null;
            if (File.Exists(m_SoXPath))
            {
                var audioConversionProcess = new Process();
                audioConversionProcess.StartInfo.FileName = m_SoXPath;
                audioConversionProcess.StartInfo.Arguments = m_InputFilePath +
                    " " + Constants.SoXFrequencyOptionName + " " + m_OutputFileFormatting.Frequency +
                    " " + Constants.SoXEncodingBitsOptionName + " " + m_OutputFileFormatting.EncodingBits + 
                    " " + Constants.SoXChannelsOptionName + " " + m_OutputFileFormatting.Channels;
                if (Endianness.SoXEndiannessNames.ContainsKey(m_OutputFileFormatting.Endianness))
                {
                    audioConversionProcess.StartInfo.Arguments += " " + Constants.SoXEndiannessOptionName + 
                        " " + Endianness.SoXEndiannessNames[m_OutputFileFormatting.Endianness];
                }
                if (AudioEncoding.EncodingNames.ContainsKey(m_OutputFileFormatting.EncodingType))
                {
                    audioConversionProcess.StartInfo.Arguments += " " + Constants.SoXEncodingTypeOptionName +
                        " " + AudioEncoding.EncodingNames[m_OutputFileFormatting.EncodingType];
                }
                audioConversionProcess.StartInfo.Arguments += " " + m_OutputFilePath;
                audioConversionProcess.StartInfo.CreateNoWindow = true;
                audioConversionProcess.StartInfo.UseShellExecute = false;

                audioConversionProcess.Start();
                audioConversionProcess.WaitForExit();

                if (!File.Exists(m_OutputFilePath))
                {
                    m_ErrorMessage = "Audio conversion with SoX was unsuccessful.";
                }
            }
            else
            {
                m_ErrorMessage = "Could not locate audio conversion application SoX within StreamingAssets.";
            }
        }
    }
}
