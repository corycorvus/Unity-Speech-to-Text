using UnitySpeechToText.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Container for helper functionality for Wit.ai speech-to-text response JSON parsing.
    /// </summary>
    public static class WitAiSpeechToTextResponseJSONParser
    {
        /// <summary>
        /// Checks a response JSON for an error and returns the error message if it exists.
        /// Otherwise returns null.
        /// </summary>
        /// <param name="responseJSON">Wit.ai speech-to-text response JSON object</param>
        /// <returns>Error message if it exists, otherwise null</returns>
        public static string GetErrorFromResponseJSON(JSONObject responseJSON)
        {
            string errorMessage = "Unknown error";
            if (responseJSON.GetField(out errorMessage, Constants.WitAiResponseJSONErrorMessageFieldKey, errorMessage))
            {
                string errorText = "";
                int errorCode = -1;
                if (responseJSON.GetField(out errorCode, Constants.WitAiResponseJSONErrorCodeFieldKey, errorCode))
                {
                    errorText += "(" + errorCode + ") ";
                }
                errorText += errorMessage;
                return errorText;
            }
            return null;
        }

        /// <summary>
        /// Returns a speech-to-text result object based on information in the response JSON.
        /// </summary>
        /// <param name="responseJSON">Wit.ai speech-to-text response JSON object</param>
        /// <returns>Speech-to-text result object</returns>
        public static SpeechToTextResult GetTextResultFromResponseJSON(JSONObject responseJSON)
        {
            string result = "";
            responseJSON.GetField(out result, Constants.WitAiResponseJSONTextResultFieldKey, result);
            // TODO: For now the result will always be treated as final since Wit.ai does not provide
            // an "isFinal" field in its response JSON. But ideally in this situation, streamed results
            // would all be treated as interim until the last result in the session.
            return new SpeechToTextResult(result, true);
        }
    }
}
