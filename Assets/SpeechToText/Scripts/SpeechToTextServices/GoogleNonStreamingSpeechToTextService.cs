using System;
using System.IO;
using System.Collections;
using HTTP;
using UnityEngine;
using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Google non-streaming speech-to-text SDK.
    /// </summary>
    public class GoogleNonStreamingSpeechToTextService : NonStreamingSpeechToTextService
    {
        /// <summary>
        /// Component used to manage temporary audio files
        /// </summary>
        TempAudioFileSavingComponent m_TempAudioComponent = new TempAudioFileSavingComponent("GoogleNonStreamingAudio");
        /// <summary>
        /// Store for APIKey property
        /// </summary>
        [SerializeField]
        string m_APIKey;

        /// <summary>
        /// Key used to authenticate requests to the Google API
        /// </summary>
        public string APIKey { set { m_APIKey = value; } }

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

            // Save recorded audio to a WAV file and convert it to FLAC format.
            string wavAudioFilePath = SavWav.Save(m_TempAudioComponent.TempAudioRelativePath(), AudioRecordingManager.Instance.RecordedAudio);
            string flacAudioFilePath = IOUtilities.MakeFilePathUnique(Path.ChangeExtension(wavAudioFilePath, "flac"));
            SmartLogger.Log(DebugFlags.GoogleNonStreamingSpeechToText, "converting audio");
            var audioConversionJob = new SoXAudioConversionJob(wavAudioFilePath, flacAudioFilePath, 16000);
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
            
            var request = new Request("POST", Constants.GoogleNonStreamingSpeechToTextURL +
                "?" + Constants.GoogleAPIKeyParameterName + "=" + m_APIKey);
            request.headers.Add("Content-Type", "application/json");

            // Construct JSON request body.
            JSONObject requestJSON = new JSONObject();
            JSONObject requestConfig = new JSONObject();
            requestConfig.AddField(Constants.GoogleRequestJSONConfigEncodingFieldKey, "FLAC");
            requestConfig.AddField(Constants.GoogleRequestJSONConfigSampleRateFieldKey, "16000");
            JSONObject requestAudio = new JSONObject();
            requestAudio.AddField(Constants.GoogleRequestJSONAudioContentFieldKey, Convert.ToBase64String(File.ReadAllBytes(flacAudioFilePath)));
            requestJSON.AddField(Constants.GoogleRequestJSONConfigFieldKey, requestConfig);
            requestJSON.AddField(Constants.GoogleRequestJSONAudioFieldKey, requestAudio);

            request.Text = requestJSON.ToString();
            request.Send();
            SmartLogger.Log(DebugFlags.GoogleNonStreamingSpeechToText, "sent request");

            while (!request.isDone)
            {
                yield return null;
            }

            // Grab the response JSON once the request is done and parse it.
            var responseJSON = new JSONObject(request.response.Text, int.MaxValue);
            SmartLogger.Log(DebugFlags.GoogleNonStreamingSpeechToText, responseJSON.ToString());

            string errorText = GoogleSpeechToTextResponseJSONParser.GetErrorFromResponseJSON(responseJSON);
            if (errorText != null)
            {
                if (m_OnError != null)
                {
                    m_OnError(errorText);
                }
            }

            SpeechToTextResult textResult;
            JSONObject resultsJSON = responseJSON.GetField(Constants.GoogleResponseJSONResultsFieldKey);
            if (resultsJSON != null && resultsJSON.Count > 0)
            {
                JSONObject resultJSON = resultsJSON[0];
                textResult = GoogleSpeechToTextResponseJSONParser.GetTextResultFromResultJSON(resultJSON);
            }
            else
            {
                textResult = GoogleSpeechToTextResponseJSONParser.GetDefaultGoogleSpeechToTextResult();
            }
            if (m_OnTextResult != null)
            {
                m_OnTextResult(textResult);
            }

            m_TempAudioComponent.ClearTempAudioFiles();
        }
    }
}
