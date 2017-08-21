//
//  API.AI Xamarin SDK - client-side libraries for API.AI
//  =================================================
//
//  Copyright (C) 2015 by Speaktoit, Inc. (https://www.speaktoit.com)
//  https://www.api.ai
//
//  ***********************************************************************************************************************
//
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
//  the License. You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
//  an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
//  specific language governing permissions and limitations under the License.
//
//  ***********************************************************************************************************************
//

using System;
using ApiAiSDK;
using AVFoundation;
using ApiAiSDK.Util;
using ApiAiSDK.Model;
using System.Threading.Tasks;
using Speech;
using System.IO;
using Foundation;
using System.Collections.Generic;
using AiApiSdk.Model;
using System.Linq;

namespace ApiAiFormsSDK.iOS
{
	public class SpeaktoitRecognitionService : BaseSpeaktoitRecognitionService
	{
        
        private readonly string TAG = typeof(SpeaktoitRecognitionService).Name;

		private const int SAMPLE_RATE_HZ = 16000;

		private readonly SoundRecorder soundRecorder;
		private AudioStream audioStream;
		private VoiceActivityDetector vad = new VoiceActivityDetector(SAMPLE_RATE_HZ);

		IDisposable anotherSessionNotificationToken;

		public SpeaktoitRecognitionService(AIConfiguration config) : base(config)
		{
			soundRecorder = new SoundRecorder(SAMPLE_RATE_HZ, vad);

			vad.Enabled = config.VoiceActivityDetectionEnabled;

			vad.SpeechBegin += Vad_SpeechBegin;
			vad.SpeechEnd += Vad_SpeechEnd;
			vad.SpeechNotDetected += Vad_SpeechNotDetected; ;
			vad.AudioLevelChange += Vad_AudioLevelChange;


		}

		void Vad_SpeechNotDetected()
		{
             
			Log.Debug(TAG, "Vad_SpeechNotDetected");
			new Task(Cancel).Start();
		}

		void Vad_SpeechBegin()
		{
			Log.Debug(TAG, "Vad_SpeechBegin");
			new Task(OnSpeechBegin).Start();
		}

		void Vad_SpeechEnd()
		{
			Log.Debug(TAG, "Vad_SpeechEnd");
			new Task(OnSpeechEnd).Start();
			new Task(StopListening).Start();
		}

		void Vad_AudioLevelChange(float level)
		{
			OnAudioLevelChanged(level);
		}

		#region implemented abstract members of BaseSpeaktoitRecognitionService
		protected override void StartRecording()
		{
			vad.Reset();

			audioStream = soundRecorder.CreateAudioStream();
			soundRecorder.StartRecording();

			anotherSessionNotificationToken = AVAudioSession.Notifications.ObserveInterruption(OnAnotherSessionStarts);
		}

		protected override void StopRecording()
		{
			soundRecorder.StopRecording();
			if (anotherSessionNotificationToken != null)
			{
				anotherSessionNotificationToken.Dispose();
			}

		}

        private void ReadWriteStream(Stream readStream, AudioToolbox.AudioFile audioFile)
		{
			int Length = 256;
			Byte[] buffer = new Byte[Length];
			int bytesRead = readStream.Read(buffer, 0, Length);
            // write the required bytes
            int errorCode = int.MinValue;
            int count = 0;
            while (bytesRead > 0)
			{
                
                audioFile.Write(Length*count, buffer, 0, 256, false, out errorCode);
				bytesRead = readStream.Read(buffer, 0, Length);
                count++;
			}
			readStream.Close();
            audioFile.Dispose();
		}

		protected override void ProcessVoiceRequest()
		{
			Log.Debug(TAG, "ProcessVoiceRequest");

			var baseUrl = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/";
			var relativeFileUrl = "jim.wav";
			var fullUrl = baseUrl + relativeFileUrl;
			var nsUrl = new NSUrl(fullUrl, false);

			var createdFile = AudioToolbox.AudioFile.Create(fullUrl, AudioToolbox.AudioFileType.WAVE, soundRecorder.AudioStreamDescription, AudioToolbox.AudioFileFlags.EraseFlags);
			ReadWriteStream(audioStream, createdFile);

			//FileStream writeStream = new FileStream(fullUrl, FileMode.Create, FileAccess.Write);
			//ReadWriteStream(audioStream,writeStream);


			var exists = File.Exists(fullUrl);
            if (exists)
            {
                SpeechToText(nsUrl);
            }
			else
			{
				Log.Error(TAG, "File doesn't exist.");
			}
		}

        private void SpeechToText(NSUrl nsUrl){
			var recognizer = new SFSpeechRecognizer();

			// Is the default language supported?
			if (recognizer == null)
				return;

			// Is recognition available?
			if (!recognizer.Available)
				return;


			var request = new SFSpeechUrlRecognitionRequest(nsUrl);
			recognizer.GetRecognitionTask(request, (SFSpeechRecognitionResult result, NSError err) =>
			{
				// Was there an error?
				if (err != null)
				{
					Log.Error(TAG, $"An error recognizing speech occurred: {err.LocalizedDescription}");
				}
				else
				{
                    // Update the user interface with the speech-to-text result.
                    if (result.Final)
                    {
                        Log.Debug("finalResult", result.BestTranscription.FormattedString);
                        var speechOutput =  new List<SpeechOutput>();
                        var transcripts = result.Transcriptions.ToList();
                        foreach(var transcript in transcripts){
                            var confidence = transcript.Segments.Average(x => x.Confidence);
                            var speech = new SpeechOutput(transcript.FormattedString, confidence);
                            speechOutput.Add(speech);
                        }

                        var aiRequest = new AIRequest(speechOutput);


                        var response = dataService.Request(aiRequest);
                        FireOnResult(response);
                    }
					Log.Debug("partial", result.BestTranscription.FormattedString);
				}

			});
		
        }

		protected override void StartVoiceRequest()
		{
			Log.Debug(TAG, "StartVoiceRequest");
			try
			{
				DoServiceRequest(audioStream);
			}
			catch (OperationCanceledException)
			{
				// Do nothing, because of request was cancelled in standard way
				Log.Debug(TAG, "StartVoiceRequest - OperationCancelled");
				new Task(OnListeningCancelled).Start();
			}
			catch (System.Exception e)
			{
				Log.Error(TAG, "StartVoiceRequest - Exception", e);
				FireOnError(new AIServiceException(e));
			}
		}

		#endregion


		private void OnAnotherSessionStarts(object sender, AVAudioSessionInterruptionEventArgs args)
		{
			if (soundRecorder != null && soundRecorder.Active)
			{
				if (args.InterruptionType == AVAudioSessionInterruptionType.Began)
				{
					// Cancel process, and do not start it again because of it means user start do something another
					// and will restart process if necessary
					Cancel();
				}
			}
		}

	}
}