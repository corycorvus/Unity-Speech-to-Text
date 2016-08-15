using System;
using UnityEngine;
using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Base class for speech-to-text SDK.
    /// </summary>
    public class SpeechToTextService : MonoBehaviour
    {
        /// <summary>
        /// Store for the IsRecording property
        /// </summary>
        protected bool m_IsRecording;
        /// <summary>
        /// Delegate for text result
        /// </summary>
        protected Action<SpeechToTextResult> m_OnTextResult;
        /// <summary>
        /// Delegate for error
        /// </summary>
        protected Action<string> m_OnError;
        /// <summary>
        /// Delegate for recording timeout
        /// </summary>
        protected Action m_OnRecordingTimeout;

        /// <summary>
        /// Whether the service is recording audio
        /// </summary>
        public bool IsRecording { get { return m_IsRecording; } }

        /// <summary>
        /// Adds a function to the text result delegate.
        /// </summary>
        /// <param name="action">Function to register</param>
        public void RegisterOnTextResult(Action<SpeechToTextResult> action)
        {
            m_OnTextResult += action;
        }

        /// <summary>
        /// Removes a function from the text result delegate.
        /// </summary>
        /// <param name="action">Function to unregister</param>
        public void UnregisterOnTextResult(Action<SpeechToTextResult> action)
        {
            m_OnTextResult -= action;
        }

        /// <summary>
        /// Adds a function to the error delegate.
        /// </summary>
        /// <param name="action">Function to register</param>
        public void RegisterOnError(Action<string> action)
        {
            m_OnError += action;
        }

        /// <summary>
        /// Removes a function from the error delegate.
        /// </summary>
        /// <param name="action">Function to unregister</param>
        public void UnregisterOnError(Action<string> action)
        {
            m_OnError -= action;
        }

        /// <summary>
        /// Adds a function to the recording timeout delegate.
        /// </summary>
        /// <param name="action">Function to register</param>
        public void RegisterOnRecordingTimeout(Action action)
        {
            m_OnRecordingTimeout += action;
        }

        /// <summary>
        /// Removes a function from the recording timeout delegate.
        /// </summary>
        /// <param name="action">Function to unregister</param>
        public void UnregisterOnRecordingTimeout(Action action)
        {
            m_OnRecordingTimeout -= action;
        }

        /// <summary>
        /// Initialization function called on the frame when the script is enabled just before any of the Update
        /// methods is called the first time.
        /// </summary>
        protected virtual void Start()
        {
            AudioRecordingManager.Instance.RegisterOnTimeout(OnRecordingTimeout);
        }

        /// <summary>
        /// Function that is called when the MonoBehaviour will be destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (AudioRecordingManager.Instance != null)
            {
                AudioRecordingManager.Instance.UnregisterOnTimeout(OnRecordingTimeout);
            }
        }

        /// <summary>
        /// Function that is called when the recording times out.
        /// </summary>
        protected void OnRecordingTimeout()
        {
            m_IsRecording = false;
            if (m_OnRecordingTimeout != null)
            {
                m_OnRecordingTimeout();
            }
        }

        /// <summary>
        /// Starts recording audio if the service is not already recording.
        /// </summary>
        /// <returns>Whether the service successfully started recording</returns>
        public virtual bool StartRecording()
        {
            if (!m_IsRecording)
            {
                m_IsRecording = true;
                StopAllCoroutines();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops recording audio if the service is already recording.
        /// </summary>
        /// <returns>Whether the service successfully stopped recording</returns>
        public virtual bool StopRecording()
        {
            if (m_IsRecording)
            {
                m_IsRecording = false;
                return true;
            }
            return false;
        }
    }
}
