using System.Collections;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using UnityEngine;
using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// IBM Watson non-streaming speech-to-text SDK.
    /// </summary>
    public class WatsonNonStreamingSpeechToTextService : NonStreamingSpeechToTextService
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
        }

        /// <summary>
        /// Translates speech to text by making a request to the speech-to-text API.
        /// </summary>
        protected override IEnumerator TranslateRecordingToText()
        {
            m_WatsonSpeechToTextComponent.WatsonSpeechToTextService.Recognize(AudioRecordingManager.Instance.RecordedAudio, OnSpeechToTextResult);
            yield return null;
        }

        /// <summary>
        /// Function that is called when the Watson API returns a result.
        /// </summary>
        /// <param name="results">List of speech-to-text results</param>
        void OnSpeechToTextResult(SpeechResultList results)
        {
            if (results.HasResult())
            {
                if (m_OnTextResult != null)
                {
                    m_OnTextResult(m_WatsonSpeechToTextComponent.CreateSpeechToTextResult(results.Results[0]));
                }
            }
            else
            {
                m_OnTextResult(new SpeechToTextResult("", true));
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
