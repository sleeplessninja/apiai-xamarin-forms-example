using System;
using Android.Content;
using ApiAi.Android;
using ApiAiFormsSDK.Interface;
using ApiAiSDK;

namespace ApiAiFormsSDK.Android
{
	public class AIServiceFactory : IAIServiceFactory
	{
        public Context Context { get; set; }

        public AIServiceFactory(Context context){
            Context = context;    
        }
		public AIService Get(AIConfiguration config)
		{
            if (Context == null)
                throw new Exception("Android Context cannot be null, please set before calling Get");
			return new SystemRecognitionService(Context, config);
		}
	}
}
