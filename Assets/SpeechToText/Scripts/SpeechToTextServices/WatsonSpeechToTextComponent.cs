using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Container for IBM Watson speech-to-text functionality.
    /// </summary>
    public class WatsonSpeechToTextComponent
    {
        /// <summary>
        /// Store for WatsonSpeechToTextService property
        /// </summary>
        SpeechToText m_WatsonSpeechToTextService = new SpeechToText();

        /// <summary>
        /// Watson speech-to-text service
        /// </summary>
        public SpeechToText WatsonSpeechToTextService { get { return m_WatsonSpeechToTextService; } }

        /// <summary>
        /// Configures Watson API user credentials.
        /// </summary>
        /// <param name="apiUsername">Watson API username</param>
        /// <param name="apiPassword">Watson API password</param>
        public void ConfigureCredentials(string apiUsername, string apiPassword)
        {
            if (Config.Instance.FindCredentials(Utilities.Constants.WatsonSpeechToTextServiceID) == null)
            {
                var speechToTextCredentialInfo = new Config.CredentialInfo();
                speechToTextCredentialInfo.m_ServiceID = Utilities.Constants.WatsonSpeechToTextServiceID;
                speechToTextCredentialInfo.m_URL = Utilities.Constants.WatsonSpeechToTextBaseURL;
                speechToTextCredentialInfo.m_User = apiUsername;
                speechToTextCredentialInfo.m_Password = apiPassword;
                Config.Instance.Credentials.Add(speechToTextCredentialInfo);
            }
        }

        /// <summary>
        /// Populates and returns a SpeechToTextResult object from a given Watson SpeechResult object.
        /// </summary>
        /// <param name="watsonResult">Watson SpeechResult object</param>
        /// <returns>A SpeechToTextResult object</returns>
        public SpeechToTextResult CreateSpeechToTextResult(SpeechResult watsonResult)
        {
            var textResult = new SpeechToTextResult();
            textResult.IsFinal = watsonResult.Final;
            textResult.TextAlternatives = new TextAlternative[watsonResult.Alternatives.Length];
            for (int i = 0; i < textResult.TextAlternatives.Length; ++i)
            {
                SpeechAlt watsonAlternative = watsonResult.Alternatives[i];
                var alternative = new WatsonTextAlternative();
                alternative.Text = watsonAlternative.Transcript;
                alternative.Confidence = (float)watsonAlternative.Confidence;
                alternative.TimeStamps = watsonAlternative.Timestamps;
                alternative.WordConfidenceValues = watsonAlternative.WordConfidence;
                textResult.TextAlternatives[i] = alternative;
            }
            return textResult;
        }
    }
}
