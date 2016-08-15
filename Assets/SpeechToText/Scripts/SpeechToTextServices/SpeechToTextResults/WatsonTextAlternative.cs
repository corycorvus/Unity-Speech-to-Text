using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;

namespace UnitySpeechToText.Services
{
    public class WatsonTextAlternative : TextAlternative
    {
        /// <summary>
        /// Confidence level for the text transcription, between 0 and 1
        /// </summary>
        public float Confidence { get; set; }
        /// <summary>
        /// Optional array of timestamps for each word
        /// </summary>
        public TimeStamp[] TimeStamps { get; set; }
        /// <summary>
        /// Optional array of word confidence values
        /// </summary>
        public WordConfidence[] WordConfidenceValues { get; set; }
    }
}
