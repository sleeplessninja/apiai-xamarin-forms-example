using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ApiAiFormsSDK;
using ApiAiSDK.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ApiAiFormsSDKExample.ViewModels
{
    public class MainViewModel : SimpleViewModel
    {
        
        public ICommand StartListeningCommand { get; private set; }
        private AIService _aiService;

        public MainViewModel(AIService aiService)
        {
            _aiService = aiService;
            _aiService.AudioLevelChanged += _aiService_AudioLevelChanged;
            _aiService.OnResult += _aiService_OnResult;
            _aiService.OnError += _aiService_OnError;
            _aiService.ListeningStarted += _aiService_ListeningStarted;
            _aiService.ListeningFinished += _aiService_ListeningFinished;
            _aiService.ListeningCancelled += _aiService_ListeningCancelled;

            StartListeningCommand = new Command(StartListening);
        }

        private void StartListening(){
            _aiService.StartListening();
        }

		void _aiService_ListeningCancelled()
		{
            BackgroundColor = Color.White;
		}

		void _aiService_ListeningFinished()
		{
            BackgroundColor = Color.White;
		}

		void _aiService_ListeningStarted()
		{
            BackgroundColor = Color.Green;
		}

		void _aiService_AudioLevelChanged(float audioLevel)
		{
            SoundLevel = audioLevel;
		}

		void _aiService_OnError(ApiAiSDK.AIServiceException error)
		{
            InfoText = error.Message;
		}


        private void FailedResult(){
            InfoText = string.Format("Could not understand what you said. Please try again");
        }

		void _aiService_OnResult(AIResponse response)
		{
            InfoText = response.JsonResponseString;
		}

        private string _infoText = "Press Start Listening Button Below to Record a submit a Voice Response ";
        public string InfoText{
            get { return _infoText; }
            set{
                if (_infoText == value) return;
                _infoText = value;
                RaisePropertyChanged();
            }
        }

        private double _soundLevel = 0;
        public double SoundLevel{
            get { return _soundLevel; }
            set{
                if (_soundLevel == value) return;
                _soundLevel = value;
                RaisePropertyChanged();
            }
        }

        private Color _backgroundColor = Color.White;
        public Color BackgroundColor{
            get { return _backgroundColor; }
            set{
                if (_backgroundColor == value) return;
                _backgroundColor = value;
                RaisePropertyChanged();
            }
        }

    }
}
