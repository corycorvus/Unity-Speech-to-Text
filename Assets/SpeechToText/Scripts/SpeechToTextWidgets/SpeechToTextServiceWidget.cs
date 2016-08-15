using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnitySpeechToText.Services;
using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Widgets
{
    /// <summary>
    /// Widget that handles interaction with a specific speech-to-text API.
    /// </summary>
    public class SpeechToTextServiceWidget : MonoBehaviour
    {
        /// <summary>
        /// Store for InterimTextResultColor property
        /// </summary>
        [SerializeField]
        Color m_InterimTextResultColor = Color.gray;
        /// <summary>
        /// Store for FinalTextResultColor property
        /// </summary>
        [SerializeField]
        Color m_FinalTextResultColor = Color.black;
        /// <summary>
        /// Store for ErrorTextColor property
        /// </summary>
        [SerializeField]
        Color m_ErrorTextColor = Color.red;
        /// <summary>
        /// Store for SpeechToTextService property
        /// </summary>
        [SerializeField]
        SpeechToTextService m_SpeechToTextService;
        /// <summary>
        /// Store for ResultsScrollRect property
        /// </summary>
        [SerializeField]
        ScrollRect m_ResultsScrollRect;
        /// <summary>
        /// Rectangle transform for the results scroll view content
        /// </summary>
        RectTransform m_ScrollViewContentRect;
        /// <summary>
        /// Text UI for speech-to-text results
        /// </summary>
        Text m_ResultsTextUI;
        /// <summary>
        /// Combined top and bottom padding for results text
        /// </summary>
        float m_VerticalTextPadding;
        /// <summary>
        /// The height of a single line of text
        /// </summary>
        float m_TextLineHeight;
        /// <summary>
        /// The minimum height that the scroll view content rectangle can be
        /// </summary>
        float m_MinScrollViewContentHeight;
        /// <summary>
        /// Whether the initial scroll view content dimensions have been determined so the proper dimensions can be calculated
        /// </summary>
        bool m_ReadyToFitScrollViewContentToText;
        /// <summary>
        /// All the final results that have already been determined in the current recording session
        /// </summary>
        string m_PreviousFinalResults;
        /// <summary>
        /// Whether this object has stopped recording and is waiting for the last final text result of the current session.
        /// Here "last" means that there will be no more results this session, and "final" means the result is fixed
        /// ("final" is used for the sake of consistency).
        /// </summary>
        bool m_WaitingForLastFinalResultOfSession;
        /// <summary>
        /// Whether the last speech-to-text result received was a final (rather than interim) result
        /// </summary>
        bool m_LastResultWasFinal;
        /// <summary>
        /// Whether this widget will display the speech-to-text results that it receives
        /// </summary>
        bool m_WillDisplayReceivedResults;
        /// <summary>
        /// Text to compare the final speech-to-text result against
        /// </summary>
        string m_ComparisonPhrase;
        /// <summary>
        /// Set of leading characters for words to ignore when computing accuracy, which includes '%' by default
        /// to account for Watson's "%HESITATION" in results
        /// </summary>
        HashSet<char> m_LeadingCharsForSpecialWords = new HashSet<char> { '%' };
        /// <summary>
        /// Set of surrounding characters for text to ignore when computing accuracy, which includes brackets
        /// by default to account for instructions in speech such as "[pause]"
        /// </summary>
        Dictionary<char, char> m_SurroundingCharsForSpecialText = new Dictionary<char, char> { { '[', ']' } };
        /// <summary>
        /// Delegate for recording timeout
        /// </summary>
        Action m_OnRecordingTimeout;
        /// <summary>
        /// Delegate for receiving the last text result
        /// </summary>
        Action<SpeechToTextServiceWidget> m_OnReceivedLastResponse;

        /// <summary>
        /// Color to use for interim text results
        /// </summary>
        public Color InterimTextResultColor { set { m_InterimTextResultColor = value; } }
        /// <summary>
        /// Color to use for final text results
        /// </summary>
        public Color FinalTextResultColor { set { m_FinalTextResultColor = value; } }
        /// <summary>
        /// Color to use for errors
        /// </summary>
        public Color ErrorTextColor { set { m_ErrorTextColor = value; } }

        /// <summary>
        /// The specific speech-to-text service to use
        /// </summary>
        public SpeechToTextService SpeechToTextService
        {
            set
            {
                m_SpeechToTextService = value;
                RegisterSpeechToTextServiceCallbacks();
            }
        }

        /// <summary>
        /// Scroll rectangle containing text results
        /// </summary>
        public ScrollRect ResultsScrollRect
        {
            set
            {
                m_ResultsScrollRect = value;
                SetResultsScrollRectChildComponents();
            }
        }

        /// <summary>
        /// Adds a function to the recording timeout delegate.
        /// </summary>
        /// <param name="action">Function to register</param>
        public void RegisterOnRecordingTimeout(Action action)
        {
            SmartLogger.Log(DebugFlags.SpeechToTextWidgets, SpeechToTextServiceString() + " register timeout");
            m_OnRecordingTimeout += action;
        }

        /// <summary>
        /// Removes a function from the recording timeout delegate.
        /// </summary>
        /// <param name="action">Function to unregister</param>
        public void UnregisterOnRecordingTimeout(Action action)
        {
            SmartLogger.Log(DebugFlags.SpeechToTextWidgets, SpeechToTextServiceString() + " unregister timeout");
            m_OnRecordingTimeout -= action;
        }

        /// <summary>
        /// Adds a function to the received last response delegate.
        /// </summary>
        /// <param name="action">Function to register</param>
        public void RegisterOnReceivedLastResponse(Action<SpeechToTextServiceWidget> action)
        {
            m_OnReceivedLastResponse += action;
        }

        /// <summary>
        /// Removes a function from the received last response delegate.
        /// </summary>
        /// <param name="action">Function to unregister</param>
        public void UnregisterOnReceivedLastResponse(Action<SpeechToTextServiceWidget> action)
        {
            m_OnReceivedLastResponse -= action;
        }

        /// <summary>
        /// Initialization function called on the frame when the script is enabled just before any of the Update
        /// methods is called the first time.
        /// </summary>
        void Start()
        {
            RegisterSpeechToTextServiceCallbacks();
            SetResultsScrollRectChildComponents();
        }

        /// <summary>
        /// Function that is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            // The scroll view content dimensions are not calculated upon initialization so Update must check for this.
            if (m_ReadyToFitScrollViewContentToText)
            {
                FitScrollViewContentToText();
            }
            else if (m_ScrollViewContentRect != null && m_ScrollViewContentRect.rect.height > 0)
            {
                m_MinScrollViewContentHeight = m_ScrollViewContentRect.rect.height;
                m_ReadyToFitScrollViewContentToText = true;
            }
        }

        /// <summary>
        /// Function that is called when the MonoBehaviour will be destroyed.
        /// </summary>
        void OnDestroy()
        {
            UnregisterSpeechToTextServiceCallbacks();
        }

        /// <summary>
        /// Finds child components for the results scroll rect and assigns them to the appropriate member variables.
        /// Also determines additional text UI member variables.
        /// </summary>
        void SetResultsScrollRectChildComponents()
        {
            if (m_ResultsScrollRect != null)
            {
                Transform contentTransform = m_ResultsScrollRect.GetComponent<RectTransform>().Find("Viewport/Content");
                m_ScrollViewContentRect = contentTransform.GetComponent<RectTransform>();
                m_ResultsTextUI = m_ScrollViewContentRect.GetComponentInChildren<Text>();

                // Determine the vertical text padding and line height upon initialization
                // so that the scroll view content rectangle can later be resized based on these.
                m_VerticalTextPadding = m_ScrollViewContentRect.rect.height *
                    (m_ResultsTextUI.rectTransform.anchorMin.y + 1 - m_ResultsTextUI.rectTransform.anchorMax.y);
                m_TextLineHeight = LayoutUtility.GetPreferredHeight(m_ResultsTextUI.rectTransform);
            }
        }

        /// <summary>
        /// Registers callbacks with the SpeechToTextService.
        /// </summary>
        void RegisterSpeechToTextServiceCallbacks()
        {
            if (m_SpeechToTextService != null)
            {
                m_SpeechToTextService.RegisterOnError(OnSpeechToTextError);
                m_SpeechToTextService.RegisterOnTextResult(OnTextResult);
                m_SpeechToTextService.RegisterOnRecordingTimeout(OnSpeechToTextRecordingTimeout);
            }
        }

        /// <summary>
        /// Unregisters callbacks with the SpeechToTextService.
        /// </summary>
        void UnregisterSpeechToTextServiceCallbacks()
        {
            if (m_SpeechToTextService != null)
            {
                m_SpeechToTextService.UnregisterOnError(OnSpeechToTextError);
                m_SpeechToTextService.UnregisterOnTextResult(OnTextResult);
                m_SpeechToTextService.UnregisterOnRecordingTimeout(OnSpeechToTextRecordingTimeout);
            }
        }

        /// <summary>
        /// Returns a string representation of the type of speech-to-text service used by this object.
        /// </summary>
        /// <returns>String representation of the type of speech-to-text service used by this object</returns>
        public string SpeechToTextServiceString()
        {
            return m_SpeechToTextService.GetType().ToString();
        }

        /// <summary>
        /// Resizes the scroll view content rectangle so that it can fit the results text.
        /// </summary>
        void FitScrollViewContentToText()
        {
            float textHeight = LayoutUtility.GetPreferredHeight(m_ResultsTextUI.rectTransform);
            float parentHeight = m_ScrollViewContentRect.rect.height;
            if (textHeight + m_VerticalTextPadding >= parentHeight)
            {
                m_ScrollViewContentRect.offsetMin = new Vector2(m_ScrollViewContentRect.offsetMin.x, m_ScrollViewContentRect.offsetMin.y - m_TextLineHeight);
                m_ResultsScrollRect.verticalNormalizedPosition = 0;
            }
            else if (textHeight + m_VerticalTextPadding < parentHeight - m_TextLineHeight && parentHeight - m_TextLineHeight >= m_MinScrollViewContentHeight)
            {
                m_ScrollViewContentRect.offsetMin = new Vector2(m_ScrollViewContentRect.offsetMin.x, m_ScrollViewContentRect.offsetMin.y + m_TextLineHeight);
                m_ResultsScrollRect.verticalNormalizedPosition = 0;
            }
        }

        /// <summary>
        /// Clears the current results text and tells the speech-to-text service to start recording.
        /// </summary>
        public void StartRecording()
        {
            SmartLogger.Log(DebugFlags.SpeechToTextWidgets, "Start service widget recording");
            m_WillDisplayReceivedResults = true;
            m_WaitingForLastFinalResultOfSession = false;
            m_LastResultWasFinal = false;
            m_PreviousFinalResults = "";
            m_ResultsTextUI.text = m_PreviousFinalResults;
            m_SpeechToTextService.StartRecording();
        }

        /// <summary>
        /// Starts waiting for the last text result and tells the speech-to-text service to stop recording.
        /// If a streaming speech-to-text service stops recording and the last result sent by it was not already final,
        /// the service is guaranteed to send a final result or error after or before some defined amount of time has passed.
        /// </summary>
        /// <param name="comparisonPhrase">Optional text to compare the speech-to-text result against</param>
        public void StopRecording(string comparisonPhrase = null)
        {
            m_ComparisonPhrase = comparisonPhrase;
            if (m_LastResultWasFinal)
            {
                ProcessEndResults();
            }
            else
            {
                m_WaitingForLastFinalResultOfSession = true;
            }
            m_SpeechToTextService.StopRecording();
        }

        /// <summary>
        /// Function that is called when a speech-to-text result is received. If it is a final result and this widget
        /// is waiting for the last result of the session, then the widget will begin processing the end results
        /// of the session.
        /// </summary>
        /// <param name="result">The speech-to-text result</param>
        void OnTextResult(SpeechToTextResult result)
        {
            if (m_WillDisplayReceivedResults)
            {
                // For the purposes of comparing results, this just uses the first alternative
                m_LastResultWasFinal = result.IsFinal;
                if (result.IsFinal)
                {
                    m_PreviousFinalResults += result.TextAlternatives[0].Text;
                    m_ResultsTextUI.color = m_FinalTextResultColor;
                    m_ResultsTextUI.text = m_PreviousFinalResults;
                    SmartLogger.Log(DebugFlags.SpeechToTextWidgets, m_SpeechToTextService.GetType().ToString() + " final result");
                    if (m_WaitingForLastFinalResultOfSession)
                    {
                        m_WaitingForLastFinalResultOfSession = false;
                        ProcessEndResults();
                    }
                }
                else
                {
                    m_ResultsTextUI.color = m_InterimTextResultColor;
                    m_ResultsTextUI.text = m_PreviousFinalResults + result.TextAlternatives[0].Text;
                }
            }
        }

        /// <summary>
        /// Does any final processing necessary for the results of the last started session and then
        /// stops the widget from displaying results until the start of the next session.
        /// </summary>
        void ProcessEndResults()
        {
            SmartLogger.Log(DebugFlags.SpeechToTextWidgets, m_SpeechToTextService.GetType().ToString() + " got last response");
            if (m_ComparisonPhrase != null)
            {
                DisplayAccuracyOfEndResults(m_ComparisonPhrase);
            }
            LogFileManager.Instance.WriteTextToFileIfShouldLog(SpeechToTextServiceString() + ": " + m_ResultsTextUI.text);
            if (m_OnReceivedLastResponse != null)
            {
                m_OnReceivedLastResponse(this);
            }
            m_WillDisplayReceivedResults = false;
        }

        /// <summary>
        /// Computes the accuracy (percentage) of the end text results in comparison to the given phrase, by using 
        /// the Levenshtein Distance between the two strings, and displays this percentage in the results text UI.
        /// </summary>
        /// <param name="originalPhrase">The phrase to compare against</param>
        void DisplayAccuracyOfEndResults(string originalPhrase)
        {
            string speechToTextResult = StringUtilities.TrimSpecialFormatting(m_ResultsTextUI.text, new HashSet<char>(),
                m_LeadingCharsForSpecialWords, m_SurroundingCharsForSpecialText);
            originalPhrase = StringUtilities.TrimSpecialFormatting(originalPhrase, new HashSet<char>(),
                m_LeadingCharsForSpecialWords, m_SurroundingCharsForSpecialText);

            int levenDistance = StringUtilities.LevenshteinDistance(speechToTextResult, originalPhrase);
            SmartLogger.Log(DebugFlags.SpeechToTextWidgets, m_SpeechToTextService.GetType().ToString() + " compute accuracy of text: \"" + speechToTextResult + "\"");
            float accuracy = Mathf.Max(0, 100f - (100f * (float)levenDistance / (float)originalPhrase.Length));
            m_PreviousFinalResults = "[Accuracy: " + accuracy + "%] " + m_PreviousFinalResults;
            m_ResultsTextUI.text = m_PreviousFinalResults;
        }

        /// <summary>
        /// Function that is called when an error occurs. If this object is waiting for
        /// a last response, then this error is treated as the last "result" of the current session.
        /// </summary>
        /// <param name="text">The error text</param>
        void OnSpeechToTextError(string text)
        {
            SmartLogger.LogError(DebugFlags.SpeechToTextWidgets, SpeechToTextServiceString() + " error: " + text);
            if (m_WillDisplayReceivedResults)
            {
                m_PreviousFinalResults += "[Error: " + text + "] ";
                m_ResultsTextUI.color = m_ErrorTextColor;
                m_ResultsTextUI.text = m_PreviousFinalResults;
                if (m_WaitingForLastFinalResultOfSession)
                {
                    m_WaitingForLastFinalResultOfSession = false;
                    if (m_OnReceivedLastResponse != null)
                    {
                        m_OnReceivedLastResponse(this);
                    }
                }
            }
        }

        /// <summary>
        /// Function that is called when the recording times out.
        /// </summary>
        void OnSpeechToTextRecordingTimeout()
        {
            SmartLogger.Log(DebugFlags.SpeechToTextWidgets, SpeechToTextServiceString() + " call timeout");
            if (m_OnRecordingTimeout != null)
            {
                m_OnRecordingTimeout();
            }
        }
    }
}
