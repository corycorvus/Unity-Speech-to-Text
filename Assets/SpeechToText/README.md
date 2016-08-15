# Speech-to-Text
This plugin interfaces Windows streaming, Wit.ai non-streaming, Google streaming/non-streaming, and IBM Watson streaming/non-streaming speech-to-text. There is also a sample scene that compares each of these APIs. [This article](https://cms-labs.hq.unity3d.com/article/speech-recognition-and-vr) on the Unity Labs website explains some of the concepts behind speech recognition and discusses the motivation behind this package.

## Table of Contents
* [Requirements](#requirements)
* [Setting up the sample scene](#setting-up-the-sample-scene)
* [Recording and comparing results](#recording-and-comparing-results)
* [Acquiring credentials](#acquiring-credentials)
	* [Google Cloud Speech](#google-cloud-speech)
	* [IBM Watson Speech to Text](#ibm-watson-speech-to-text)
	* [Wit.ai](#wit.ai)
* [Architecture](#architecture)
	* [Namespaces](#namespaces)
	* [Speech-to-text services and results inheritance hierarchy](#speech-to-text-services-and-results-inheritance-hierarchy)
	* [Speech-to-text services and results base functions and properties](#speech-to-text-services-and-results-base-functions-and-properties)
	* [AudioRecordingManager functions and properties](#audiorecordingmanager-functions-and-properties)
	* [SmartLogger and DebugFlags](#smartlogger-and-debugflags)
	* [Example of speech-to-text service usage](#example-of-speech-to-text-service-usage)
* [Forks](#forks)

## Requirements
* Matthew Schoen from Unity Labs has given us permission to include his JSON library in the package.
* Windows and Google streaming speech-to-text will only work in Windows environments.
* Watson streaming and non-streaming speech-to-text both rely on IBM's Watson SDK for Unity, which must be manually added to the project. [The Unity Watson SDK can be found here.](https://github.com/watson-developer-cloud/unity-sdk)
* Google non-streaming and Wit.ai non-streaming speech-to-text both rely on UniWeb, which must be manually added to the project. [UniWeb can be found on the Unity Asset Store here.](https://www.assetstore.unity3d.com/en/#!/content/483)
* Google streaming and non-streaming speech-to-text both rely on SoX (Sound eXchange), which must be manually added to the project. [SoX can be found here.](http://sox.sourceforge.net/) The SoX application must be located within `Application.streamingAssetsPath`/ThirdParty/SoX/Windows for Windows environments, and `Application.streamingAssetsPath`/ThirdParty/SoX/MacOSX otherwise.
	
## Setting up the sample scene
1. Open the scene "speechToTextComparison.unity".
2. Enter your credentials for each API by going through each child of "Canvas/SpeechToTextServiceWidgets" in the Inspector and changing the appropriate field(s) in the "[Specific Name Here] Speech To Text Service" component. Note that Google streaming speech-to-text uses a JSON credentials file, which must be saved under "GoogleStreamingSpeechToTextProgram" under `Application.streamingAssetsPath`, and whose name must match the "JSON Credentials File Name" field of the "Google Streaming Speech To Text Service" component of "Canvas/SpeechToTextServiceWidgets/GoogleStreamingSpeechToTextService". You will only receive transcriptions from APIs for which you have provided valid credentials (except Windows, which does not require any). See the "[Acquiring credentials](#acquiring-credentials)" section for instructions on acquiring credentials for each API.
3. Configure any parameters that you wish to change for each service (timeout, audio chunk length, etc.) Check the "Speech To Text Comparison Widget" component of "Canvas/SpeechToTextComparisonWidget" in the Inspector to make sure that all the services you wish to test are listed under "Speech To Text Service Widgets", and add/remove services from this list as needed.
4. The scene is now ready to run. Refer to [Recording and comparing results](#recording-and-comparing-results) for how the sample scene works.

## Recording and comparing results
* Real-time results will be displayed for streaming speech-to-text, and results for non-streaming speech-to-text will be displayed after you stop recording.
* One single recording session can only last 15 seconds before timing out, but this can be changed by looking at the "Audio Recording Manager" component of the "Singletons" game object in the Inspector and modifying "Max Recording Length In Seconds". 
* After recording, if the application has not received all results within 10 seconds, it will stop listening for results. This can be changed by going to the "Speech To Text Comparison Widget" component of "Canvas/SpeechToTextComparisonWidget" in the Inspector and changing "Responses Timeout In Seconds".
* If you have selected a sample phrase before you stop recording, then each end result will be compared against this sample phrase and the accuracy will be displayed.
* If you have selected "Save results to file?", then text results at the end of each recording session will be saved to a .txt file in the `Application.dataPath`/SpeechToText folder. A new file will be used with each run of the application.
		
## Acquiring credentials

### Google Cloud Speech
1. Sign up for a [Google Cloud Platform](https://cloud.google.com/) account.
2. Sign up for [Google Cloud Speech API](https://cloud.google.com/speech/).
3. Once you have been granted access to Google Cloud Speech API, refer to the documentation for instructions on generating an API key (for non-streaming speech-to-text) and a JSON service account key (for streaming speech-to-text).

### IBM Watson Speech to Text
1. Sign up for an [IBM Bluemix](https://console.ng.bluemix.net/) account.
2. Sign up for [IBM Watson Speech to Text](https://console.ng.bluemix.net/catalog/services/speech-to-text). 
3. Once you have been granted access to IBM Watson Speech to Text, refer to the documentation for instructions on generating service credentials.

### Wit.ai
1. Sign up for a [Wit.ai](https://wit.ai/) account.
2. Create a new app through the Wit.ai console.
3. Your server access token will be listed under your app settings.

## Architecture

### Namespaces
* `UnitySpeechToText` includes all non-third-party scripts in the package.
* `UnitySpeechToText.Services` includes all speech-to-text service and result scripts.
* `UnitySpeechToText.Widgets` includes the speech-to-text widget scripts used in the sample scene.
* `UnitySpeechToText.Utilities` includes all general utility scripts, such as `AudioRecordingManager` and `DebugFlags`.

### Speech-to-text services and results inheritance hierarchy
* MonoBehaviour
	* SpeechToTextService
		* NonStreamingSpeechToTextService
			* GoogleNonStreamingSpeechToTextService
			* WatsonNonStreamingSpeechToTextService
			* WitAiNonStreamingSpeechToTextService
		* StreamingSpeechToTextService
			* GoogleStreamingSpeechToTextService
			* WatsonStreamingSpeechToTextService
		* WindowsSpeechToTextService
* SpeechToTextResult
* TextAlternative
	* GoogleTextAlternative
	* WatsonTextAlternative
	* WindowsTextAlternative

### Speech-to-text services and results base functions and properties
* SpeechToTextService
	* `public bool IsRecording` [get]
		* Whether the service is recording audio
	* `public void RegisterOnTextResult(Action<SpeechToTextResult> action)`
		* Adds a function to the text result delegate.
	* `public void UnregisterOnTextResult(Action<SpeechToTextResult> action)`
		* Removes a function from the text result delegate.
	* `public void RegisterOnError(Action<string> action)`
		* Adds a function to the error delegate.
	* `public void UnregisterOnError(Action<string> action)`
		* Removes a function from the error delegate.
	* `public void RegisterOnRecordingTimeout(Action action)`
		* Adds a function to the recording timeout delegate.
	* `public void UnregisterOnRecordingTimeout(Action action)`
		* Removes a function from the recording timeout delegate.
	* `public virtual bool StartRecording()`
		* Starts recording audio if the service is not already recording.
		* Returns whether the service successfully started recording.
	* `public virtual bool StopRecording()`
		* Stops recording audio if the service is already recording.
		* Returns whether the service successfully stopped recording.
	* `protected virtual void Start()`
		* Initialization function called on the frame when the script is enabled just before any of the Update methods is called the first time.
	* `protected virtual void OnDestroy()`
		* Function that is called when the MonoBehaviour will be destroyed.
	* `protected void OnRecordingTimeout()`
		* Function that is called when the recording times out.
* NonStreamingSpeechToTextService
	* `public override bool StartRecording()`
		* Starts recording audio if the service is not already recording.
		* Returns whether the service successfully started recording.
	* `public override bool StopRecording()`
		* Stops recording audio if the service is already recording.
		* Returns whether the service successfully stopped recording.
	* `private IEnumerator RecordAndTranslateToText()`
		* Records audio and translates any speech to text.
	* `protected abstract IEnumerator TranslateRecordingToText()`
		* Translates speech to text by making a request to the speech-to-text API.
* StreamingSpeechToTextService
	* `public float SessionTimeoutAfterDoneRecording` [set]
		* Number of seconds after recording to wait until the session times out
	* `public float AudioChunkLengthInSeconds` [set]
		* Length (in seconds) of each chunk of recorded audio to send to the server
	* `public override bool StartRecording()`
		* Starts recording audio if the service is not already recording.
		* Returns whether the service successfully started recording.
	* `public override bool StopRecording()`
		* Stops recording audio if the service is already recording.
		* Returns whether the service successfully stopped recording.
	* `private IEnumerator RecordAudio()`
		* Records audio and queues fixed audio chunks.
	* `protected abstract IEnumerator StreamAudioAndListenForResponses()`
		* Sends queued chunks of audio to the server and then waits for the transcription(s).
* SpeechToTextResult
	* `public bool IsFinal` [get and set]
		* Whether this is a final (rather than interim) result
	* `public TextAlternative[] TextAlternatives` [get and set]
		* Array of text transcription alternatives
	* `public SpeechToTextResult()`
		* Default class constructor.
	* `public SpeechToTextResult(string text, bool isFinal)`
		* Class constructor given a single string text alternative and whether the result is final.
* TextAlternative
	* `public string Text` [get and set]
		* The text transcription itself

### AudioRecordingManager functions and properties
* `public int RecordingFrequency` [set]
	* Frequency (samples-per-second) at which to record
* `public int MaxRecordingLengthInSeconds` [set]
	* Number of seconds to record before the recording times out
* `public AudioClip RecordedAudio` [get]
	* Audio clip created from the most recent recording
* `public void RegisterOnTimeout(Action action)`
	* Adds a function to the timeout delegate.
* `public void UnregisterOnTimeout(Action action)`
	* Removes a function from the timeout delegate.
* `public bool IsRecording()`
	* Queries if the default device is currently recording.
	* Returns whether the default device is currently recording.
* `private IEnumerator WaitForRecordingTimeout()`
	* Waits for the default device to stop recording and checks if this was due to a timeout.
* `public IEnumerator RecordAndWaitUntilDone()`
	* Starts a recording session and waits until it finishes.
* `public void StartRecording()`
	* Tells the default device to start recording if it is not already.
* `public void StopRecording()`
	* If the default device is recording, ends the recording session and trims the default audio clip produced.
* `public AudioClip GetChunkOfRecordedAudio(float offsetInSeconds, float chunkLengthInSeconds)`
	* Creates and returns a specific chunk of audio from the current recording.
	* Returns the audio chunk or null if the chunk length is less than or equal to 0 or if the offset is greater than or equal to the recorded audio length.

### SmartLogger and DebugFlags
* `SmartLogger` is a wrapper for the `UnityEngine.Debug` logger that can be used to only log debug messages when explicitly specified given a debug flag.
* `DebugFlags` contains static flags that can be passed to the `SmartLogger` functions. To create your own flag, simply add a `public static DebugFlag` member variable to this class and construct it with your desired flag name and boolean value.

## Example of speech-to-text service usage
* Create a game object with a "[Specific Name Here] Speech To Text Service" component, and fill in all necessary fields in the Inspector. (Of course, you may instead choose to do all this programmatically.)
* Assign a reference to [SpecificNameHere]SpeechToTextService (in this example we will call the reference `m_SpeechToTextService`) to the script that will be interacting with [SpecificNameHere]SpeechToTextService.
* Add the following functions to your script.
```csharp
void OnError(string text)
{
	Debug.LogError(text);
}

// Note that handling interim results is only necessary if your speech-to-text service is streaming.
// Non-streaming speech-to-text services should only return one result per recording.
void OnTextResult(SpeechToTextResult result)
{
	if (result.IsFinal)
	{
		Debug.Log("Final result:");
	}
	else
	{
		Debug.Log("Interim result:");
	}
	for (int i = 0; i < result.TextAlternatives.Length; ++i)
	{
		Debug.Log("Alternative " + i + ": " + result.TextAlternatives[i].Text);
	}
}

void OnRecordingTimeout()
{
	Debug.Log("Timeout");
}
```
* Add the following code in a place that will be guaranteed to execute before you call `m_SpeechToTextService.StartRecording()`. (Most of the time this code should just be in either `Start()` or `Awake()`, assuming your script inherits from `MonoBehaviour`.)
```csharp
m_SpeechToTextService.RegisterOnError(OnError);
m_SpeechToTextService.RegisterOnTextResult(OnTextResult);
m_SpeechToTextService.RegisterOnRecordingTimeout(OnRecordingTimeout);
```
* (Optional) If at some point in execution you want to stop handling results within this script, add the following code to that place. (A good example is within `OnDestroy()`.)
```csharp
m_SpeechToTextService.UnregisterOnError(OnError);
m_SpeechToTextService.UnregisterOnTextResult(OnTextResult);
m_SpeechToTextService.UnregisterOnRecordingTimeout(OnRecordingTimeout);
```
* Create a hook for when you want to start recording, and in it add `m_SpeechToTextService.StartRecording()`. Create a hook for when you want to stop recording, and in it add `m_SpeechToTextService.StopRecording()`. These hooks could be functions called upon button presses, for example.
* (Optional) If the specific speech-to-text service you choose uses `AudioRecordingManager` to record audio, you can assign the public properties of `AudioRecordingManager` in a place that will be guaranteed to execute before you call `m_SpeechToTextService.StartRecording()`. This can be done from the Inspector if you create a game object with a "Audio Recording Manager" component, or from script by using `AudioRecordingManager.Instance`. (In the latter case, do not worry about creating a game object with this component - the `MonoSingleton` implementation will take care of that for you.)

## Forks
[The BitBucket repository for this project can be found here.](https://bitbucket.org/Unity-Technologies/speech-to-text) Anyone in the community is welcome to create their own forks. Drop us a note at labs@unity3d.com if you find it useful, we'd love to hear from you!