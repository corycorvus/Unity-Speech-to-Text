namespace UnitySpeechToText.Utilities
{
    /// <summary>
    /// Flag with a debug name and boolean value.
    /// </summary>
    public class DebugFlag
    {
        /// <summary>
        /// Store for Name property
        /// </summary>
        string m_Name;
        /// <summary>
        /// Store for Value property
        /// </summary>
        bool m_Value;

        /// <summary>
        /// Flag name
        /// </summary>
        public string Name { get { return m_Name; } }
        /// <summary>
        /// Flag value
        /// </summary>
        public bool Value { get { return m_Value; } }

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="name">Flag name</param>
        /// <param name="value">Flag value</param>
        public DebugFlag(string name, bool value)
        {
            m_Name = name;
            m_Value = value;
        }
    }
}
