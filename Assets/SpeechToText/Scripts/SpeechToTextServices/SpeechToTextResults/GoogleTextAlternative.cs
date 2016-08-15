namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Google text transcription alternative.
    /// </summary>
    public class GoogleTextAlternative : TextAlternative
    {
        /// <summary>
        /// Confidence level for the text transcription, between 0 and 1
        /// </summary>
        public float Confidence { get; set; }
    }
}
