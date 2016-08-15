using System;
using System.Collections;
using UnityEngine;

namespace UnitySpeechToText.Utilities
{
    /// <summary>
    /// Manager for recording audio.
    /// </summary>
    public class AudioRecordingManager : MonoSingleton<AudioRecordingManager>
    {
        /// <summary>
        /// Store for RecordingFrequency property
        /// </summary>
        [SerializeField]
        int m_RecordingFrequency = 44100;
        /// <summary>
        /// Store for MaxRecordingLengthInSeconds property
        /// </summary>
        [SerializeField]
        int m_MaxRecordingLengthInSeconds = 15;
        /// <summary>
        /// Time at which the most recent recording started
        /// </summary>
        float m_RecordingStartTime;
        /// <summary>
        /// Whether the current recording session ended forcefully rather than by timeout
        /// </summary>
        bool m_ForcedStopRecording;
        /// <summary>
        /// Store for RecordedAudio property
        /// </summary>
        AudioClip m_RecordedAudio;
        /// <summary>
        /// Delegate for recording timeout
        /// </summary>
        Action m_OnTimeout;

        /// <summary>
        /// Frequency (samples-per-second) at which to record
        /// </summary>
        public int RecordingFrequency { set { m_RecordingFrequency = value; } }
        /// <summary>
        /// Number of seconds to record before the recording times out
        /// </summary>
        public int MaxRecordingLengthInSeconds { set { m_MaxRecordingLengthInSeconds = value; } }
        /// <summary>
        /// Audio clip created from the most recent recording
        /// </summary>
        public AudioClip RecordedAudio { get { return m_RecordedAudio; } }

        /// <summary>
        /// Class constructor. Because this class inherits from MonoSingleton, ordinary construction must be prevented.
        /// </summary>
        protected AudioRecordingManager() { }

        /// <summary>
        /// Adds a function to the timeout delegate.
        /// </summary>
        /// <param name="action">Function to register</param>
        public void RegisterOnTimeout(Action action)
        {
            m_OnTimeout += action;
        }

        /// <summary>
        /// Removes a function from the timeout delegate.
        /// </summary>
        /// <param name="action">Function to unregister</param>
        public void UnregisterOnTimeout(Action action)
        {
            m_OnTimeout -= action;
        }

        /// <summary>
        /// Queries if the default device is currently recording.
        /// </summary>
        /// <returns>Whether the default device is currently recording</returns>
        public bool IsRecording()
        {
            return Microphone.IsRecording(null);
        }

        /// <summary>
        /// Waits for the default device to stop recording and checks if this was due to a timeout.
        /// </summary>
        IEnumerator WaitForRecordingTimeout()
        {
            while (Microphone.IsRecording(null))
            {
                yield return null;
            }
            if (!m_ForcedStopRecording)
            {
                if (m_OnTimeout != null)
                {
                    m_OnTimeout();
                }
            }
        }

        /// <summary>
        /// Starts a recording session and waits until it finishes.
        /// </summary>
        public IEnumerator RecordAndWaitUntilDone()
        {
            StartRecording();
            while (IsRecording())
            {
                yield return null;
            }
        }

        /// <summary>
        /// Tells the default device to start recording if it is not already.
        /// </summary>
        public void StartRecording()
        {
            if (!Microphone.IsRecording(null))
            {
                m_ForcedStopRecording = false;
                m_RecordingStartTime = Time.time;
                m_RecordedAudio = Microphone.Start(null, false, m_MaxRecordingLengthInSeconds, m_RecordingFrequency);
                StartCoroutine(WaitForRecordingTimeout());
            }
        }

        /// <summary>
        /// If the default device is recording, ends the recording session and trims the default audio clip produced.
        /// </summary>
        public void StopRecording()
        {
            if (Microphone.IsRecording(null))
            {
                m_ForcedStopRecording = true;
                Microphone.End(null);
                float recordingLengthInSeconds = Time.time - m_RecordingStartTime;
                SmartLogger.Log(DebugFlags.AudioRecordingManager, "Unity mic recording length: " + recordingLengthInSeconds + " seconds");

                // Trim the default audio clip produced by UnityEngine.Microphone to fit the actual recording length.
                var samples = new float[Mathf.CeilToInt(m_RecordedAudio.frequency * recordingLengthInSeconds)];
                m_RecordedAudio.GetData(samples, 0);
                m_RecordedAudio = AudioClip.Create("TrimmedAudio", samples.Length,
                    m_RecordedAudio.channels, m_RecordedAudio.frequency, false);
                m_RecordedAudio.SetData(samples, 0);
            }
        }

        /// <summary>
        /// Creates and returns a specific chunk of audio from the current recording.
        /// </summary>
        /// <param name="offsetInSeconds">Number of seconds from the start of the recording at which the chunk begins</param>
        /// <param name="chunkLengthInSeconds">Maximum number of seconds the audio chunk should be</param>
        /// <returns>The audio chunk, or null if the chunk length is less than or equal to 0 or if
        /// the offset is greater than or equal to the recorded audio length</returns>
        public AudioClip GetChunkOfRecordedAudio(float offsetInSeconds, float chunkLengthInSeconds)
        {
            // Check for nonsense parameters.
            if (chunkLengthInSeconds <= 0)
            {
                SmartLogger.LogError(DebugFlags.AudioRecordingManager, "Audio chunk length cannot be less than or equal to 0.");
                return null;
            }
            if (offsetInSeconds >= m_RecordedAudio.length)
            {
                SmartLogger.LogError(DebugFlags.AudioRecordingManager, "Offset cannot be greater than or equal to the recorded audio length.");
                return null;
            }

            // Check for parameters that can be clamped.
            if (offsetInSeconds < 0)
            {
                SmartLogger.LogWarning(DebugFlags.AudioRecordingManager, "Chunk offset is less than 0. Clamping to 0.");
                offsetInSeconds = 0;
            }
            if (offsetInSeconds + chunkLengthInSeconds > m_RecordedAudio.length)
            {
                SmartLogger.LogWarning(DebugFlags.AudioRecordingManager, 
                    "Chunk offset + length is greater than the recorded audio length. Clamping chunk length.");
                chunkLengthInSeconds = m_RecordedAudio.length - offsetInSeconds;
            }

            // The audio chunk will have a number of samples equal to [chunk length] * [audio frequency (samples-per-second)].
            var samples = new float[Mathf.CeilToInt(m_RecordedAudio.frequency * chunkLengthInSeconds)];

            // Grab samples from the recorded audio starting from the appropriate offset.
            m_RecordedAudio.GetData(samples, Mathf.FloorToInt(m_RecordedAudio.frequency * offsetInSeconds));

            // Create a new AudioClip with these samples.
            AudioClip audioChunk = AudioClip.Create("AudioChunk", samples.Length,
                m_RecordedAudio.channels, m_RecordedAudio.frequency, false);
            audioChunk.SetData(samples, 0);
            return audioChunk;
        }
    }
}
