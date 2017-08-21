using System;
using ApiAiSDK.Model;

namespace AiApiSdk.Model
{
    public class AiApiResponseEventArgs : EventArgs
    {

        public AiApiResponse Response { get; private set; }

        public AiApiResponseEventArgs(AiApiResponse response)
        {
            Response = response;
        }
    }
}