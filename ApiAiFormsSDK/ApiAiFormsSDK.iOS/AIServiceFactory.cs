using System;
using ApiAiFormsSDK.Interface;
using ApiAiSDK;

namespace ApiAiFormsSDK.iOS
{
    public class AIServiceFactory : IAIServiceFactory
    {

        public AIService Get(AIConfiguration config)
        {
            return new SpeaktoitRecognitionService(config);
        }
    }
}
