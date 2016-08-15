using UnityEngine.Windows.Speech;

namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Windows text transcription alternative.
    /// </summary>
    public class WindowsTextAlternative : TextAlternative
    {
        /// <summary>
        /// Confidence level for the text transcription, either High, Medium, Low, or Rejected
        /// </summary>
        public ConfidenceLevel Confidence { get; set; }
    }
}
