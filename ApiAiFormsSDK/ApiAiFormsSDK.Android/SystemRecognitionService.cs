﻿//
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
using Android.Speech;
using Android.OS;
using Android.Content;
using ApiAiSDK.Model;
using System.Threading.Tasks;
using System.Linq;
using ApiAiFormsSDK;

namespace ApiAi.Android
{
    public class SystemRecognitionService : AIService
    {
        private readonly string TAG = typeof(SystemRecognitionService).Name;

        private SpeechRecognizer speechRecognizer;
        private readonly object speechRecognizerLock = new object();

        private volatile bool recognitionActive = false;
        private volatile bool wasReadyForSpeech = false;

        private readonly Handler handler;

        private Context context;

        private RequestExtras requestExtras;

        public SystemRecognitionService(Context context, AIConfiguration config)
            : base(config)
        {
            this.context = context;
            handler = new Handler(Looper.MainLooper);
        }

        protected void InitializeRecognizer()
        {
            lock (speechRecognizerLock)
            {
                if (speechRecognizer != null)
                {
                    speechRecognizer.Destroy();
                    speechRecognizer = null;
                }

                speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(context);

                speechRecognizer.ReadyForSpeech += SpeechRecognizer_ReadyForSpeech;
                speechRecognizer.RmsChanged += SpeechRecognizer_RmsChanged;            
                speechRecognizer.EndOfSpeech += SpeechRecognizer_EndOfSpeech;

                speechRecognizer.Results += SpeechRecognizer_Results;
                speechRecognizer.Error += SpeechRecognizer_Error;

            }
        }

        void SpeechRecognizer_Results(object sender, ResultsEventArgs e)
        {
            if (recognitionActive)
            {
                recognitionActive = false;

                var recognitionResults = e.Results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);

                float[] rates = null;


                if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich)
                {
                    rates = e.Results.GetFloatArray(SpeechRecognizer.ConfidenceScores);
                }

                if (recognitionResults == null || recognitionResults.Count == 0)
                {
                    // empty response
                    FireOnResult(new AIResponse());
                }
                else
                {
                    var aiRequest = new AIRequest();
                    if (rates != null)
                    {
                        aiRequest.Query = recognitionResults.ToArray();
                        aiRequest.Confidence = rates;
                    }
                    else
                    {
                        aiRequest.Query = new [] { recognitionResults[0] };
                    }
 
                    if (requestExtras != null)
                    {
                        requestExtras.CopyTo(aiRequest);
                    }

                    SendRequest(aiRequest);
                    ClearRecognizer();
                }
            }
        }

        void SpeechRecognizer_Error(object sender, ErrorEventArgs e)
        {
            if (e.Error == SpeechRecognizerError.NoMatch && !wasReadyForSpeech)
            {
                Log.Debug(TAG, "Trying to restart recognition " + e.Error);
                RestartRecognition();
                return;
            }

            Log.Debug(TAG, "SpeechRecognizer_Error " + e.Error);

            if (recognitionActive)
            {
                var errorMessage = "Speech recognition engine error: " + e.Error;
                FireOnError(new AIServiceException(errorMessage));
            }

            recognitionActive = false;
        }

        void SpeechRecognizer_EndOfSpeech(object sender, EventArgs e)
        {
            OnListeningFinished();
        }

        void SpeechRecognizer_RmsChanged(object sender, RmsChangedEventArgs e)
        {
            OnAudioLevelChanged(e.RmsdB);
        }

        void SpeechRecognizer_ReadyForSpeech(object sender, ReadyForSpeechEventArgs e)
        {
            Log.Debug(TAG, "SpeechRecognizer_ReadyForSpeech");
            wasReadyForSpeech = true;
            if (recognitionActive)
            {
                OnListeningStarted();
            }
        }

        protected void ClearRecognizer()
        {
            if (speechRecognizer != null)
            {
                lock (speechRecognizerLock)
                {
                    if (speechRecognizer != null)
                    {
                        speechRecognizer.Destroy();
                        speechRecognizer = null;
                        requestExtras = null;
                    }
                }
            }
        }

        private void SendRequest(AIRequest request)
        {
            new Task(() =>
                {
                    try
                    {
                        var response = dataService.Request(request);
                        FireOnResult(response);
                    }
                    catch (Exception e)
                    {
                        FireOnError(new AIServiceException(e));
                    }
                }
            ).Start();
        }

        #region implemented abstract members of AIService

        public override void StartListening(RequestExtras requestExtras = null)
        {
            if (!recognitionActive)
            {
                this.requestExtras = requestExtras;

                var sttIntent = CreateRecognitionIntent();

                RunInUIThread(() =>
                    {
                        InitializeRecognizer();
                        wasReadyForSpeech = false;
                        speechRecognizer.StartListening(sttIntent);
                        recognitionActive = true;
                    });

            }
            else
            {
                Log.Warn(TAG, "Trying to start recognition while another recognition active");
                if (!wasReadyForSpeech)
                {
                    Cancel();
                }
            }
        }

        public Intent CreateRecognitionIntent()
        {
            var sttIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            sttIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            var language = config.Language.code.Replace('-', '_');
            sttIntent.PutExtra(RecognizerIntent.ExtraLanguage, language);
            sttIntent.PutExtra(RecognizerIntent.ExtraLanguagePreference, language);
            // WORKAROUND for https://code.google.com/p/android/issues/detail?id=75347
            // TODO Must be removed after fix in Android
            sttIntent.PutExtra("android.speech.extra.EXTRA_ADDITIONAL_LANGUAGES", new String[] { });
            return sttIntent;
        }

        public override void StopListening()
        {
            if (recognitionActive)
            {
                RunInUIThread(() =>
                    {
                        lock (speechRecognizerLock)
                        {
                            if (recognitionActive)
                            {
                                Log.Debug(TAG, "Stop listening");
                                speechRecognizer.StopListening();
                            }
                        }
                    });
            }
            else
            {
                Log.Warn(TAG, "Trying to stop listening while not active recognition");
            }
        }

        public override void Cancel()
        {
            if (recognitionActive)
            {
                RunInUIThread(() =>
                    {
                        lock (speechRecognizerLock)
                        {
                            if (recognitionActive)
                            {
                                Log.Debug(TAG, "Cancel listening");
                                speechRecognizer.Cancel();
                                new Task(OnListeningCancelled).Start();
                                recognitionActive = false;
                                requestExtras = null;
                            }
                        }
                    });
            }
        }

        /// <summary>
        /// Method to deal with problem in Google Recognition service 
        /// http://stackoverflow.com/questions/31071650/speechrecognizer-throws-onerror-on-the-first-listening
        /// </summary>
        private void RestartRecognition()
        {
            recognitionActive = false;
            lock (speechRecognizerLock)
            {
                try
                {
                    if (speechRecognizer != null) 
                    {
                        speechRecognizer.Cancel();

                        wasReadyForSpeech = false;
                        var intent = CreateRecognitionIntent();
                        speechRecognizer.StartListening(intent);
                        recognitionActive = true;
                    }    
                }
                catch (Exception)
                {
                    StopListening();
                }
            }
        }

        #endregion

        public override void Pause()
        {
            ClearRecognizer();
        }

        private void RunInUIThread(Action a)
        {
            handler.Post(a);
        }
    }
}

