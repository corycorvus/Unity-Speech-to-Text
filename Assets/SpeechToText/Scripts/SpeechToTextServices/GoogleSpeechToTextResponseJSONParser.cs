using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Container for helper functionality for Google speech-to-text response JSON parsing.
    /// </summary>
    public static class GoogleSpeechToTextResponseJSONParser
    {
        /// <summary>
        /// Checks a response JSON for an error and returns the error message if it exists.
        /// Otherwise returns null.
        /// </summary>
        /// <param name="responseJSON">Google speech-to-text response JSON object</param>
        /// <returns>Error message if it exists, otherwise null</returns>
        public static string GetErrorFromResponseJSON(JSONObject responseJSON)
        {
            JSONObject error = responseJSON.GetField(Constants.GoogleResponseJSONErrorFieldKey);
            if (error != null)
            {
                string errorText = "";
                int errorCode = -1;
                if (error.GetField(out errorCode, Constants.GoogleResponseJSONErrorCodeFieldKey, errorCode))
                {
                    errorText += "(" + errorCode + ") ";
                }
                string errorMessage = "Unknown error";
                error.GetField(out errorMessage, Constants.GoogleResponseJSONErrorMessageFieldKey, errorMessage);
                errorText += errorMessage;
                return errorText;
            }
            return null;
        }

        /// <summary>
        /// Returns a speech-to-text result object based on information in the result JSON.
        /// </summary>
        /// <param name="resultJSON">Google speech-to-text result JSON object</param>
        /// <returns>Speech-to-text result object</returns>
        public static SpeechToTextResult GetTextResultFromResultJSON(JSONObject resultJSON)
        {
            SpeechToTextResult textResult = null;
            JSONObject alternatives = resultJSON.GetField(Constants.GoogleResponseJSONAlternativesFieldKey);
            if (alternatives != null)
            {
                textResult = new SpeechToTextResult();
                textResult.TextAlternatives = new TextAlternative[alternatives.Count];
                for (int i = 0; i < textResult.TextAlternatives.Length; ++i)
                {
                    var alternative = new GoogleTextAlternative();
                    string text = "";
                    float confidence = 0;
                    alternatives[i].GetField(out text, Constants.GoogleResponseJSONAlternativeTranscriptFieldKey, text);
                    alternatives[i].GetField(out confidence, Constants.GoogleResponseJSONAlternativeConfidenceFieldKey, confidence);
                    alternative.Text = text;
                    alternative.Confidence = confidence;
                    textResult.TextAlternatives[i] = alternative;
                }
            }
            if (textResult == null || textResult.TextAlternatives == null || textResult.TextAlternatives.Length == 0)
            {
                textResult = GetDefaultGoogleSpeechToTextResult();
            }
            return textResult;
        }

        /// <summary>
        /// Returns a speech-to-text result with a single empty Google text alternative.
        /// </summary>
        /// <returns>Default Google speech-to-text result object</returns>
        public static SpeechToTextResult GetDefaultGoogleSpeechToTextResult()
        {
            var textResult = new SpeechToTextResult();
            textResult.TextAlternatives = new TextAlternative[1];
            var alternative = new GoogleTextAlternative();
            alternative.Text = "";
            alternative.Confidence = 0;
            textResult.TextAlternatives[0] = alternative;
            return textResult;
        }
    }
}
