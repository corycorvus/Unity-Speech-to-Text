using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Base class for streaming speech-to-text SDK.
    /// </summary>
    public abstract class StreamingSpeechToTextService : SpeechToTextService
    {
        /// <summary>
        /// Queue of audio chunks to send to the server
        /// </summary>
        protected Queue<AudioClip> m_AudioChunksQueue = new Queue<AudioClip>();
        /// <summary>
        /// The last speech-to-text result that was processed
        /// </summary>
        protected SpeechToTextResult m_LastResult;
        /// <summary>
        /// Store for SessionTimeoutAfterDoneRecording property
        /// </summary>
        [SerializeField]
        protected float m_SessionTimeoutAfterDoneRecording = 2.0f;
        /// <summary>
        /// Store for AudioChunkLengthInSeconds property
        /// </summary>
        [SerializeField]
        protected float m_AudioChunkLengthInSeconds;

        /// <summary>
        /// Number of seconds after recording to wait until the session times out
        /// </summary>
        public float SessionTimeoutAfterDoneRecording { set { m_SessionTimeoutAfterDoneRecording = value; } }
        /// <summary>
        /// Length (in seconds) of each chunk of recorded audio to send to the server
        /// </summary>
        public float AudioChunkLengthInSeconds { set { m_AudioChunkLengthInSeconds = value; } }

        /// <summary>
        /// Starts recording audio if the service is not already recording.
        /// </summary>
        /// <returns>Whether the service successfully started recording</returns>
        public override bool StartRecording()
        {
            if (base.StartRecording())
            {
                m_LastResult = new SpeechToTextResult("", false);
                StartCoroutine(RecordAudio());
                StartCoroutine(StreamAudioAndListenForResponses());
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
        /// Records audio and queues fixed audio chunks.
        /// </summary>
        IEnumerator RecordAudio()
        {
            AudioRecordingManager.Instance.StartRecording();
            float chunkStartTime = 0;
            while (m_IsRecording)
            {
                float startTime = Time.time;
                while (AudioRecordingManager.Instance.IsRecording() && Time.time - startTime < m_AudioChunkLengthInSeconds)
                {
                    yield return null;
                }
                float chunkLengthInSeconds = Time.time - startTime;
                if (chunkLengthInSeconds > 0)
                {
                    m_AudioChunksQueue.Enqueue(AudioRecordingManager.Instance.GetChunkOfRecordedAudio(chunkStartTime, chunkLengthInSeconds));
                    chunkStartTime += chunkLengthInSeconds;
                }
            }
        }

        /// <summary>
        /// Sends queued chunks of audio to the server and then waits for the transcription(s).
        /// </summary>
        protected abstract IEnumerator StreamAudioAndListenForResponses();
    }
}
