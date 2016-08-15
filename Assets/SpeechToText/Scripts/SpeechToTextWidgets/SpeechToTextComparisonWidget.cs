using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Widgets
{
    /// <summary>
    /// Widget that handles the side-by-side comparison of different speech-to-text services.
    /// </summary>
    public class SpeechToTextComparisonWidget : MonoBehaviour
    {
        /// <summary>
        /// Store for PhrasesToggleGroup property
        /// </summary>
        [SerializeField]
        ToggleGroup m_PhrasesToggleGroup;
        /// <summary>
        /// Store for RecordingText property
        /// </summary>
        [SerializeField]
        string m_RecordingText;
        /// <summary>
        /// Store for NotRecordingText property
        /// </summary>
        [SerializeField]
        string m_NotRecordingText;
        /// <summary>
        /// Store for RecordingButtonColor property
        /// </summary>
        [SerializeField]
        Color m_RecordingButtonColor = Color.red;
        /// <summary>
        /// Store for NotRecordingButtonColor property
        /// </summary>
        [SerializeField]
        Color m_NotRecordingButtonColor = Color.white;
        /// <summary>
        /// Store for WaitingForResponsesText property
        /// </summary>
        [SerializeField]
        string m_WaitingForResponsesText;
        /// <summary>
        /// Store for ResponsesTimeoutInSeconds property
        /// </summary>
        [SerializeField]
        float m_ResponsesTimeoutInSeconds = 8f;
        /// <summary>
        /// Store for SpeechToTextServiceWidgets property
        /// </summary>
        [SerializeField]
        SpeechToTextServiceWidget[] m_SpeechToTextServiceWidgets;
        /// <summary>
        /// Store for RecordButton property
        /// </summary>
        [SerializeField]
        Button m_RecordButton;
        /// <summary>
        /// Text UI for the record button
        /// </summary>
        Text m_RecordButtonTextUI;
        /// <summary>
        /// Image for the record button
        /// </summary>
        Image m_RecordButtonImage;
        /// <summary>
        /// Whether the application is currently in a speech-to-text session
        /// </summary>
        bool m_IsCurrentlyInSpeechToTextSession;
        /// <summary>
        /// Whether the application is currently recording audio
        /// </summary>
        bool m_IsRecording;
        /// <summary>
        /// Set of speech-to-text service widgets that are still waiting on a final response
        /// </summary>
        HashSet<SpeechToTextServiceWidget> m_WaitingSpeechToTextServiceWidgets = new HashSet<SpeechToTextServiceWidget>();

        /// <summary>
        /// Toggle group for sample phrases
        /// </summary>
        public ToggleGroup PhrasesToggleGroup { set { m_PhrasesToggleGroup = value; } }
        /// <summary>
        /// Text to display on the record button when recording
        /// </summary>
        public string RecordingText { set { m_RecordingText = value; } }
        /// <summary>
        /// Text to display on the record button when not recording
        /// </summary>
        public string NotRecordingText { set { m_NotRecordingText = value; } }
        /// <summary>
        /// Color of the record button when recording
        /// </summary>
        public Color RecordingButtonColor { set { m_RecordingButtonColor = value; } }
        /// <summary>
        /// Color of the record button when not recording
        /// </summary>
        public Color NotRecordingButtonColor { set { m_NotRecordingButtonColor = value; } }
        /// <summary>
        /// Text to display on the record button when waiting for responses
        /// </summary>
        public string WaitingForResponsesText { set { m_WaitingForResponsesText = value; } }
        /// <summary>
        /// Number of seconds to wait for all responses after recording
        /// </summary>
        public float ResponsesTimeoutInSeconds { set { m_ResponsesTimeoutInSeconds = value; } }

        /// <summary>
        /// Array of speech-to-text service widgets 
        /// </summary>
        public SpeechToTextServiceWidget[] SpeechToTextServiceWidgets
        {
            set
            {
                m_SpeechToTextServiceWidgets = value;
                RegisterSpeechToTextServiceWidgetsCallbacks();
            }
        }

        /// <summary>
        /// Button to start/stop recording
        /// </summary>
        public Button RecordButton
        {
            set
            {
                m_RecordButton = value;
                SetRecordButtonChildComponents();
            }
        }

        /// <summary>
        /// Initialization function called on the frame when the script is enabled just before any of the Update
        /// methods is called the first time.
        /// </summary>
        void Start()
        {
            SetRecordButtonChildComponents();
            RegisterSpeechToTextServiceWidgetsCallbacks();
        }

        /// <summary>
        /// Function that is called when the MonoBehaviour will be destroyed.
        /// </summary>
        void OnDestroy()
        {
            UnregisterSpeechToTextServiceWidgetsCallbacks();
        }

        /// <summary>
        /// Finds child components for the record button and assigns them to the appropriate member variables.
        /// </summary>
        void SetRecordButtonChildComponents()
        {
            if (m_RecordButton != null)
            {
                m_RecordButtonTextUI = m_RecordButton.GetComponentInChildren<Text>();
                m_RecordButtonImage = m_RecordButton.GetComponent<Image>();
            }
        }

        /// <summary>
        /// Registers callbacks with each SpeechToTextServiceWidget.
        /// </summary>
        void RegisterSpeechToTextServiceWidgetsCallbacks()
        {
            if (m_SpeechToTextServiceWidgets != null)
            {
                SmartLogger.Log(DebugFlags.SpeechToTextWidgets, "register service widgets callbacks");
                foreach (var serviceWidget in m_SpeechToTextServiceWidgets)
                {
                    SmartLogger.Log(DebugFlags.SpeechToTextWidgets, "register service widget callbacks");
                    serviceWidget.RegisterOnRecordingTimeout(OnRecordTimeout);
                    serviceWidget.RegisterOnReceivedLastResponse(OnSpeechToTextReceivedLastResponse);
                }
            }
        }

        /// <summary>
        /// Unregisters callbacks with each SpeechToTextServiceWidget.
        /// </summary>
        void UnregisterSpeechToTextServiceWidgetsCallbacks()
        {
            if (m_SpeechToTextServiceWidgets != null)
            {
                SmartLogger.Log(DebugFlags.SpeechToTextWidgets, "unregister service widgets callbacks");
                foreach (var serviceWidget in m_SpeechToTextServiceWidgets)
                {
                    SmartLogger.Log(DebugFlags.SpeechToTextWidgets, "unregister service widget callbacks");
                    serviceWidget.UnregisterOnRecordingTimeout(OnRecordTimeout);
                    serviceWidget.UnregisterOnReceivedLastResponse(OnSpeechToTextReceivedLastResponse);
                }
            }
        }

        /// <summary>
        /// Function that is called when the record button is clicked.
        /// </summary>
        public void OnRecordButtonClicked()
        {
            if (m_IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        /// <summary>
        /// Function that is called when audio recording times out.
        /// </summary>
        void OnRecordTimeout()
        {
            StopRecording();
        }

        /// <summary>
        /// Function that is called when the given SpeechToTextServiceWidget has gotten its last response. If there are no waiting
        /// SpeechToTextServiceWidgets left, then this function will wrap-up the current comparison session.
        /// </summary>
        /// <param name="serviceWidget">The speech-to-text service widget that received a last response</param>
        void OnSpeechToTextReceivedLastResponse(SpeechToTextServiceWidget serviceWidget)
        {
            SmartLogger.Log(DebugFlags.SpeechToTextWidgets, "Response from " + serviceWidget.SpeechToTextServiceString());
            m_WaitingSpeechToTextServiceWidgets.Remove(serviceWidget);
            if (m_WaitingSpeechToTextServiceWidgets.Count == 0)
            {
                SmartLogger.Log(DebugFlags.SpeechToTextWidgets, "Responses from everyone");
                FinishComparisonSession();
            }
        }

        /// <summary>
        /// Starts recording audio for each speech-to-text service widget if not already recording.
        /// </summary>
        void StartRecording()
        {
            if (!m_IsRecording)
            {
                SmartLogger.Log(DebugFlags.SpeechToTextWidgets, "Start comparison recording");
                m_IsCurrentlyInSpeechToTextSession = true;
                m_IsRecording = true;
                m_RecordButtonTextUI.text = m_RecordingText;
                m_RecordButtonImage.color = m_RecordingButtonColor;
                m_WaitingSpeechToTextServiceWidgets.Clear();
                foreach (var serviceWidget in m_SpeechToTextServiceWidgets)
                {
                    SmartLogger.Log(DebugFlags.SpeechToTextWidgets, "tell service widget to start recording");
                    serviceWidget.StartRecording();
                    m_WaitingSpeechToTextServiceWidgets.Add(serviceWidget);
                }
            }
        }

        /// <summary>
        /// Stops recording audio for each speech-to-text service widget if already recording. Also schedules a wrap-up of the
        /// current comparison session to happen after the responses timeout.
        /// </summary>
        void StopRecording()
        {
            if (m_IsRecording)
            {
                m_IsRecording = false;

                // Disable all UI interaction until all responses have been received or after the specified timeout.
                DisableAllUIInteraction();
                m_RecordButtonImage.color = m_NotRecordingButtonColor;
                Invoke("FinishComparisonSession", m_ResponsesTimeoutInSeconds);

                // If a phrase is selected, pass it to the SpeechToTextServiceWidget.
                string comparisonPhrase = null;
                if (m_PhrasesToggleGroup.AnyTogglesOn())
                {
                    IEnumerator<Toggle> toggleEnum = m_PhrasesToggleGroup.ActiveToggles().GetEnumerator();
                    toggleEnum.MoveNext();
                    comparisonPhrase = toggleEnum.Current.gameObject.GetComponentInChildren<Text>().text;
                }

                foreach (var serviceWidget in m_SpeechToTextServiceWidgets)
                {
                    serviceWidget.StopRecording(comparisonPhrase);
                }
            }
        }

        /// <summary>
        /// Wraps up the current speech-to-text comparison session by enabling all UI interaction.
        /// </summary>
        void FinishComparisonSession()
        {
            // If this function is called before the timeout, cancel all invokes so that it is not called again upon timeout.
            CancelInvoke();

            if (m_IsCurrentlyInSpeechToTextSession)
            {
                m_IsCurrentlyInSpeechToTextSession = false;
                EnableAllUIInteraction();
            }
        }

        /// <summary>
        /// Enables interaction with the record button and phrase toggles.
        /// </summary>
        void EnableAllUIInteraction()
        {
            m_RecordButton.interactable = true;
            m_RecordButtonTextUI.text = m_NotRecordingText;
            foreach (var toggle in m_PhrasesToggleGroup.GetComponentsInChildren<Toggle>(true))
            {
                toggle.interactable = true;
            }
        }

        /// <summary>
        /// Disables interaction with the record button and phrase toggles.
        /// </summary>
        void DisableAllUIInteraction()
        {
            m_RecordButton.interactable = false;
            m_RecordButtonTextUI.text = m_WaitingForResponsesText;
            foreach (var toggle in m_PhrasesToggleGroup.GetComponentsInChildren<Toggle>())
            {
                toggle.interactable = false;
            }
        }
    }
}
