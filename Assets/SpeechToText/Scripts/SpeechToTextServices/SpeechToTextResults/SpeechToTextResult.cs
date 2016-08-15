namespace UnitySpeechToText.Services
{
    /// <summary>
    /// Base class for a speech-to-text result.
    /// </summary>
    public class SpeechToTextResult
    {
        /// <summary>
        /// Store for IsFinal property
        /// </summary>
        bool m_IsFinal = true;

        /// <summary>
        /// Whether this is a final (rather than interim) result
        /// </summary>
        public bool IsFinal
        {
            get { return m_IsFinal; }
            set { m_IsFinal = value; }
        }

        /// <summary>
        /// Array of text transcription alternatives
        /// </summary>
        public TextAlternative[] TextAlternatives { get; set; }

        /// <summary>
        /// Default class constructor.
        /// </summary>
        public SpeechToTextResult() { }

        /// <summary>
        /// Class constructor given a single string text alternative and whether the result is final.
        /// </summary>
        /// <param name="text">Single text transcription alternative</param>
        /// <param name="isFinal">Whether the result is final</param>
        public SpeechToTextResult(string text, bool isFinal)
        {
            TextAlternatives = new TextAlternative[] { new TextAlternative() };
            TextAlternatives[0].Text = text;
            IsFinal = isFinal;
        }
    }
}
