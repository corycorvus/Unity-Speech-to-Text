using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Grpc.Core;
using Grpc.Auth;
using Google.Protobuf;
using Google.Cloud.Speech.V1Beta1;
using Google.Apis.Auth.OAuth2;

namespace GoogleCloudSpeech
{
    /// <summary>
    /// Communicates with the Google Cloud Speech server to stream speech-to-text results to the Console.
    /// </summary>
    class GoogleStreamingSpeechToText
    {
        /// <summary>
        /// Number of milliseconds between each write of audio data to the request stream.
        /// </summary>
        const int k_MillisecondsBetweenChunks = 500;

        /// <summary>
        /// gRPC channel used to communicate with the Google Cloud Speech API.
        /// </summary>
        static Channel m_Channel;

        /// <summary>
        /// Client for speech-to-text.
        /// </summary>
        static Speech.SpeechClient m_Client;

        /// <summary>
        /// Chunks of audio that are queued to be sent to the server.
        /// </summary>
        static Queue<ByteString> m_AudioChunksQueue = new Queue<ByteString>();

        /// <summary>
        /// Whether the process has finished streaming audio.
        /// </summary>
        static bool m_DoneStreaming;

        /// <summary>
        /// Starting point of the Google Streaming Speech-to-Text program.
        /// </summary>
        /// <param name="args">First argument must be the path to the credentials JSON file.</param>
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: GoogleCloudSpeech.exe [path to credentials JSON file]");
            }
            else
            {
                // Create client
                var fileStream = new FileStream(args[0], FileMode.Open);
                GoogleCredential googleCredential = GoogleCredential.FromStream(fileStream);
                ChannelCredentials channelCredentials = GoogleGrpcCredentials.ToChannelCredentials(googleCredential);
                m_Channel = new Channel("speech.googleapis.com", channelCredentials);
                m_Client = new Speech.SpeechClient(m_Channel);

                // Wait to begin streaming
                Console.WriteLine("ready to start");

                while (Console.ReadLine() != "start") ;

                Console.WriteLine("start streaming");
                Task streamingTask = StreamingRequest();

                // Check for input files
                string input;
                while ((input = Console.ReadLine()) != "stop")
                {
                    m_AudioChunksQueue.Enqueue(ByteString.CopyFrom(File.ReadAllBytes(input)));
                    Console.WriteLine("received audio file input");
                }

                // Finish streaming and wait for the request to finish
                Console.WriteLine("stop streaming");
                m_DoneStreaming = true;
                streamingTask.Wait();
                m_Channel.ShutdownAsync().Wait();
            }
        }

        /// <summary>
        /// Asynchronously streams audio to the Google Cloud Speech server and receives results.
        /// </summary>
        static async Task StreamingRequest()
        {
            using (var call = m_Client.StreamingRecognize())
            {
                Task responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        StreamingRecognizeResponse response = call.ResponseStream.Current;
                        var responseJSON = new JSONObject();
                        if (response.Error != null)
                        {
                            var errorJSON = new JSONObject();
                            errorJSON.AddField("code", response.Error.Code);
                            errorJSON.AddField("message", response.Error.Message);
                            responseJSON.AddField("error", errorJSON);
                        }
                        if (response.Results != null && response.Results.Count > 0)
                        {
                            var resultsJSON = new JSONObject();
                            foreach (var result in response.Results)
                            {
                                var resultJSON = new JSONObject();
                                if (result.Alternatives != null && result.Alternatives.Count > 0)
                                {
                                    var alternativesJSON = new JSONObject();
                                    foreach (var alternative in result.Alternatives)
                                    {
                                        var alternativeJSON = new JSONObject();
                                        alternativeJSON.AddField("transcript", alternative.Transcript);
                                        alternativeJSON.AddField("confidence", alternative.Confidence);
                                        alternativesJSON.Add(alternativeJSON);
                                    }
                                    resultJSON.AddField("alternatives", alternativesJSON);
                                    resultJSON.AddField("is_final", result.IsFinal);
                                    resultJSON.AddField("stability", result.Stability);
                                }
                                resultsJSON.Add(resultJSON);
                            }
                            responseJSON.AddField("results", resultsJSON);
                        }
                        responseJSON.AddField("result_index", response.ResultIndex);
                        responseJSON.AddField("endpointer_type", response.EndpointerType.ToString());
                        Console.WriteLine("response: " + responseJSON);
                    }
                });

                // Send initial config request
                var configRequest = new StreamingRecognizeRequest();
                var streamingRecognitionConfig = new StreamingRecognitionConfig();
                var recognitionConfig = new RecognitionConfig();
                recognitionConfig.Encoding = RecognitionConfig.Types.AudioEncoding.LINEAR16;
                recognitionConfig.SampleRate = 16000;
                streamingRecognitionConfig.InterimResults = true;
                streamingRecognitionConfig.SingleUtterance = false;
                streamingRecognitionConfig.Config = recognitionConfig;
                configRequest.StreamingConfig = streamingRecognitionConfig;
                await call.RequestStream.WriteAsync(configRequest);

                // Send audio chunks
                Task sendChunksTask = SendChunks(call);

                await sendChunksTask;
                await call.RequestStream.CompleteAsync();
                await responseReaderTask;

                call.Dispose();
            }
        }

        /// <summary>
        /// Asynchronously sends chunks of audio to the server.
        /// </summary>
        /// <param name="call">Bidirectional streaming call whose request stream to write to.</param>
        static async Task SendChunks(AsyncDuplexStreamingCall<StreamingRecognizeRequest, StreamingRecognizeResponse> call)
        {
            while (!m_DoneStreaming)
            {
                if (m_AudioChunksQueue.Count > 0)
                {
                    var audioRequest = new StreamingRecognizeRequest();
                    audioRequest.AudioContent = m_AudioChunksQueue.Dequeue();
                    await call.RequestStream.WriteAsync(audioRequest);
                }
                await Task.Delay(k_MillisecondsBetweenChunks);
            }
        }
    }
}
