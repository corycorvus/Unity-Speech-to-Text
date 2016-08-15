using System;
using System.IO;
using System.Collections;
using HTTP;
using UnityEngine;
using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Wit.ai non-streaming speech-to-text SDK.
    /// </summary>
    public class WitAiNonStreamingSpeechToTextService : NonStreamingSpeechToTextService
    {
        /// <summary>
        /// Component used to manage temporary audio files
        /// </summary>
        TempAudioFileSavingComponent m_TempAudioComponent = new TempAudioFileSavingComponent("WitAiNonStreamingAudio");
        /// <summary>
        /// Store for APIAccessToken property
        /// </summary>
        [SerializeField]
        string m_APIAccessToken;

        /// <summary>
        /// Access token for API calls
        /// </summary>
        public string APIAccessToken { set { m_APIAccessToken = value; } }

        /// <summary>
        /// Function that is called when the MonoBehaviour will be destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_TempAudioComponent.ClearTempAudioFiles();
        }

        /// <summary>
        /// Translates speech to text by making a request to the speech-to-text API.
        /// </summary>
        protected override IEnumerator TranslateRecordingToText()
        {
            m_TempAudioComponent.ClearTempAudioFiles();

            // Save recorded audio to a WAV file.
            string recordedAudioFilePath = SavWav.Save(m_TempAudioComponent.TempAudioRelativePath(), AudioRecordingManager.Instance.RecordedAudio);

            // Construct a request with the WAV file and send it.
            var request = new Request("POST", Constants.WitAiSpeechToTextBaseURL + "?" +
                Constants.WitAiVersionParameterName + "=" + DateTime.Now.ToString(Constants.WitAiVersionDateFormat));
            request.headers.Add("Authorization", "Bearer " + m_APIAccessToken);
            request.headers.Add("Content-Type", "audio/wav");
            request.Bytes = File.ReadAllBytes(recordedAudioFilePath);
            SmartLogger.Log(DebugFlags.WitAINonStreamingSpeechToText, "Sending request");
            request.Send();

            float startTime = Time.time;
            while (!request.isDone)
            {
                yield return null;
            }
            SmartLogger.Log(DebugFlags.WitAINonStreamingSpeechToText, "response time: " + (Time.time - startTime));

            // Finally, grab the response JSON once the request is done.
            var responseJSON = new JSONObject(request.response.Text, int.MaxValue);
            SmartLogger.Log(DebugFlags.WitAINonStreamingSpeechToText, "Received request result");
            SmartLogger.Log(DebugFlags.WitAINonStreamingSpeechToText, responseJSON.ToString());

            string errorText = WitAiSpeechToTextResponseJSONParser.GetErrorFromResponseJSON(responseJSON);
            if (errorText != null)
            {
                if (m_OnError != null)
                {
                    m_OnError(errorText);
                }
            }

            if (m_OnTextResult != null)
            {
                m_OnTextResult(WitAiSpeechToTextResponseJSONParser.GetTextResultFromResponseJSON(responseJSON));
            }

            m_TempAudioComponent.ClearTempAudioFiles();
        }
    }
}
