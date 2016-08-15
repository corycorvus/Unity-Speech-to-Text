using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Google streaming speech-to-text SDK.
    /// </summary>
    public class GoogleStreamingSpeechToTextService : StreamingSpeechToTextService
    {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WINRT || UNITY_WINRT_8_0 || UNITY_WINRT_8_1 || UNITY_WINRT_10_0)
        /// <summary>
        /// Store for JSONCredentialsFileName property
        /// </summary>
        [SerializeField]
        string m_JSONCredentialsFileName = "credentials.json";
        /// <summary>
        /// Relative folder from temp folder in which the streaming speech-to-text application exists
        /// </summary>
        const string k_StreamingSpeechToTextApplicationFolderName = "GoogleStreamingSpeechToTextProgram";
        /// <summary>
        /// Name of the streaming speech-to-text application
        /// </summary>
        const string k_StreamingSpeechToTextApplicationFileName = "GoogleCloudSpeech.exe";
        /// <summary>
        /// Prompt string from the streaming speech-to-text process signalling that it is ready to receive data
        /// </summary>
        const string k_ReadyToStreamDataOutputPrompt = "ready to start";
        /// <summary>
        /// Prompt string expected by the streaming speech-to-text process signalling that it should start streaming data
        /// </summary>
        const string k_StartStreamingDataInputPrompt = "start";
        /// <summary>
        /// Prompt string expected by the streaming speech-to-text process signalling that it should stop streaming data
        /// </summary>
        const string k_StopStreamingDataInputPrompt = "stop";
        /// <summary>
        /// String that prefixes a JSON response in the speech-to-text process output
        /// </summary>
        const string k_ResponsePrefix = "response: ";
        /// <summary>
        /// Component used to manage temporary audio files
        /// </summary>
        TempAudioFileSavingComponent m_TempAudioComponent = new TempAudioFileSavingComponent("GoogleStreamingAudio");
        /// <summary>
        /// External process for streaming speech-to-text
        /// </summary>
        Process m_StreamingSpeechToTextProcess;
        /// <summary>
        /// Queue of JSON strings that represent speech-to-text responses
        /// </summary>
        Queue<string> m_ResponseJSONsQueue = new Queue<string>();
        /// <summary>
        /// Whether the streaming speech-to-text process has started running
        /// </summary>
        bool m_StreamingSpeechToTextProcessHasStarted;
        /// <summary>
        /// Whether the streaming speech-to-text process is ready to receive streamed audio data
        /// </summary>
        bool m_ReadyToStreamData;

        /// <summary>
        /// Name of the Google Application Credentials JSON file
        /// </summary>
        public string JSONCredentialsFileName { set { m_JSONCredentialsFileName = value; } }

        /// <summary>
        /// Function that is called when the MonoBehaviour will be destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_TempAudioComponent.ClearTempAudioFiles();
            if (m_StreamingSpeechToTextProcessHasStarted && !m_StreamingSpeechToTextProcess.HasExited)
            {
                m_StreamingSpeechToTextProcess.Kill();
                SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, "kill streaming speech-to-text process");
            }
        }

        /// <summary>
        /// Sends queued chunks of audio to the server and listens for responses.
        /// </summary>
        protected override IEnumerator StreamAudioAndListenForResponses()
        {
            m_TempAudioComponent.ClearTempAudioFiles();
            m_ResponseJSONsQueue.Clear();
            m_StreamingSpeechToTextProcessHasStarted = false;
            m_ReadyToStreamData = false;

            string jsonCredentialsPath = Path.Combine(
                Path.Combine(Application.streamingAssetsPath, k_StreamingSpeechToTextApplicationFolderName),
                m_JSONCredentialsFileName);
            if (!File.Exists(jsonCredentialsPath))
            {
                if (m_OnError != null)
                {
                    m_OnError("Missing JSON credentials file in StreamingAssets/GoogleStreamingSpeechToTextProgram");
                }
                yield break;
            }

            // Initialize streaming speech-to-text process with appropriate start info, including the path to the credentials file.
            m_StreamingSpeechToTextProcess = new Process();
            m_StreamingSpeechToTextProcess.StartInfo.FileName = Path.Combine(
                Path.Combine(Application.streamingAssetsPath, k_StreamingSpeechToTextApplicationFolderName),
                k_StreamingSpeechToTextApplicationFileName);
            m_StreamingSpeechToTextProcess.StartInfo.Arguments = jsonCredentialsPath;
            m_StreamingSpeechToTextProcess.StartInfo.CreateNoWindow = true;
            m_StreamingSpeechToTextProcess.StartInfo.UseShellExecute = false;
            m_StreamingSpeechToTextProcess.StartInfo.RedirectStandardInput = true;
            m_StreamingSpeechToTextProcess.StartInfo.RedirectStandardOutput = true;
            m_StreamingSpeechToTextProcess.OutputDataReceived += OnStreamingSpeechToTextProcessOutputDataReceived;
            
            SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, "start streaming speech-to-text process");
            m_StreamingSpeechToTextProcess.Start();
            m_StreamingSpeechToTextProcess.BeginOutputReadLine();
            m_StreamingSpeechToTextProcessHasStarted = true;

            while (!m_ReadyToStreamData)
            {
                yield return null;
            }

            // TODO: I don't know why, but I need to write garbage text first.
            // For some reason the first standard input begins with "0x3F3F3F".
            SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, "ready to stream data");
            m_StreamingSpeechToTextProcess.StandardInput.WriteLine("clear input stream");

            // Tell the process to start streaming.
            m_StreamingSpeechToTextProcess.StandardInput.WriteLine(k_StartStreamingDataInputPrompt);
            
            StartCoroutine(ProcessResponseJSONs());

            // While still recording, send chunks as they arrive in the queue.
            while (m_IsRecording)
            {
                while (m_AudioChunksQueue.Count == 0)
                {
                    yield return null;
                }
                yield return SaveAndSendNextChunk();
            }
            SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, "stopped recording");

            // Send any remaining chunks.
            while (m_AudioChunksQueue.Count > 0)
            {
                yield return SaveAndSendNextChunk();
            }
            SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, "sent all chunks");

            // Tell the process to stop streaming.
            m_StreamingSpeechToTextProcess.StandardInput.WriteLine(k_StopStreamingDataInputPrompt);

            // Wait a specified number of seconds for a final result.
            float timeElapsedAfterRecording = 0;
            while (!m_LastResult.IsFinal && timeElapsedAfterRecording < m_SessionTimeoutAfterDoneRecording)
            {
                yield return null;
                timeElapsedAfterRecording += Time.deltaTime;
            }
            SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, "session timeout");

            // If still determining a final result, just treat the last result processed as a final result.
            if (!m_LastResult.IsFinal)
            {
                SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, "treat last result as final result");
                m_LastResult.IsFinal = true;
                if (m_OnTextResult != null)
                {
                    m_OnTextResult(m_LastResult);
                }
            }

            while (!m_StreamingSpeechToTextProcess.HasExited)
            {
                yield return null;
            }
            SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, "streaming speech-to-text process exited");

            m_TempAudioComponent.ClearTempAudioFiles();
        }

        /// <summary>
        /// Callback function for when the streaming speech-to-text process receives output data.
        /// </summary>
        /// <param name="sender">Sender of this event</param>
        /// <param name="e">Arguments for data received event</param>
        void OnStreamingSpeechToTextProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string trimmedData = e.Data.Trim();
                SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, "process output: " + trimmedData);
                if (trimmedData == k_ReadyToStreamDataOutputPrompt)
                {
                    SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, "set ready to stream data");
                    m_ReadyToStreamData = true;
                }
                else if (trimmedData.StartsWith(k_ResponsePrefix))
                {
                    trimmedData = trimmedData.Remove(0, k_ResponsePrefix.Length);
                    m_ResponseJSONsQueue.Enqueue(trimmedData);
                }
            }
        }

        /// <summary>
        /// Creates a temporary audio file from the next audio chunk in the queue and writes the
        /// file name to the streaming speech-to-text process so that it can send its data to the server.
        /// </summary>
        IEnumerator SaveAndSendNextChunk()
        {
            // Save recorded audio to a WAV file and convert it to RAW format.
            string wavAudioFilePath = SavWav.Save(m_TempAudioComponent.TempAudioRelativePath(), m_AudioChunksQueue.Dequeue());
            string rawAudioFilePath = IOUtilities.MakeFilePathUnique(Path.ChangeExtension(wavAudioFilePath, "raw"));
            var audioConversionJob = new SoXAudioConversionJob(wavAudioFilePath, rawAudioFilePath, 16000, 16, 1,
                AudioEncoding.EncodingType.SignedInteger, Endianness.EndiannessType.Little);
            audioConversionJob.Start();
            yield return StartCoroutine(audioConversionJob.WaitFor());
            
            if (audioConversionJob.ErrorMessage != null)
            {
                if (m_OnError != null)
                {
                    m_OnError(audioConversionJob.ErrorMessage);
                }
                yield break;
            }

            m_StreamingSpeechToTextProcess.StandardInput.WriteLine(rawAudioFilePath);
        }

        /// <summary>
        /// Process any JSON string responses in the queue.
        /// </summary>
        IEnumerator ProcessResponseJSONs()
        {
            // While still recording or the speech-to-text process is still running,
            // process JSON responses as they arrive in the queue.
            while (m_IsRecording || !m_StreamingSpeechToTextProcess.HasExited)
            {
                while (m_ResponseJSONsQueue.Count == 0)
                {
                    yield return null;
                }

                ProcessNextResponseJSON();
            }

            // Process any remaining responses.
            while (m_ResponseJSONsQueue.Count > 0)
            {
                ProcessNextResponseJSON();
            }
        }

        /// <summary>
        /// Extract the speech-to-text result info from the next response JSON in the queue.
        /// </summary>
        void ProcessNextResponseJSON()
        {
            // Create a JSON object from the next string in the queue and process the speech-to-text result.
            var responseJSON = new JSONObject(m_ResponseJSONsQueue.Dequeue(), int.MaxValue);
            SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, responseJSON.ToString());

            string errorText = GoogleSpeechToTextResponseJSONParser.GetErrorFromResponseJSON(responseJSON);
            if (errorText != null)
            {
                if (m_OnError != null)
                {
                    m_OnError(errorText);
                }
            }

            JSONObject resultsJSON = responseJSON.GetField(Constants.GoogleResponseJSONResultsFieldKey);
            if (resultsJSON != null && resultsJSON.Count > 0)
            {
                JSONObject resultJSON = resultsJSON[0];
                SpeechToTextResult textResult = GoogleSpeechToTextResponseJSONParser.GetTextResultFromResultJSON(resultJSON);
                bool isFinal = false;
                resultJSON.GetField(out isFinal, Constants.GoogleResponseJSONResultIsFinalFieldKey, isFinal);
                textResult.IsFinal = isFinal;

                SmartLogger.Log(DebugFlags.GoogleStreamingSpeechToText, "processing result - isFinal = " + isFinal);
                if (m_OnTextResult != null)
                {
                    m_OnTextResult(textResult);
                }
                m_LastResult = textResult;
            }
        }
#else
        /// <summary>
        /// Initialization function called on the frame when the script is enabled just before any of the Update
        /// methods is called the first time.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            SmartLogger.LogError(DebugFlags.GoogleStreamingSpeechToText, "This service is only supported on Windows.");
        }

        /// <summary>
        /// This class is required to override this abstract base function.
        /// </summary>
        protected override IEnumerator StreamAudioAndListenForResponses()
        {
            yield return null;
        }
#endif
    }
}
