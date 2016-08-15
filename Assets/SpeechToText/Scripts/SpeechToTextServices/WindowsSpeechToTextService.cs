using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Windows streaming speech-to-text SDK.
    /// </summary>
    public class WindowsSpeechToTextService : SpeechToTextService
    {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WINRT || UNITY_WINRT_8_0 || UNITY_WINRT_8_1 || UNITY_WINRT_10_0)
        /// <summary>
        /// Speech recognizer
        /// </summary>
        DictationRecognizer m_DictationRecognizer;
        /// <summary>
        /// The last speech-to-text result that was processed
        /// </summary>
        protected SpeechToTextResult m_LastResult;
        /// <summary>
        /// Store for MinimumConfidence property
        /// </summary>
        [SerializeField]
        ConfidenceLevel m_MinimumConfidence = ConfidenceLevel.High;
        /// <summary>
        /// Store for SessionTimeoutAfterDoneRecording property
        /// </summary>
        [SerializeField]
        float m_SessionTimeoutAfterDoneRecording = 2.0f;

        /// <summary>
        /// Minimum confidence level a text result needs for it to be recognized
        /// </summary>
        public ConfidenceLevel MinimumConfidence { set { m_MinimumConfidence = value; } }
        /// <summary>
        /// Number of seconds after recording to wait until the session times out
        /// </summary>
        public float SessionTimeoutAfterDoneRecording { set { m_SessionTimeoutAfterDoneRecording = value; } }

        /// <summary>
        /// Initialization function called on the frame when the script is enabled just before any of the Update
        /// methods is called the first time.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            m_DictationRecognizer = new DictationRecognizer(m_MinimumConfidence, DictationTopicConstraint.Dictation);
            m_DictationRecognizer.InitialSilenceTimeoutSeconds = float.MaxValue;
            m_DictationRecognizer.DictationResult += OnDictationResult;
            m_DictationRecognizer.DictationHypothesis += OnDictationHypothesis;
            m_DictationRecognizer.DictationComplete += OnDictationComplete;
            m_DictationRecognizer.DictationError += OnDictationError;
        }

        /// <summary>
        /// Starts recording audio if the service is not already recording.
        /// </summary>
        /// <returns>Whether the service successfully started recording</returns>
        public override bool StartRecording()
        {
            if (base.StartRecording())
            {
                m_LastResult = new SpeechToTextResult("", false);
                m_DictationRecognizer.Start();
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
                m_DictationRecognizer.Stop();
                StartCoroutine(FinishSession());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Waits until the last processed result is a final result.
        /// If this does not happen before the timeout, the last result is treated as a final result.
        /// </summary>
        /// <returns></returns>
        IEnumerator FinishSession()
        {
            SmartLogger.Log(DebugFlags.WindowsSpeechToText, "finish session");

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
                SmartLogger.Log(DebugFlags.WindowsSpeechToText, "treat last interim result as final");
                m_LastResult.IsFinal = true;
                if (m_OnTextResult != null)
                {
                    m_OnTextResult(m_LastResult);
                }
            }
        }

        /// <summary>
        /// Function that is called when a final text result is received.
        /// </summary>
        /// <param name="text">The final text result</param>
        /// <param name="confidence">Confidence level of the text result</param>
        void OnDictationResult(string text, ConfidenceLevel confidence)
        {
            SmartLogger.LogFormat(DebugFlags.WindowsSpeechToText, "Dictation result: {0}", text);
            m_LastResult = new SpeechToTextResult(text, true);
            if (m_OnTextResult != null)
            {
                m_OnTextResult(m_LastResult);
            }
        }

        /// <summary>
        /// Function that is called when an interim text result is received.
        /// </summary>
        /// <param name="text">The interim text result</param>
        void OnDictationHypothesis(string text)
        {
            SmartLogger.LogFormat(DebugFlags.WindowsSpeechToText, "Dictation hypothesis: {0}", text);
            m_LastResult = new SpeechToTextResult(text, false);
            if (m_OnTextResult != null)
            {
                m_OnTextResult(m_LastResult);
            }
        }

        /// <summary>
        /// Function that is called when dictation has completed.
        /// </summary>
        /// <param name="cause">Cause for completion</param>
        void OnDictationComplete(DictationCompletionCause cause)
        {
            if (cause != DictationCompletionCause.Complete)
            {
                if (cause == DictationCompletionCause.TimeoutExceeded)
                {
                    OnRecordingTimeout();
                }
                else
                {
                    if (m_OnError != null)
                    {
                        m_OnError("Dictation completed unsuccessfully - " + cause);
                    }
                }
            }
        }

        /// <summary>
        /// Function that is called when an error occurs.
        /// </summary>
        /// <param name="error">Error text</param>
        /// <param name="hresult">Error code</param>
        void OnDictationError(string error, int hresult)
        {
            if (m_OnError != null)
            {
                m_OnError("Dictation error - " + error + "; HResult = " + hresult);
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
            SmartLogger.LogError(DebugFlags.WindowsSpeechToText, "This service is only supported on Windows.");
        }
#endif
    }
}
