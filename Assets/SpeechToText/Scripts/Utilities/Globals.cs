using System.Collections.Generic;

namespace UnitySpeechToText.Utilities
{
    /// <summary>
    /// Container for all global constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Name of Speech-to-Text folder within Assets
        /// </summary>
        public const string SpeechToTextFolderName = "SpeechToText";
        /// <summary>
        /// Name of folder to which to save temporary files
        /// </summary>
        public const string TempFolderName = "Temp";
        /// <summary>
        /// Name of folder to which to save third party streaming assets
        /// </summary>
        public const string StreamingAssetsThirdPartyFolderName = "ThirdParty";

        /// <summary>
        /// Name of the SoX (Sound eXchange) application
        /// </summary>
        public const string SoXApplicationName = "sox";
        /// <summary>
        /// Text that SoX uses for the frequency option
        /// </summary>
        public const string SoXFrequencyOptionName = "--rate";
        /// <summary>
        /// Text that SoX uses for the encoding type option
        /// </summary>
        public const string SoXEncodingTypeOptionName = "--encoding";
        /// <summary>
        /// Text that SoX uses for the bits-per-sample option
        /// </summary>
        public const string SoXEncodingBitsOptionName = "--bits";
        /// <summary>
        /// Text that SoX uses for the channels option
        /// </summary>
        public const string SoXChannelsOptionName = "--channels";
        /// <summary>
        /// Text that SoX uses for the Endianness option
        /// </summary>
        public const string SoXEndiannessOptionName = "--endian";
        
        /// <summary>
        /// URL to use for Google non-streaming speech-to-text
        /// </summary>
        public const string GoogleNonStreamingSpeechToTextURL = "https://speech.googleapis.com/v1beta1/speech:syncrecognize/";
        /// <summary>
        /// Name for Google API key parameter in URL
        /// </summary>
        public const string GoogleAPIKeyParameterName = "key";
        /// <summary>
        /// Config field name in Google request JSON
        /// </summary>
        public const string GoogleRequestJSONConfigFieldKey = "config";
        /// <summary>
        /// Audio field key in Google request JSON
        /// </summary>
        public const string GoogleRequestJSONAudioFieldKey = "audio";
        /// <summary>
        /// Config encoding field key in Google request JSON
        /// </summary>
        public const string GoogleRequestJSONConfigEncodingFieldKey = "encoding";
        /// <summary>
        /// Config sample rate field key in Google request JSON
        /// </summary>
        public const string GoogleRequestJSONConfigSampleRateFieldKey = "sampleRate";
        /// <summary>
        /// Audio content field key in Google request JSON
        /// </summary>
        public const string GoogleRequestJSONAudioContentFieldKey = "content";
        /// <summary>
        /// Results field key in Google response JSON
        /// </summary>
        public const string GoogleResponseJSONResultsFieldKey = "results";
        /// <summary>
        /// Alternatives field key in Google response JSON
        /// </summary>
        public const string GoogleResponseJSONAlternativesFieldKey = "alternatives";
        /// <summary>
        /// Alternative transcript field key in Google response JSON
        /// </summary>
        public const string GoogleResponseJSONAlternativeTranscriptFieldKey = "transcript";
        /// <summary>
        /// Alternative confidence field key in Google response JSON
        /// </summary>
        public const string GoogleResponseJSONAlternativeConfidenceFieldKey = "confidence";
        /// <summary>
        /// Result is final field key in Google response JSON
        /// </summary>
        public const string GoogleResponseJSONResultIsFinalFieldKey = "is_final";
        /// <summary>
        /// Error field key in Google response JSON
        /// </summary>
        public const string GoogleResponseJSONErrorFieldKey = "error";
        /// <summary>
        /// Error code field key in Google response JSON
        /// </summary>
        public const string GoogleResponseJSONErrorCodeFieldKey = "code";
        /// <summary>
        /// Error message field key in Google response JSON
        /// </summary>
        public const string GoogleResponseJSONErrorMessageFieldKey = "message";

        /// <summary>
        /// ID of the Watson speech-to-text service
        /// </summary>
        public const string WatsonSpeechToTextServiceID = "SpeechToTextV1";
        /// <summary>
        /// Base URL to use for Watson speech-to-text
        /// </summary>
        public const string WatsonSpeechToTextBaseURL = "https://stream.watsonplatform.net/speech-to-text/api/";

        /// <summary>
        /// Base URL to use for Wit.ai speech-to-text
        /// </summary>
        public const string WitAiSpeechToTextBaseURL = "https://api.wit.ai/speech";
        /// <summary>
        /// Name for Wit.ai version parameter in URL
        /// </summary>
        public const string WitAiVersionParameterName = "v";
        /// <summary>
        /// Date format for Wit.ai version in URL
        /// </summary>
        public const string WitAiVersionDateFormat = "yyyyMMdd";
        /// <summary>
        /// Error message field key in Wit.ai response JSON
        /// </summary>
        public const string WitAiResponseJSONErrorMessageFieldKey = "error";
        /// <summary>
        /// Error code field key in Wit.ai response JSON
        /// </summary>
        public const string WitAiResponseJSONErrorCodeFieldKey = "code";
        /// <summary>
        /// Text result field key in Wit.ai response JSON
        /// </summary>
        public const string WitAiResponseJSONTextResultFieldKey = "_text";
    }

    /// <summary>
    /// Flags to be used for debugging purposes.
    /// </summary>
    public static class DebugFlags
    {
        /// <summary>
        /// Flag for MonoSingleton
        /// </summary>
        public static DebugFlag MonoSingleton = new DebugFlag("MonoSingleton", false);
        /// <summary>
        /// Flag for StringUtilities
        /// </summary>
        public static DebugFlag StringUtilities = new DebugFlag("StringUtilities", false);
        /// <summary>
        /// Flag for IOUtilities
        /// </summary>
        public static DebugFlag IOUtilities = new DebugFlag("IOUtilities", false);
        /// <summary>
        /// Flag for LogFileManager
        /// </summary>
        public static DebugFlag LogFileManager = new DebugFlag("LogFileManager", false);
        /// <summary>
        /// Flag for AudioRecordingManager
        /// </summary>
        public static DebugFlag AudioRecordingManager = new DebugFlag("AudioRecordingManager", false);
        /// <summary>
        /// Flag for SpeechToTextWidgets
        /// </summary>
        public static DebugFlag SpeechToTextWidgets = new DebugFlag("SpeechToTextWidgets", false);
        /// <summary>
        /// Flag for WindowsSpeechToText
        /// </summary>
        public static DebugFlag WindowsSpeechToText = new DebugFlag("WindowsSpeechToText", false);
        /// <summary>
        /// Flag for GoogleNonStreamingSpeechToText
        /// </summary>
        public static DebugFlag GoogleNonStreamingSpeechToText = new DebugFlag("GoogleNonStreamingSpeechToText", false);
        /// <summary>
        /// Flag for GoogleStreamingSpeechToText
        /// </summary>
        public static DebugFlag GoogleStreamingSpeechToText = new DebugFlag("GoogleStreamingSpeechToText", false);
        /// <summary>
        /// Flag for WitAINonStreamingSpeechToText
        /// </summary>
        public static DebugFlag WitAINonStreamingSpeechToText = new DebugFlag("WitAINonStreamingSpeechToText", false);
        /// <summary>
        /// Flag for WatsonStreamingSpeechToText
        /// </summary>
        public static DebugFlag WatsonStreamingSpeechToText = new DebugFlag("WatsonStreamingSpeechToText", false);
        /// <summary>
        /// Flag for WatsonNonStreamingSpeechToText
        /// </summary>
        public static DebugFlag WatsonNonStreamingSpeechToText = new DebugFlag("WatsonNonStreamingSpeechToTextingleton", false);
    }

    /// <summary>
    /// Wrapper for different byte orders.
    /// </summary>
    public static class Endianness
    {
        /// <summary>
        /// Types of byte orders.
        /// </summary>
        public enum EndiannessType
        {
            Unspecified,
            Little,
            Big,
            Swap
        }

        /// <summary>
        /// Names of byte orders.
        /// </summary>
        public static Dictionary<EndiannessType, string> SoXEndiannessNames = new Dictionary<EndiannessType, string>
        {
            { EndiannessType.Little, "little" },
            { EndiannessType.Big, "big" },
            { EndiannessType.Swap, "swap" }
        };
    }

    /// <summary>
    /// Wrapper for different audio encodings.
    /// </summary>
    public static class AudioEncoding
    {
        /// <summary>
        /// Types of audio encodings.
        /// </summary>
        public enum EncodingType
        {
            Unspecified,
            SignedInteger,
            UnsignedInteger,
            FloatingPoint,
            ALaw,
            ULaw,
            MuLaw,
            OkiAdpcm,
            ImaAdpcm,
            MsAdpcm,
            GSMFullRate
        }

        /// <summary>
        /// Names of audio encodings.
        /// </summary>
        public static Dictionary<EncodingType, string> EncodingNames = new Dictionary<EncodingType, string>
        {
            { EncodingType.SignedInteger, "signed-integer" },
            { EncodingType.UnsignedInteger, "unsigned-integer" },
            { EncodingType.FloatingPoint, "floating-point" },
            { EncodingType.ALaw, "a-law" },
            { EncodingType.ULaw, "u-law" },
            { EncodingType.MuLaw, "mu-law" },
            { EncodingType.OkiAdpcm, "oki-adpcm" },
            { EncodingType.ImaAdpcm, "ima-adpcm" },
            { EncodingType.MsAdpcm, "ms-adpcm" },
            { EncodingType.GSMFullRate, "gsm-full-rate" }
        };
    }
}
