using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnitySpeechToText.Utilities
{
    /// <summary>
    /// Wrapper for UnityEngine.Debug logger that only outputs messages when in the Unity Editor and explicitly specified
    /// given a debug flag. Messages are formatted with the name of the debug flag.
    /// </summary>
    public static class SmartLogger
    {
        /// <summary>
        /// Store for AlwaysLogErrors property
        /// </summary>
        static bool m_AlwaysLogErrors = true;

        /// <summary>
        /// Whether errors should be logged to the Unity Editor regardless of the value of the debug flag.
        /// By default this is true, but if you really want to hide errors this can be changed.
        /// </summary>
        public static bool AlwaysLogErrors { set { m_AlwaysLogErrors = value; } }

        /// <summary>
        /// Logs a message to the Unity Console if the program is running from the Unity Editor and the value of debugFlag is true.
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="message">Message to log to the console</param>
        public static void Log(DebugFlag debugFlag, object message)
        {
#if UNITY_EDITOR
            if (debugFlag.Value)
            {
                Debug.Log("[DebugFlag: " + debugFlag.Name + "] " + message);
            }
#endif
        }

        /// <summary>
        /// Logs a message to the Unity Console if the program is running from the Unity Editor and the value of debugFlag is true.
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="message">Message to log to the console</param>
        /// <param name="context">Object to which the message applies</param>
        public static void Log(DebugFlag debugFlag, object message, UnityObject context)
        {
#if UNITY_EDITOR
            if (debugFlag.Value)
            {
                Debug.Log("[DebugFlag: " + debugFlag.Name + "] " + message, context);
            }
#endif
        }

        /// <summary>
        /// Logs a formatted message to the Unity Console if the program is running from the Unity Editor and the value of debugFlag is true.
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="format">A composite format string</param>
        /// <param name="args">Format arguments</param>
        public static void LogFormat(DebugFlag debugFlag, string format, params object[] args)
        {
#if UNITY_EDITOR
            if (debugFlag.Value)
            {
                Debug.LogFormat("[DebugFlag: " + debugFlag.Name + "] " + format, args);
            }
#endif
        }

        /// <summary>
        /// Logs a formatted message to the Unity Console if the program is running from the Unity Editor and the value of debugFlag is true.
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="context">Object to which the message applies</param>
        /// <param name="format">A composite format string</param>
        /// <param name="args">Format arguments</param>
        public static void LogFormat(DebugFlag debugFlag, UnityObject context, string format, params object[] args)
        {
#if UNITY_EDITOR
            if (debugFlag.Value)
            {
                Debug.LogFormat(context, "[DebugFlag: " + debugFlag.Name + "] " + format, args);
            }
#endif
        }

        /// <summary>
        /// Logs a warning message to the Unity Console if the program is running from the Unity Editor and the value of debugFlag is true.
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="message">Message to log to the console</param>
        public static void LogWarning(DebugFlag debugFlag, object message)
        {
#if UNITY_EDITOR
            if (debugFlag.Value)
            {
                Debug.LogWarning("[DebugFlag: " + debugFlag.Name + "] " + message);
            }
#endif
        }

        /// <summary>
        /// Logs a warning message to the Unity Console if the program is running from the Unity Editor and the value of debugFlag is true.
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="message">Message to log to the console</param>
        /// <param name="context">Object to which the message applies</param>
        public static void LogWarning(DebugFlag debugFlag, object message, UnityObject context)
        {
#if UNITY_EDITOR
            if (debugFlag.Value)
            {
                Debug.LogWarning("[DebugFlag: " + debugFlag.Name + "] " + message, context);
            }
#endif
        }

        /// <summary>
        /// Logs a formatted warning message to the Unity Console if the program is running from the Unity Editor and the value of debugFlag is true.
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="format">A composite format string</param>
        /// <param name="args">Format arguments</param>
        public static void LogWarningFormat(DebugFlag debugFlag, string format, params object[] args)
        {
#if UNITY_EDITOR
            if (debugFlag.Value)
            {
                Debug.LogWarningFormat("[DebugFlag: " + debugFlag.Name + "] " + format, args);
            }
#endif
        }

        /// <summary>
        /// Logs a formatted warning message to the Unity Console if the program is running from the Unity Editor and the value of debugFlag is true.
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="context">Object to which the message applies</param>
        /// <param name="format">A composite format string</param>
        /// <param name="args">Format arguments</param>
        public static void LogWarningFormat(DebugFlag debugFlag, UnityObject context, string format, params object[] args)
        {
#if UNITY_EDITOR
            if (debugFlag.Value)
            {
                Debug.LogWarningFormat(context, "[DebugFlag: " + debugFlag.Name + "] " + format, args);
            }
#endif
        }

        /// <summary>
        /// Logs an error message to the Unity Console if the program is running from the Unity Editor
        /// and either AlwaysLogErrors is true or the value of debugFlag is true.
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="message">Message to log to the console</param>
        public static void LogError(DebugFlag debugFlag, object message)
        {
#if UNITY_EDITOR
            if (m_AlwaysLogErrors || debugFlag.Value)
            {
                Debug.LogError("[DebugFlag: " + debugFlag.Name + "] " + message);
            }
#endif
        }

        /// <summary>
        /// Logs an error message to the Unity Console if the program is running from the Unity Editor
        /// and either AlwaysLogErrors is true or the value of debugFlag is true..
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="message">Message to log to the console</param>
        /// <param name="context">Object to which the message applies</param>
        public static void LogError(DebugFlag debugFlag, object message, UnityObject context)
        {
#if UNITY_EDITOR
            if (m_AlwaysLogErrors || debugFlag.Value)
            {
                Debug.LogError("[DebugFlag: " + debugFlag.Name + "] " + message, context);
            }
#endif
        }

        /// <summary>
        /// Logs a formatted error message to the Unity Console if the program is running from the Unity Editor
        /// and either AlwaysLogErrors is true or the value of debugFlag is true.
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="format">A composite format string</param>
        /// <param name="args">Format arguments</param>
        public static void LogErrorFormat(DebugFlag debugFlag, string format, params object[] args)
        {
#if UNITY_EDITOR
            if (m_AlwaysLogErrors || debugFlag.Value)
            {
                Debug.LogErrorFormat("[DebugFlag: " + debugFlag.Name + "] " + format, args);
            }
#endif
        }

        /// <summary>
        /// Logs a formatted error message to the Unity Console if the program is running from the Unity Editor
        /// and either AlwaysLogErrors is true or the value of debugFlag is true..
        /// </summary>
        /// <param name="debugFlag">Flag for whether the message should be logged to the console</param>
        /// <param name="context">Object to which the message applies</param>
        /// <param name="format">A composite format string</param>
        /// <param name="args">Format arguments</param>
        public static void LogErrorFormat(DebugFlag debugFlag, UnityObject context, string format, params object[] args)
        {
#if UNITY_EDITOR
            if (m_AlwaysLogErrors || debugFlag.Value)
            {
                Debug.LogErrorFormat(context, "[DebugFlag: " + debugFlag.Name + "] " + format, args);
            }
#endif
        }
    }
}
