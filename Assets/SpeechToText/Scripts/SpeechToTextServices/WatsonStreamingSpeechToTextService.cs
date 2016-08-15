using System.Collections;
using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using UnityEngine;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// IBM Watson streaming speech-to-text SDK.
    /// </summary>
    public class WatsonStreamingSpeechToTextService : StreamingSpeechToTextService
    {
        /// <summary>
        /// Watson speech-to-text functionality
        /// </summary>
        WatsonSpeechToTextComponent m_WatsonSpeechToTextComponent = new WatsonSpeechToTextComponent();
        /// <summary>
        /// Store for APIUsername property
        /// </summary>
        [SerializeField]
        string m_APIUsername;
        /// <summary>
        /// Store for APIPassword property
        /// </summary>
        [SerializeField]
        string m_APIPassword;
        
        /// <summary>
        /// Watson speech-to-text username
        /// </summary>
        public string APIUsername { set { m_APIUsername = value; } }
        /// <summary>
        /// Watson speech-to-text password
        /// </summary>
        public string APIPassword { set { m_APIPassword = value; } }

        /// <summary>
        /// Initialization function called on the frame when the script is enabled just before any of the Update
        /// methods is called the first time.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            m_WatsonSpeechToTextComponent.ConfigureCredentials(m_APIUsername, m_APIPassword);
            m_WatsonSpeechToTextComponent.WatsonSpeechToTextService.OnError = OnSpeechToTextError;
            m_WatsonSpeechToTextComponent.WatsonSpeechToTextService.EnableContinousRecognition = true;
            m_WatsonSpeechToTextComponent.WatsonSpeechToTextService.EnableInterimResults = true;
            m_WatsonSpeechToTextComponent.WatsonSpeechToTextService.DetectSilence = false;
        }

        /// <summary>
        /// Sends queued chunks of audio to the server and then waits for the last result in the session
        /// before telling the Watson speech-to-text service to stop listening.
        /// </summary>
        protected override IEnumerator StreamAudioAndListenForResponses()
        {
            m_WatsonSpeechToTextComponent.WatsonSpeechToTextService.StartListening(OnSpeechToTextResult);

            // While still recording, send chunks as they arrive in the queue.
            while (m_IsRecording)
            {
                while (m_AudioChunksQueue.Count == 0)
                {
                    yield return null;
                }
                SendNextChunk();
            }

            // Send any remaining chunks.
            while (m_AudioChunksQueue.Count > 0)
            {
                SendNextChunk();
            }

            // Wait a specified number of seconds for a final result.
            float timeElapsedAfterRecording = 0;
            while (!m_LastResult.IsFinal && timeElapsedAfterRecording < m_SessionTimeoutAfterDoneRecording)
            {
                yield return null;
                timeElapsedAfterRecording += Time.deltaTime;
            }

            // If still determining a final result, just treat the last result processed as a final result.
            if (!m_LastResult.IsFinal)
            {
                m_LastResult.IsFinal = true;
                if (m_OnTextResult != null)
                {
                    m_OnTextResult(m_LastResult);
                }
            }

            m_WatsonSpeechToTextComponent.WatsonSpeechToTextService.StopListening();
        }

        /// <summary>
        /// Sends the next audio chunk in the queue to the server.
        /// </summary>
        void SendNextChunk()
        {
            AudioClip currentChunk = m_AudioChunksQueue.Dequeue();
            var samples = new float[currentChunk.samples];
            currentChunk.GetData(samples, 0);

            m_WatsonSpeechToTextComponent.WatsonSpeechToTextService.OnListen(new AudioData(currentChunk, Mathf.Max(samples)));
        }

        /// <summary>
        /// Function that is called when the Watson API returns a result.
        /// </summary>
        /// <param name="results">List of speech-to-text results</param>
        protected void OnSpeechToTextResult(SpeechResultList results)
        {
            if (results.HasResult())
            {
                SpeechResult watsonResult = results.Results[0];
                var textResult = m_WatsonSpeechToTextComponent.CreateSpeechToTextResult(watsonResult);
                if (m_OnTextResult != null)
                {
                    m_OnTextResult(textResult);
                }
                m_LastResult = textResult;
            }
        }

        /// <summary>
        /// Function that is called when an error occurs.
        /// </summary>
        /// <param name="error">Error text</param>
        void OnSpeechToTextError(string error)
        {
            if (m_OnError != null)
            {
                m_OnError(error);
            }
        }
    }
}
