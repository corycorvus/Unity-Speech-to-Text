using System.Collections;
using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Base class for non-streaming speech-to-text SDK.
    /// </summary>
    public abstract class NonStreamingSpeechToTextService : SpeechToTextService
    {
        /// <summary>
        /// Starts recording audio if the service is not already recording.
        /// </summary>
        /// <returns>Whether the service successfully started recording</returns>
        public override bool StartRecording()
        {
            if (base.StartRecording())
            {
                StartCoroutine(RecordAndTranslateToText());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops recording audio if the service is already recording.
        /// </summary>
        /// <returns>Whether the service successfully stopped recording</returns>
        public override bool StopRecording()
        {
            if (base.StopRecording())
            {
                AudioRecordingManager.Instance.StopRecording();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Records audio and translates any speech to text.
        /// </summary>
        IEnumerator RecordAndTranslateToText()
        {
            yield return AudioRecordingManager.Instance.RecordAndWaitUntilDone();
            StartCoroutine(TranslateRecordingToText());
        }

        /// <summary>
        /// Translates speech to text by making a request to the speech-to-text API.
        /// </summary>
        protected abstract IEnumerator TranslateRecordingToText();
    }
}
